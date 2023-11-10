namespace Backgammon.Entities
{
    public class Tour
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int TournamentId { get; set; }

        public Tournament Tournament { get; set; }
        public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
        public ICollection<Pair> Pairs { get; set; } = new List<Pair>();

    }
}
