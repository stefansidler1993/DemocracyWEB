using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Democracy2.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Democracy2.Controllers.API
{
    [RoutePrefix ("api/Users")]
    public class UsersController : ApiController
    {
        private DemocracyContext db = new DemocracyContext();

        [HttpPost]
        [Route("Login")]
        public IHttpActionResult Login(JObject form)
        {

            dynamic jsonObject = form;
            var email = string.Empty;
            var password = string.Empty;

            try
            {
                email = jsonObject.email.Value;
            }
            catch{ }

            if (string.IsNullOrEmpty(email))
            {

                return (this.BadRequest("Incorrect Call Error 1"));
            }

            try
            {
                password = jsonObject.password.Value;
            }
            catch { }

            if (string.IsNullOrEmpty(password))
            {

                return (this.BadRequest("Incorrect Call Error 2"));
            }

            var userContext = new ApplicationDbContext();
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(userContext));

            var userASP = userManager.Find(email,password);

            if (userASP==null)
            {
                return (this.BadRequest("Incorrect user or password"));
            }

            var user = db.Users
                .Where(u=> u.UserName== email)
                .FirstOrDefault();

            if (user==null)
            {
                
                return (this.BadRequest("Problem, better call me"));

            }


            return this.Ok(user);
        }


        
        [HttpPut]
        public IHttpActionResult PutUser(int id, User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState + "Call me" );
            }

            if (id != user.UserId)
            {
                return BadRequest("Bad request callme now");
            }

            db.Entry(user).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return this.Ok(user);
        }

        // POST: api/Users
        [ResponseType(typeof(User))]
        public IHttpActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
            {
                // return BadRequest(ModelState);
                return BadRequest("Bad Request call skula");
            }

            db.Users.Add(user);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = user.UserId }, user);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(User))]
        public IHttpActionResult DeleteUser(int id)
        {
            User user = db.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok(user);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool UserExists(int id)
        {
            return db.Users.Count(e => e.UserId == id) > 0;
        }
    }
}