using System.ComponentModel.DataAnnotations;

namespace Backgammon.Entities
{
    public class Tournament
    {
        public int Id { get; set; }    
        public string Name { get; set; }
        [Required(ErrorMessage = "Başlama zamanı gereklidir.")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "Turnuva konumu gereklidir.")]
        public string Place { get; set; }
        public string System { get; set; }
        [Required(ErrorMessage ="Turnuva tipi gereklidir.")]
        public string Type { get; set; }
        [Required(ErrorMessage = "Oynama hakkı gereklidir.")]
        public int PlayLife { get; set; }
        [Required(ErrorMessage = "Bye Type gereklidir.")]
        public string ByeType { get; set; }
        public int TableStart { get; set; } 
        public DateTime? CreateDate { get; set; }
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    }
}
