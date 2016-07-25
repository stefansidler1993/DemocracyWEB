using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class AddGroupView
    {


      
        public int VotingId{ get; set; }

        [Required(ErrorMessage = "You must select group")]
        public int GroupId { get; set; }

    }
}