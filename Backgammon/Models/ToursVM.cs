using Backgammon.Entities;

namespace Backgammon.Models
{
    public class ToursVM
    {

        public Tournament? Tournament { get; set; }
        public List<PairVM>? PairVMs { get; set; }
        public List<ScoreViewModel> Scores { get; set; }

        public bool AllPairsHaveZeroScore { get; set; }

        public bool DrawLot { get; set; }
        public bool SaveScore { get; set; }
    }
}
