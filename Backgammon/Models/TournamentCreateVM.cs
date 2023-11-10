using Backgammon.Entities;

namespace Backgammon.Models
{
    public class TournamentCreateVM
    {

        public Tournament Tournament { get; set; }
        public List<AppUser>? Users { get; set; }

    }
}
