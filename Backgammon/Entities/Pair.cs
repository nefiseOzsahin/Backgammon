namespace Backgammon.Entities
{
    public class Pair
    {

        public int Id { get; set; }
        public int User1Id { get; set; }
        public int User2Id { get; set;}
        public int User1Score { get; set; }
        public int User2Score { get; set;}
        public bool IsBye { get; set; }

        public int TourId { get; set; }
        public Tour Tour { get; set; }

        public int TableNo { get; set; }

    }
}
