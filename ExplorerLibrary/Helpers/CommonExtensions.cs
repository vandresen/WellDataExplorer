using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerLibrary.Helpers
{
    public static class CommonExtensions
    {
        public static string[] ParseString(this string ln, int[] widths)
        {
            string[] ret = new string[widths.Length];
            char[] c = ln.ToCharArray();
            int startPos = 0;
            for (int i = 0; i < widths.Length; i++)
            {
                int width = widths[i];
                ret[i] = new string(c.Skip(startPos).Take(width).ToArray<char>());
                startPos += width;
            }
            return ret;
        }
    }
}
