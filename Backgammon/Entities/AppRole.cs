using Microsoft.AspNetCore.Identity;

namespace Backgammon.Entities
{
    public class AppRole:IdentityRole<int>
    {
        public DateTime CreatedTime { get; set; }
    }
}
