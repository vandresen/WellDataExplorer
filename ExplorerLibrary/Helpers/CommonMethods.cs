using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerLibrary.Helpers
{
    public class CommonMethods
    {
        public static void WaitForFileDownload(string downloadDirectory, string fileName, int timeoutInSeconds)
        {
            var filePath = Path.Combine(downloadDirectory, fileName);
            var endTime = DateTime.Now.AddSeconds(timeoutInSeconds);

            while (DateTime.Now < endTime)
            {
                if (File.Exists(filePath))
                {
                    Console.WriteLine("File downloaded successfully.");
                    return;
                }
                System.Threading.Thread.Sleep(1000);
            }

            throw new TimeoutException("File download timed out.");
        }

        public static bool SaveCache(string filePath)
        {
            if (File.Exists(filePath))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(filePath);
                DateTime currentDate = DateTime.Now;
                if (currentDate < lastWriteTime.AddDays(14))
                {
                    Console.WriteLine("The wellbore file was last written less than 14 days ago.");
                    return false;
                }
                File.Delete(filePath);
            }
            return true;
        }
    }
}
