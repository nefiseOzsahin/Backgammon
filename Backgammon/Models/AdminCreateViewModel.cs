using System.ComponentModel.DataAnnotations;

namespace Backgammon.Models
{
    public class AdminCreateViewModel
    {


        [Required(ErrorMessage = "Kullanıcıadı gereklidir.")]
        public string Username { get; set; }

    
        [Required(ErrorMessage = "Parola gereklidir.")]
        public string Password { get; set; }

    }
}
