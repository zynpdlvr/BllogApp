using System.ComponentModel.DataAnnotations;
using BlogApp.Entity;
using System.Security.Claims;

namespace BlogApp.Models{

    public class PostCreateViewModel{

        public int PostId {get;set;}
        [Required]
        [Display(Name = "Title")]
        public string? Title {get;set;}
        
        [Required]
        [Display(Name = "Description")]
        public string? Description {get;set;}

        [Required]
        [Display(Name = "Content")]
        public string? Content {get;set;}

        [Required]
        [Display(Name = "Url")]
        public string? Url {get;set;}

        public bool IsActive {get;set;}
        public List<Tag> Tags {get;set;} = new();


    }
}