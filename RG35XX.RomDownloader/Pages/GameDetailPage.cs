using RG35XX.Core.Drawing;
using RG35XX.Core.GamePads;
using RG35XX.Core.Interfaces;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RomDownloader.Models;

namespace RomDownloader.Pages
{
    internal class GameDetailPage : Page
    {
        private readonly Application _application;

        private readonly GameDefinition _game;

        private readonly HttpClient _httpClient;

        private readonly IStorageProvider _storageProvider;

        private readonly string _systemName;

        private int _selectedScreenshot;

        public GameDetailPage(Application application, string systemName, GameDefinition game, IStorageProvider storageProvider)
        {
            HttpClientHandler handler = new()
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
              (httpRequestMessage, cert, cetChain, policyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", $"https://vimm.net/vault/{game.GameId}");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://vimm.net");

            _application = application;

            _systemName = systemName;

            _storageProvider = storageProvider;

            _game = game;

            float detailsTop = 0;

            if (_game.ImageDefinitions.Count > 0)
            {
                _selectedScreenshot = 0;

                ImageDefinition image = _game.ImageDefinitions[0];

                PictureBox screenshot = new()
                {
                    Bounds = new Bounds(0, 0, 1, 0.5f),
                    ScaleMode = ScaleMode.PreserveAspectRatio,
                    Image = new Bitmap("Images/loading.png"),
                    IsSelectable = true
                };

                screenshot.OnKeyPressed += this.Screenshot_OnKeyPressed;

                detailsTop = 0.5f;

                Task t = Task.Run(async () => await screenshot.TryLoadImageAsync(image.GetUrl(), _httpClient));

                this.AddControl(screenshot);
            }

            Label title = new()
            {
                Text = game.Title,
                Bounds = new Bounds(0, detailsTop, 1, 0.05f),
            };

            Label genre = new()
            {
                Text = game.Genre,
                FontSize = 0.4f,
                Bounds = new Bounds(0, detailsTop + 0.05f, 1, 0.05f),
            };

            Label releaseDate = new()
            {
                Text = game.ReleaseDate.ToString("yyyy-MM-dd"),
                FontSize = 0.4f,
                Bounds = new Bounds(0, detailsTop + 0.1f, 1, 0.05f),
            };

            Label details = new()
            {
                Text = game.Description,
                FontSize = 0.4f,
                Bounds = new Bounds(0, detailsTop + 0.15f, 1, 0.75f),
            };

            Button downloadButton = new()
            {
                Text = "Download",
                Bounds = new Bounds(0, 0.9f, 1, 0.1f),
                IsSelectable = true
            };

            downloadButton.Click += this.DownloadButton_Click;
            this.AddControl(title);
            this.AddControl(genre);
            this.AddControl(releaseDate);
            this.AddControl(details);
            this.AddControl(downloadButton);
        }

        public override void OnKey(GamepadKey key)
        {
            if (key.IsCancel())
            {
                _application.ClosePage();
            }

            if (key == GamepadKey.DOWN)
            {
                this.SelectNext();
            }

            if (key == GamepadKey.UP)
            {
                this.SelectPrevious();
            }

            base.OnKey(key);
        }

        private void DownloadButton_Click(object? sender, EventArgs e)
        {
            GameInstallPage installPage = new(_systemName, _httpClient, _storageProvider, _game.MediaId, _application);

            _application.OpenPage(installPage);

            Task t = Task.Run(installPage.Download);
        }

        private void Screenshot_OnKeyPressed(object? sender, GamepadKey e)
        {
            int lastScreenshot = _selectedScreenshot;

            switch (e)
            {
                case GamepadKey.LEFT:
                    _selectedScreenshot--;

                    if (_selectedScreenshot < 0)
                    {
                        _selectedScreenshot = _game.ImageDefinitions.Count - 1;
                    }

                    break;

                case GamepadKey.RIGHT:
                    _selectedScreenshot++;

                    if (_selectedScreenshot >= _game.ImageDefinitions.Count)
                    {
                        _selectedScreenshot = 0;
                    }

                    break;
            }

            if (lastScreenshot != _selectedScreenshot)
            {
                lastScreenshot = _selectedScreenshot;

                PictureBox screenshot = (PictureBox)sender;

                screenshot.Image = new Bitmap("Images/loading.png");

                Task t = Task.Run(async () => await screenshot.TryLoadImageAsync(_game.ImageDefinitions[_selectedScreenshot].GetUrl(), _httpClient));
            }
        }
    }
}