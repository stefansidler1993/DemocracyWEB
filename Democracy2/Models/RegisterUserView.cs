﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Democracy2.Models
{   // RegisterUserView inheritates from Userview, because it has similar attributes that are needed
    //for users registration, besides UserView don't go to persistency, and neither registerUserView
    public class RegisterUserView: UserView
    {
        [Required(ErrorMessage = "The field {0} is required")]
        [StringLength(20, ErrorMessage =
            "The field {0} can contain maximum {1}and minimum {2} characters",
            MinimumLength = 8)]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(20, ErrorMessage =
            "El campo {0} puede contener maximo {1} y minimo {2} caracteres",
            MinimumLength = 8)]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The passwords don't match")]
        public string ConfirmPassword { get; set; }

    }
}