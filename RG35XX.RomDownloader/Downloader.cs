using RG35XX.Libraries;
using RG35XX.Libraries.Dialogs;
using RomDownloader.Models;
using RomDownloader.Pages;
using System.Reflection;
using System.Text.Json;

namespace RomDownloader
{
    public class Downloader
    {
        private static Dictionary<string, List<GameDefinition>> _games = [];

        private readonly Application _application;

        private readonly HttpClient _httpClient;

        private readonly List<string> _skipTags =
        [
            "CRC", "MD5", "SHA1"
        ];

        private readonly StorageProvider _storageProvider;

        public Downloader()
        {
            HttpClientHandler handler = new()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => true
            };

            _httpClient = new HttpClient(handler);

            _storageProvider = new StorageProvider();
            _storageProvider.Initialize();

            _application = new Application(640, 480);
            _application.Execute();

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://vimm.net/");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://vimm.net");
        }

        public async Task Execute()
        {
            try
            {
                Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RomDownloader.games.json");

                string content = await new StreamReader(stream).ReadToEndAsync();

                _games = JsonSerializer.Deserialize<Dictionary<string, List<GameDefinition>>>(content, new JsonSerializerOptions()
                {
                    TypeInfoResolver = GameDefinitionContext.Default
                });

                SystemListPage systemListPage = new(_application, _games, _storageProvider);

                _application.OpenPage(systemListPage);
            }
            catch (Exception ex)
            {
                Alert alert = new("Error", ex.Message);
                await _application.ShowDialog(alert);
                File.WriteAllText("RomDownloader.log", ex.ToString());
                throw;
            }

            await _application.WaitForClose();
        }
    }
}