namespace Backgammon.Business
{
    public class BackgammonPairing
    {
        private readonly Dictionary<string, HashSet<string>> _matchupHistory;

        public BackgammonPairing(List<string> users)
        {
            _matchupHistory = new Dictionary<string, HashSet<string>>();
            foreach (var user in users)
            {
                _matchupHistory[user] = new HashSet<string>();
            }
        }

        public Dictionary<string, HashSet<string>> MatchupHistory => _matchupHistory;
    }

}
