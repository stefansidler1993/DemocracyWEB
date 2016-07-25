using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Democracy2.Models;
using System.Data.SqlClient;
using CrystalDecisions.CrystalReports.Engine;
using System.Configuration;

namespace Democracy2.Controllers
{

    public class VotingsController : Controller
    {
        private DemocracyContext db = new DemocracyContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Close(int id)
        {
            var voting = db.Votings.Find(id);
            if (voting != null)
            {
                var candidate = db.Candidates
                    .Where(c => c.VotingId == voting.VotingId)
                    .OrderByDescending(c => c.QuantityVotes)
                    .FirstOrDefault();
                if (candidate != null)
                {
                    var state = this.GetState("Closed");
                    voting.StateId = state.StateId;
                    voting.CandidateWinId = candidate.User.UserId;
                    db.Entry(voting).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index");
        }




        public ActionResult ShowResults(int id)
        {
            var report = this.GenerateResultReport(id);
            var stream = report.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            return File(stream, "application/pdf");

        }

        private ReportClass GenerateResultReport(int id)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            var dataTable = new DataTable();
            //@ from not concatenate
            var sql = @"SELECT  Votings.VotingId, Votings.Description AS Voting, States.Descripcion AS State, 
                                Users.FirstName + ' ' + Users.LastName AS Candidate, Candidates.QuantityVotes
                         FROM   Candidates INNER JOIN
                                Users ON Candidates.UserId = Users.UserId INNER JOIN
                                Votings ON Candidates.VotingId = Votings.VotingId INNER JOIN
                                States ON Votings.StateId = States.StateId
                          WHERE Votings.VotingId =" + id;

            try
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                // full data table
                var adapter = new SqlDataAdapter(command);
                adapter.Fill(dataTable);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            var report = new ReportClass();
            report.FileName = Server.MapPath("/Reports/Results.rpt");
            report.Load();
            report.SetDataSource(dataTable);
            return report;
        }



        [Authorize(Roles = "User")]
        public ActionResult Results()
        {
            var state = this.GetState("Closed");
            var votings = db.Votings
                .Where(v => v.StateId == state.StateId)
                .Include(v => v.State);
            var views = new List<VotingIndexView>();
            var db2 = new DemocracyContext();

            //Winner
            foreach (var voting in votings)
            {
                User user = null;
                if (voting.CandidateWinId != 0)
                {
                    user = db2.Users.Find(voting.CandidateWinId);
                }

                views.Add(new VotingIndexView
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnableBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = user,

                });
            }
            return View(views);


        }


        [Authorize(Roles = "User")]
        public ActionResult VoteForCandidate(int candidateId, int votingId)
        {
            // validation of user,candidate,
            var user = db.Users
                .Where(u => u.UserName == this.User.Identity.Name)
                .FirstOrDefault();

            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var candidate = db.Candidates.Find(candidateId);

            if (candidate == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var voting = db.Votings.Find(votingId);

            if (voting == null)
            {
                return RedirectToAction("Index", "Home");
            }

            //validation if vote
            if (this.VoteCandidate(user, candidate, voting))
            {
                return RedirectToAction("MyVotings");
            }

            return RedirectToAction("Index", "Home");
        }

        private bool VoteCandidate(
            Models.User user,
            Candidate candidate,
            Voting voting)
        {
            //transaction
            using (var transaction = db.Database.BeginTransaction())
            {
                var votingDetail = new VotingDetail
                {
                    CandidateId = candidate.CandidateId,
                    DateTime = DateTime.Now,
                    UserId = user.UserId,
                    VotingID = voting.VotingId,
                };

                db.VotingDetails.Add(votingDetail);

                //add one vote
                candidate.QuantityVotes++;
                //register modified
                db.Entry(candidate).State = EntityState.Modified;

                voting.QuantityVotes++;
                db.Entry(voting).State = EntityState.Modified;

                //savechanges
                try
                {
                    db.SaveChanges();
                    //confirm transaction
                    transaction.Commit();
                    return true;
                }
                catch (Exception)
                {
                    //desace of one,two
                    transaction.Rollback();
                }
            }
            return false;
        }



        [Authorize(Roles = "User")]
        public ActionResult Vote(int votingId)
        {
            var voting = db.Votings.Find(votingId);
            var view = new VotingVoteView
            {

                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnableBlankVote = voting.IsEnableBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                MyCandidate = voting.Candidates.ToList(),
                Remarks = voting.Remarks,
                VotingId = voting.VotingId,
            };

            return View(view);
        }


        [Authorize(Roles = "User")]
        public ActionResult MyVotings()
        {
            // search user login
            var user = db.Users
                .Where(u => u.UserName == this.User.Identity.Name)
                .FirstOrDefault();

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "There an error with the current user, call the support");
                return View();
            }

            //Get event voting for the correct time 
            var state = this.GetState("Open");

            var votings = db.Votings
                .Where(v => v.StateId == state.StateId &&
                v.DateTimeStart <= DateTime.Now &&
                v.DateTimeEnd >= DateTime.Now)
                .Include(v => v.Candidates)
                .Include(v => v.VotingGroups)
                .Include(v => v.State)
                .ToList();


           //Discard the events in wich the user already voted 
            foreach (var voting in votings.ToList() )
            {
                

                var votingDetail = db.VotingDetails.
                    Where(vd => vd.VotingID == voting.VotingId &&
                    vd.UserId == user.UserId).FirstOrDefault();

                if (votingDetail != null)
                {
                    votings.Remove(voting);
                }

            }

            //Discard events by groups in wich the users are not included
            foreach (var voting in votings.ToList())
            {

                if (!voting.IsForAllUsers)
                {
                    bool userBelongsToGroup = false;

                    foreach (var votingGroup in voting.VotingGroups)
                    {
                        var userGroup = votingGroup.Group.GroupMember.
                            Where(gm => gm.UserId == user.UserId)
                            .FirstOrDefault();

                        if (userGroup != null)
                        {
                            userBelongsToGroup = true;
                            break;
                        }
                    }

                    if (!userBelongsToGroup)
                    {

                        votings.Remove(voting);
                    }


                }
            }

           

            return View(votings);
        }



        private State GetState(string stateName)
        {
            var state = db.States.Where(s => s.Descripcion == stateName)
                .FirstOrDefault();
            if (state == null)
            {
                state = new State
                {
                    Descripcion = stateName,
                };

                db.States.Add(state);
                db.SaveChanges();
            }

            return state;
        }

      


        //Get AddCandidate

    [Authorize(Roles = "Admin")]
        public ActionResult AddCandidate(int id)
        {
            var view = new AddCandidateView
            {
                VotingId = id,
            };

            ViewBag.UserId = new SelectList(
                db.Users.OrderBy(u => u.FirstName)
               .ThenBy(u => u.LastName),
               "UserId",
               "FullName");

            return View(view);
        }

        //Post AddCandidate
        [HttpPost]
        public ActionResult AddCandidate(AddCandidateView view)
        {
            if (ModelState.IsValid)
            {
                //no meter 2 veces un candidato
                var candidate = db.Candidates
                    .Where(c => c.VotingId == view.VotingId &&
                                c.UserId == view.UserId)
                                .FirstOrDefault();

                if (candidate != null)
                {
                    //another way error
                    ModelState.AddModelError(string.Empty, "The candidate already belongs to voting");
                    //ViewBag.Error = "The group already belongs to voting";
                    ViewBag.UserId = new SelectList(
                                    db.Users.OrderBy(u => u.FirstName)
                                    .ThenBy(u => u.LastName),
                                    "UserId",
                                    "FullName");
                    return View(view);
                }

                candidate = new Candidate
                {
                    UserId = view.UserId,
                    VotingId = view.VotingId,
                };


                db.Candidates.Add(candidate);
                db.SaveChanges();
                return RedirectToAction(string.Format("Details/{0}", view.VotingId));
            }

            ViewBag.UserId = new SelectList(
                db.Users.OrderBy(u => u.FirstName)
               .ThenBy(u => u.LastName),
               "UserId",
               "FullName");
            return View(view);

        }

       // [Authorize(Roles = "Admin")]
        //Delete Candidate

         [Authorize(Roles = "Admin")]
        public ActionResult DeleteCandidate(int id)
        {
            var candidate = db.Candidates.Find(id);

            if (candidate != null)
            {
                db.Candidates.Remove(candidate);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", candidate.VotingId));
        }


        //Get AddGroup

        [Authorize(Roles = "Admin")]
        public ActionResult AddGroup(int id)
        {
            ViewBag.GroupId = new SelectList(
             db.Groups.OrderBy(g => g.Descripcion),
            "GroupId",
            "Descripcion");

            var view = new AddGroupView
            {
                VotingId = id,
            };
            return View(view);

        }

        //Post AddGroup
        [HttpPost]
        public ActionResult AddGroup(AddGroupView view)
        {

            if (ModelState.IsValid)
            {
                //search in db
                var votingGroup = db.VotingGroups
                    .Where(vg => vg.VotingId == view.VotingId &&
                                 vg.Groupid == view.GroupId)
                                 .FirstOrDefault();

                if (votingGroup != null)
                {
                    //another way error
                    ModelState.AddModelError(string.Empty, "The group already belongs to voting");
                    //ViewBag.Error = "The group already belongs to voting";
                    ViewBag.GroupId = new SelectList(
                                db.Groups.OrderBy(g => g.Descripcion),
                                "GroupId",
                                "Descripcion");
                    return View(view);
                }


                votingGroup = new VotingGroup
                {
                   Groupid= view.GroupId,
                    VotingId = view.VotingId,
                };

                db.VotingGroups.Add(votingGroup);
                db.SaveChanges();
                return RedirectToAction(string.Format("Details/{0}", view.VotingId));
            }

            ViewBag.GroupId = new SelectList(
                           db.Groups.OrderBy(g => g.Descripcion),
                           "GroupId",
                           "Descripcion");
            return View(view);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult DeleteGroup(int id)
        {
            var votingGroup = db.VotingGroups.Find(id);

            if (votingGroup != null)
            {
                db.VotingGroups.Remove(votingGroup);
                db.SaveChanges();
            }

            return RedirectToAction(string.Format("Details/{0}", votingGroup.VotingId));
        }

        // GET: /Votings/

        [Authorize(Roles = "Admin")]
        // GET: /Votings/
        public ActionResult Index()
        {
            var votings = db.Votings.Include(v => v.State);
            var views = new List<VotingIndexView>();
            var db2 = new DemocracyContext();

            //Winner
            foreach (var voting in votings)
            {
                User user = null;
                if (voting.CandidateWinId != 0)
                {
                    user = db2.Users.Find(voting.CandidateWinId);
                }

                views.Add(new VotingIndexView
                {
                    CandidateWinId = voting.CandidateWinId,
                    DateTimeEnd = voting.DateTimeEnd,
                    DateTimeStart = voting.DateTimeStart,
                    Description = voting.Description,
                    IsEnableBlankVote = voting.IsEnableBlankVote,
                    IsForAllUsers = voting.IsForAllUsers,
                    QuantityBlankVotes = voting.QuantityBlankVotes,
                    Remarks = voting.Remarks,
                    StateId = voting.StateId,
                    State = voting.State,
                    VotingId = voting.VotingId,
                    Winner = user,

                });
            }
            return View(views);
        }


        // GET: /Votings/Details/5

        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }

            var view = new DetailsVotingView
            {
                Candidates = voting.Candidates.ToList(),
                CandidateWinId = voting.CandidateWinId,
                DateTimeEnd = voting.DateTimeEnd,
                DateTimeStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnableBlankVote = voting.IsEnableBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                QuantityBlankVotes = voting.QuantityBlankVotes,
                QuantityVotes = voting.QuantityVotes,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                VotingGroups = voting.VotingGroups.ToList(),
                VotingId = voting.VotingId,
            };

            

          
         

            return View(view);
        }

        // GET: /Votings/Create

    [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.StateId = new SelectList(db.States, "StateId", "Descripcion");

            //DateTime
            var view = new VotingView
            {
                DateStart = DateTime.Now,
                DateEnd = DateTime.Now,
            };
           


            return View();
        }

        // POST: /Votings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( VotingView view)
        {
            if (ModelState.IsValid)
            {
                //DateTime
                var voting = new Voting
                {
                    DateTimeEnd = view.DateEnd
                                  .AddHours(view.TimeEnd.Hour)
                                  .AddMinutes(view.TimeEnd.Minute),
                    DateTimeStart = view.DateStart
                                  .AddHours(view.TimeStart.Hour)
                                  .AddMinutes(view.TimeStart.Minute),
                    Description = view.Description,
                    IsEnableBlankVote = view.IsEnabledBlankVote,
                    IsForAllUsers = view.IsForAllUsers,
                    Remarks = view.Remarks,
                    StateId = view.StateId,
                };



                db.Votings.Add(voting);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.StateId = new SelectList(db.States, "StateId", "Descripcion", view.StateId);
            return View(view);
        }

        // GET: /Votings/Edit/5

    [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var voting = db.Votings.Find(id);

            if (voting == null)
            {
                return HttpNotFound();
            }

            //DateTime
            var view = new VotingView
            {
                DateEnd = voting.DateTimeEnd,
                DateStart = voting.DateTimeStart,
                Description = voting.Description,
                IsEnabledBlankVote = voting.IsEnableBlankVote,
                IsForAllUsers = voting.IsForAllUsers,
                Remarks = voting.Remarks,
                StateId = voting.StateId,
                TimeEnd = voting.DateTimeEnd,
                TimeStart = voting.DateTimeStart,
                VotingId = voting.VotingId,
            };

            ViewBag.StateId = new SelectList(db.States, "StateId", "Descripcion", voting.StateId);
            return View(view);


        }

        // POST: /Votings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(VotingView view)
        {
            if (ModelState.IsValid)
            {
                //DateTime
                var voting = new Voting
                {
                    DateTimeEnd = view.DateEnd
                                  .AddHours(view.TimeEnd.Hour)
                                  .AddMinutes(view.TimeEnd.Minute),
                    DateTimeStart = view.DateStart
                                  .AddHours(view.TimeStart.Hour)
                                  .AddMinutes(view.TimeStart.Minute),
                    Description = view.Description,
                    IsEnableBlankVote = view.IsEnabledBlankVote,
                    IsForAllUsers = view.IsForAllUsers,
                    Remarks = view.Remarks,
                    StateId = view.StateId,
                    VotingId = view.VotingId,
                };

                db.Entry(voting).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StateId = new SelectList(db.States, "StateId", "Descripcion", view.StateId);
            return View(view);
        }

        // GET: /Votings/Delete/5

    [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Voting voting = db.Votings.Find(id);
            if (voting == null)
            {
                return HttpNotFound();
            }
            return View(voting);
        }

        // POST: /Votings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Voting voting = db.Votings.Find(id);
            db.Votings.Remove(voting);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
