﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{
    public class AddMemberView
    {

         [Required(ErrorMessage = "You must select an user")]
        public int UserId { get; set; }

        public int GroupId { get; set; }
    }
}