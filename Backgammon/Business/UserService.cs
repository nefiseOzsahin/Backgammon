using Backgammon.Context;
using Backgammon.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Backgammon.Business
{
    public class UserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly BackgammonContext _context;

        public UserService(UserManager<AppUser> userManager, BackgammonContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<AppUser>> GetNonAdminUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var nonAdminUsers = new List<AppUser>();
            var adminRole = "Admin"; // Change to your actual admin role name

            foreach (var user in users)
            {
                var isInAdminRole = await _userManager.IsInRoleAsync(user, adminRole);

                if (!isInAdminRole)
                {
                    nonAdminUsers.Add(user);
                }
            }

            return nonAdminUsers;
        }

        public async Task<List<AppUser>> GetNonAdminUsersOfATournamentAsync(int tournamentId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Users) // Assuming there's a navigation property "Users" in your Tournament entity
                .FirstOrDefaultAsync(tu => tu.Id == tournamentId);


            if (tournament != null)
            {
                var nonAdminUsers = new List<AppUser>();
                var adminRole = "Admin"; // Change to your actual admin role name

                foreach (var user in tournament.Users)
                {
                    var isInAdminRole = await _userManager.IsInRoleAsync(user, adminRole);

                    if (!isInAdminRole)
                    {
                        nonAdminUsers.Add(user);
                    }
                }

                return nonAdminUsers;
            }

            return null;
        }
    }
}
