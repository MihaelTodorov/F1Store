using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Core.Contacts;
using F1Store.Infrastructure.Data;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Services
{
    public class TeamService : ITeamService
    {
        private readonly ApplicationDbContext _context;

        public TeamService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Team GetTeamById(int teamId)
        {
            return _context.Teams.Find(teamId);
        }

        public List<Team> GetTeams()
        {
            List<Team> teams = _context.Teams.ToList();
            return teams;
        }

        public List<Product> GetProductsByTeam(int teamId)
        {
            return _context.Products
                .Where(x => x.TeamId == teamId)
                .ToList();
        }
    }
}


























//secret