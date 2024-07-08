namespace WellDataExplorer.Models
{
    public class StateWellCount
    {
        public Dictionary<string, int> WellCount { get; set; }

        public StateWellCount()
        {
            WellCount = new Dictionary<string, int>
        {
            { "KS", 495397},
            { "TX", 1192063}
        };
        }
    }
}
