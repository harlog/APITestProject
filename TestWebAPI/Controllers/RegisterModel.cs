using System.ComponentModel.DataAnnotations;

namespace TestWebAPI.Controllers
{
    public class RegisterModel
    {
        [Required]
        public string name { get; set; }

        public string id { get; set; }

        [Required]
        public string email { get; set; }

        [Required]
        public string password { get; set; }
    }
}