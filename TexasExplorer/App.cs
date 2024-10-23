using ExplorerLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TexasExplorer
{
    public class App
    {
        private readonly IDataExplorer _dataExplorer;

        public App(IDataExplorer dataExplorer)
        {
            _dataExplorer = dataExplorer;
        }

        public async Task Run(string path, string connectionString)
        {
            await _dataExplorer.GetExplorerData(connectionString);
        }
    }
}
