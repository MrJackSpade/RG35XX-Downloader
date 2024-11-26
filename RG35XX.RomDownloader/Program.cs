namespace RomDownloader
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            Downloader store = new();

            await store.Execute();
        }
    }
}