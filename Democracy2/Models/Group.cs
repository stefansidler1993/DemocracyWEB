using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class Group
    {
        [Key]
        // gets an sets the Groupid
        public int Groupid { get; set; }


        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(50, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 3)]

        // gets an sets the Group description
        public String Descripcion { get; set; }

        // part one of the relationship, it means a group can have many groupmembers
        public virtual ICollection<GroupMember>GroupMember { get; set; }

        public virtual ICollection<VotingGroup> VotingGroup { get; set; }



    }
}