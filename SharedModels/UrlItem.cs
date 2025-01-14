namespace DeviceTunerNET.SharedModels
{
    public class UrlItem
    {
        public UrlItem(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
