using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class State
    {
        [Key]
        // gets an sets the Statesid
        public int StateId { get; set; }

        [Required (ErrorMessage= "The field {0} is required")]
        [StringLength(50, ErrorMessage = "The field {0} can contain maximum {1} and minimum {2} characters", MinimumLength = 3)]

        // gets an sets the states description
        [Display(Name = "State Description")]
        public String Descripcion { get; set; }

        public virtual ICollection<Voting> Votings { get; set; }
    }
}