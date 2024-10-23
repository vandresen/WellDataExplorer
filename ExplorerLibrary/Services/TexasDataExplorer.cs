using ExplorerLibrary.Helpers;
using ExplorerLibrary.Models;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using System.IO.Compression;

namespace ExplorerLibrary.Services
{
    public class TexasDataExplorer : IDataExplorer
    {
        private readonly ILogger<TexasDataExplorer> _log;
        private readonly IDataAccess _da;
        private readonly string _path = @"C:\temp";
        private readonly string stateId = "TX";
        private readonly string wellBoreUrl = @"https://mft.rrc.texas.gov/link/b070ce28-5c58-4fe2-9eb7-8b70befb7af9";
        private int[] rootPos = {1, 3, 6, 11, 13, 15, 17, 20, 21, 23, 25, 27, 29, 34, 39, 41, 43, 45, 47, 49, 51,
            53, 55, 56, 57, 65, 73, 81, 87, 88, 89, 90, 91, 92, 100, 101, 102, 103, 105, 106, 108, 110, 112, 114,
            116, 118, 120, 122, 124, 126, 128, 130, 132, 133, 139, 146, 152, 158, 159, 160, 161, 168 };
        private int[] rootWidths;

        public TexasDataExplorer(ILogger<TexasDataExplorer> log, IDataAccess da)
        {
            _log = log;
            _da = da;
            rootWidths = new int[rootPos.Length - 1];
        }

        public async Task<List<StateInfo>> GetExplorerData(string connectionString)
        {
            List<StateInfo> info = new List<StateInfo>();
            int wellCount = 0;
            string url = wellBoreUrl;
            string file = "dbf900.txt.gz";
            ChromeDownload(url, file);

            string extractedFilePath = Path.Combine(_path, Path.GetFileNameWithoutExtension(file));
            //string extractPath = _path + @"\extract";
            string zipPath = _path + @"\" + file;
            using (FileStream sourceFileStream = File.OpenRead(zipPath))
            using (FileStream extractedFileStream = File.Create(extractedFilePath))
            using (GZipStream gzipStream = new GZipStream(sourceFileStream, CompressionMode.Decompress))
            {
                gzipStream.CopyTo(extractedFileStream);
            }

            string textFile = _path + @"\dbf900.txt";
            if (File.Exists(textFile))
            {
                int prevAPICount = 0;
                int referAPICount = 0;
                using (StreamReader fileStream = new StreamReader(textFile))
                {
                    for (int i = 1; i < rootPos.Length; i++)
                    {
                        rootWidths[i - 1] = rootPos[i] - rootPos[i - 1];
                    }
                    string ln;
                    while ((ln = fileStream.ReadLine()) != null)
                    {
                        string prevWellAPI = "";
                        string referCorrectAPInbr = "";
                        string recordKey = ln.Substring(0, 2);
                        
                        if (recordKey == "01")
                        {
                            string[] ret = ln.ParseString(rootWidths);
                            prevWellAPI = ret[33];
                            referCorrectAPInbr = ret[24];
                            wellCount++;
                            if (prevWellAPI != null)
                            {
                                if (prevWellAPI != "00000000")
                                {
                                    prevAPICount++;
                                    wellCount--;
                                }
                            }

                            if (referCorrectAPInbr != "00000000")
                            {
                                referAPICount++;
                                wellCount--;
                            }
                        }
                        
                    }
                }
                Console.WriteLine($"Previous API count: {prevAPICount}");
                Console.WriteLine($"Refer API count: {referAPICount}");
                Console.WriteLine($"Well count: {wellCount}");
            }

            IEnumerable<StateInfo> result = await _da.ReadData<IEnumerable<StateInfo>>("", connectionString);
            info = result.ToList();
            StateInfo stateInfo = info.SingleOrDefault(s => s.StateId == stateId);
            if (stateInfo == null)
            {
                StateInfo newState = new StateInfo();
                newState.StateId = stateId;
                newState.StateFullName = "Texas";
                newState.WellCount = $"{wellCount}";
                newState.LoaderSource = wellBoreUrl;
                info.Add(newState);
            }
            else
            {
                stateInfo.WellCount = $"{wellCount}";
            }
            await _da.SaveData(connectionString, info, "");
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
                    driver.FindElement(By.LinkText(file));
                    driver.FindElement(By.LinkText(file)).Click();
                    WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(600));
                    CommonMethods.WaitForFileDownload(_path, file, 600);
                }
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
