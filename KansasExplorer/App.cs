using ExplorerLibrary.Services;

namespace KansasExplorer
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
