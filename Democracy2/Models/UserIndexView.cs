using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    //not bd, class or atribute
    [NotMapped]
    public class UserIndexView : User
    {
        //admin
        [Display(Name = "Is Admin?")]
        public bool IsAdmin { get; set; }

    }
}