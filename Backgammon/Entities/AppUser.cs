using Microsoft.AspNetCore.Identity;

namespace Backgammon.Entities
{
    public class AppUser : IdentityUser<int>
    {
        public string Name { get; set; }
        public string SurName { get; set; }
        public string ImagePath { get; set; }
        public string Gender { get; set; }
        public string Club { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDate { get; set; }
        public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
        public ICollection<Tour> Tours { get; set; } = new List<Tour>();

        public ICollection<TournamentUser> TournamentUsers { get; set; }= new List<TournamentUser>();

    }
}
