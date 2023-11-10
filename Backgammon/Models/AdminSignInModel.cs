using System.ComponentModel.DataAnnotations;

namespace Backgammon.Models
{
    public class AdminSignInModel
    {
        [Required(ErrorMessage ="Kullanıcı adı gereklidir.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Parola gereklidir.")]
        public string Password { get; set; }
    }
}
