namespace Backgammon.Models
{
    public class ScoreViewModel
    {
        public int PairId { get; set; }
        public int User1Score { get; set; }
        public int User2Score { get; set; }

        public string? User1Name { get; set; }
        public string? User2Name { get; set; }

        public string? User1PhoneNumber { get; set; }
        public string? User2PhoneNumber { get; set; }

        public int TournamentId { get; set; }

        public int TourId { get; set; }
    }
}
