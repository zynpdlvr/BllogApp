using System.ComponentModel.DataAnnotations;
using BlogApp.Entity;

namespace BlogApp.Models{

    public class RegisterViewModel{

        [Required]
        [Display(Name = "UserName")]
        public string? UserName {get;set;}

        [Required]
        [Display(Name = "Name")]
        public string? Name {get;set;}

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email {get;set;}

        [Required]
        [StringLength(10, ErrorMessage = "{0} field emust be at least {2} at most {1} characters in length.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password {get;set;}
        
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword {get;set;}
    }
}