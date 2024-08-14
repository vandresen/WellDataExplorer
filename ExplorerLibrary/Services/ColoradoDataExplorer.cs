using ExplorerLibrary.Helpers;
using ExplorerLibrary.Models;
using Microsoft.Extensions.Logging;
using NetTopologySuite.IO.Esri.Dbf;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.IO.Compression;

namespace ExplorerLibrary.Services
{
    public class ColoradoDataExplorer : IDataExplorer
    {
        private readonly ILogger<ColoradoDataExplorer> _log;
        private readonly IDataAccess _da;
        private readonly string _path = @"C:\temp";
        private readonly string fileNameInZip = "Wells.dbf";
        private readonly string stateId = "CO";
        private readonly string wellBoreUrl = @"https://ecmc.state.co.us/data2.html#/downloads";

        public ColoradoDataExplorer(ILogger<ColoradoDataExplorer> log, IDataAccess da)
        {
            _log = log;
            _da = da;
        }
        public async Task<List<StateInfo>> GetExplorerData(string connectionString)
        {
            List<StateInfo> info = new List<StateInfo>();

            string url = wellBoreUrl;
            string file = "WELLS_SHP.ZIP";
            ChromeDownload(url, file);
            string zipPath = _path + @"\" + file;
            using (FileStream zipFileStream = new FileStream(zipPath, FileMode.Open, FileAccess.Read))
            using (ZipArchive zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry zipEntry = zipArchive.GetEntry(fileNameInZip);
                if (zipEntry == null)
                {
                    throw new FileNotFoundException($"File '{fileNameInZip}' not found in the zip archive.");
                }
                MemoryStream zipStream = new MemoryStream();
                using (Stream stream = zipEntry.Open())
                {
                    stream.CopyTo(zipStream);
                }
                zipStream.Position = 0;

                using var dbf = new DbfReader(zipStream);
                Console.WriteLine($"Number of wells are: {dbf.RecordCount}");

                IEnumerable<StateInfo> result= await _da.ReadData<IEnumerable<StateInfo>>("", connectionString);
                info = result.ToList();
                StateInfo stateInfo = info.SingleOrDefault(s => s.StateId == stateId);
                if (stateInfo == null)
                {
                    StateInfo newState = new StateInfo();
                    newState.StateId = stateId;
                    newState.StateFullName = "Colorado";
                    newState.WellCount = $"{dbf.RecordCount}";
                    newState.LoaderSource = wellBoreUrl;
                    info.Add(newState);
                }
                else
                {
                    stateInfo.WellCount = $"{dbf.RecordCount}";
                }
                await _da.SaveData(connectionString, info, "");
            }
            return info;
        }

        private void ChromeDownload(string url, string file)
        {
            string filePath = _path + @"\" + file;
            bool getNewCache = CommonMethods.SaveCache(filePath);

            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless=new");
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);
            chromeOptions.AddUserProfilePreference("download.default_directory", _path);
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", "true");
            chromeOptions.AddArgument("--ignore-ssl-errors");
            chromeOptions.AddArgument("--ignore-certificate-errors");

            IWebDriver driver = new ChromeDriver(chromeOptions);

            try
            {
                if (getNewCache)
                {
                    driver.Navigate().GoToUrl(wellBoreUrl);
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(40));

                    IWebElement expandLink = wait.Until(drv =>
                    {
                        var element = drv.FindElement(By.LinkText("Well Surface Location Data (Updated Daily)"));
                        return (element != null && element.Displayed && element.Enabled) ? element : null;
                    });
                    expandLink.Click();

                    IWebElement downloadLink = wait.Until(drv =>
                    {
                        var element = drv.FindElement(By.LinkText("Well Spots (APIs)(10 Mb)"));
                        return (element != null && element.Displayed && element.Enabled) ? element : null;
                    });
                    downloadLink.Click();

                    CommonMethods.WaitForFileDownload(_path, file, 60);
                }

                //string zipPath = _path + @"\" + file;
            }
            catch (NoSuchElementException e)
            {
                Exception error = new Exception(
                    $"Element not found: {e.Message}, " +
                    $"Page Source: {driver.PageSource}"
                    );
                throw error;
            }
            catch (Exception e)
            {
                Exception error = new Exception(
                    $"An error occurred: {e.Message}"
                    );
                throw error;
            }
            finally
            {
                driver.Quit();
            }
        }
    }
}
