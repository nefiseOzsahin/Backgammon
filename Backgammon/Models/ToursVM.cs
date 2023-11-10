using Backgammon.Entities;

namespace Backgammon.Models
{
    public class ToursVM
    {

        public Tournament? Tournament { get; set; }
        public List<PairVM>? PairVMs { get; set; }
        public List<ScoreViewModel> Scores { get; set; }
    }
}
