using System;
using System.ComponentModel.DataAnnotations;

namespace EImece.Models
{
    public class EditUserViewModel
    {
        public EditUserViewModel() { }

        // Allow Initialization with an instance of ApplicationUser:

        public string Id { get; set; }


        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }
        public virtual String Role { get; set; }
        //you might want to implement jobs too, if you want to display them in your index view
    }
}