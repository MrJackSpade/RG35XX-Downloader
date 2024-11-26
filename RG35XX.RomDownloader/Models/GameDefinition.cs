namespace RomDownloader.Models
{
    public class GameDefinition
    {
        public int GameId { get; set; }

        public List<ImageDefinition> ImageDefinitions { get; set; } = [];

        public int MediaId { get; set; }

        public string Title { get; set; }
    }

    public class ImageDefinition
    {
        public string Url { get; set; }

        internal string GetUrl()
        {
            return $"https://vimm.net/image.php?{Url}";
        }
    }
}