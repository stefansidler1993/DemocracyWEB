﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Democracy2.Models
{
    public class VotingDetail
    {

        [Key]
        public int VotingDetailId { get; set; }

        public DateTime DateTime { get; set; }

        public int VotingID { get; set; }

        public int UserId { get; set; }

        public int CandidateId { get; set; }


        public virtual Voting Voting { get; set; }

        public virtual User User { get; set; }

        public virtual Candidate Candidate { get; set; }
    }
}
