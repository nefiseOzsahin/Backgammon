namespace Backgammon.Entities
{
    public class TournamentChampion
    {
        public int Id { get; set; }

        public int TournamentId { get; set; }
        public Tournament Tournament { get; set; }

        public int ChampionId { get; set; }
        public AppUser Champion { get; set; }
    }
}
