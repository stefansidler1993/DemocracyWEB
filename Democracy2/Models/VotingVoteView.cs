using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Democracy2.Models
{
    

        [NotMapped]
        public class VotingVoteView : Voting
        {
            //list of Icollection
            public List<Candidate> MyCandidate { get; set; }
        }

    
}
