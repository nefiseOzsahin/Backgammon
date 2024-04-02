using System.ComponentModel.DataAnnotations;

namespace Backgammon.Models
{
    public class UserCreateViewModel
    {


        [Required(ErrorMessage = "İsim gereklidir.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyisim gereklidir.")]
        public string Surname { get; set; }

        [EmailAddress(ErrorMessage = "Lütfen bir Email formatı giriniz.")]
        [Required(ErrorMessage = "Mail gereklidir.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Telefon gereklidir.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Telefon numarası başında sıfır olmadan 10 rakamdan oluşmalıdır.")]
        public string Phone { get; set; }
        public string? ImagePath { get; set; }
        public string? Club { get; set; }
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Cinsiyet gereklidir.")]
        public string Gender { get; set; }
        [Required(ErrorMessage = "Ülke gereklidir.")]
        public string Country { get; set; }
        [Required(ErrorMessage = "Şehir gereklidir.")]
        public string City { get; set; }
    }
}
