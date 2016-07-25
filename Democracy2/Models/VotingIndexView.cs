using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Democracy2.Models
{
    [NotMapped]
    public class VotingIndexView : Voting
    {
        public User Winner { get; set; }
    }
}
