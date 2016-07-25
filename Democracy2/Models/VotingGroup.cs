using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class VotingGroup
    {

        [Key]
        public int VotingGroupId { get; set; }

        public int VotingId { get; set; }

        public int Groupid { get; set; }

        public virtual Voting Voting { get; set; }

        public virtual Group Group{ get; set; }

    }
}