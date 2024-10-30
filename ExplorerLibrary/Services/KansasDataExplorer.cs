using ExplorerLibrary.Models;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace ExplorerLibrary.Services
{
    public class KansasDataExplorer : IDataExplorer
    {
        private readonly string wellBoreUrl = @"https://www.kgs.ku.edu/PRS/Ora_Archive/ks_wells.zip";
        private readonly ILogger<KansasDataExplorer> _log;
        private readonly IDataAccess _da;
        private readonly string stateId = "KS";

        public KansasDataExplorer(ILogger<KansasDataExplorer> log, IDataAccess da)
        {
            _log = log;
            _da = da;
        }
        public async Task<List<StateInfo>> GetExplorerData(string connectionString)
        {
            List<StateInfo> info = new List<StateInfo>();

            using (HttpClient client = new HttpClient())
            {
                byte[] zipData = await client.GetByteArrayAsync(wellBoreUrl);
                using (MemoryStream zipStream = new MemoryStream(zipData))
                {
                    using (ZipArchive archive = new ZipArchive(zipStream))
                    {
                        if (archive != null)
                        {
                            _log.LogInformation("Data has been downloaded from Kansas website");
                            if (archive.Entries.Count > 0)
                            {
                                ZipArchiveEntry? entry = archive.GetEntry("ks_wells.txt");
                                if (entry != null)
                                {
                                    _log.LogInformation("Start processing data");
                                    int count = WellBoreCount(entry);
                                    IEnumerable<StateInfo> result = await _da.ReadData<IEnumerable<StateInfo>>("", connectionString);
                                    info = result.ToList();
                                    StateInfo stateInfo = info.SingleOrDefault(s => s.StateId == stateId);
                                    if (stateInfo == null)
                                    {
                                        StateInfo newState = new StateInfo();
                                        newState.StateId = stateId;
                                        newState.StateFullName = "Colorado";
                                        newState.WellCount = $"count";
                                        newState.LoaderSource = wellBoreUrl;
                                        info.Add(newState);
                                    }
                                    else
                                    {
                                        stateInfo.WellCount = $"{count}";
                                    }
                                    await _da.SaveData(connectionString, info, "");
                                }
                                else
                                {
                                    _log.LogError("File not found in the zip archive.");
                                }
                            }
                            else
                            {
                                _log.LogError("The zip archive is empty.");
                            }
                        }
                        else
                        {
                            _log.LogError("Failed to create the ZipArchive.");
                        }
                    }
                }
            }
            
            return info;
        }

        public int WellBoreCount(ZipArchiveEntry entry)
        {
            int count = 0;

            using (Stream entryStream = entry.Open())
            using (StreamReader reader = new StreamReader(entryStream))
            {
                while (reader.ReadLine() != null)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
