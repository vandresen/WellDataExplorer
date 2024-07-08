namespace WellDataExplorer.Models
{
    public class StateLoaderSource
    {
        public Dictionary<string, string> StateLoaders { get; set; }

        public StateLoaderSource()
        {
            StateLoaders = new Dictionary<string, string>
            {
                { "KS", @"https://github.com/vandresen/KansasPPDMLoader"},
                { "TX", @"https://github.com/vandresen/TexasPPDMLoader"}
            };
        }
    }
}
