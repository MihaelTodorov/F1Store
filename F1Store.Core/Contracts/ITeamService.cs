using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Contacts
{
    public interface ITeamService
    {
        List<Team> GetTeams();
        Team GetTeamById(int teamId);
        List<Product> GetProductsByTeam(int teamId);
    }
}
