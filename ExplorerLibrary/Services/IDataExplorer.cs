using ExplorerLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerLibrary.Services
{
    public interface IDataExplorer
    {
        Task<List<StateInfo>> GetExplorerData(string connectionString);
    }
}
