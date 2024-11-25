using Backgammon.Entities;

namespace Backgammon.Models
{
    public class PairVM
    {
        public int PairId { get; set; }

        public int TableNo { get; set; }
        public  Tour? Tour { get; set; }
        public AppUser? User1 { get; set; }
        public AppUser? User2 { get; set; }
    }
}
