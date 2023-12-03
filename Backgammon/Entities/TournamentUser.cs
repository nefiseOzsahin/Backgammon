namespace Backgammon.Entities
{
    public class TournamentUser
    {

        public int Id { get; set; }
        public int TournamentId { get; set; }

        public Tournament Tournament { get; set; }
        public int UserId { get; set; }
        public AppUser User { get; set; }

        public int LoseCount { get; set; }
        public int LifeCount { get; set; }

        public int WinCount { get; set; }

        public int ByeCount { get; set; }

        public int TourId { get; set; }

    }
}
