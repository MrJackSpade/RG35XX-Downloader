using RG35XX.Core.Drawing;
using RG35XX.Core.Extensions;
using RG35XX.Core.Fonts;
using RG35XX.Core.GamePads;
using RG35XX.Libraries;
using RG35XX.Libraries.Extensions;
using RomDownloader.Exceptions;
using RomDownloader.Extensions;
using RomDownloader.Models;
using SixLabors.ImageSharp;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Web;
using Color = RG35XX.Core.Drawing.Color;

namespace RomDownloader
{
    public class Downloader
    {
        private readonly ConsoleRenderer _consoleRenderer;

        private readonly GamePadReader _gamePadReader;

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
            _gamePadReader = new GamePadReader();
            _gamePadReader.Initialize();

            _consoleRenderer = new ConsoleRenderer(ConsoleFont.Px437_IBM_VGA_8x16);
            _consoleRenderer.Initialize(640, 480);

            _storageProvider = new StorageProvider();
            _storageProvider.Initialize();

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Referer", "https://vimm.net/");
            _httpClient.DefaultRequestHeaders.Add("Origin", "https://vimm.net");
        }

        public async Task Download(VimmSystem system, string url)
        {
            StorageLocation? storageLocation;

            string? romDir = DirectoryMapper.GetDirectory(system.Name);

            if (string.IsNullOrWhiteSpace(romDir))
            {
                return;
            }

            string mmcPath = Path.Combine(_storageProvider.MMC, "Roms", romDir);
            string sdPath = Path.Combine(_storageProvider.SD, "Roms", romDir);

            if (Directory.Exists(sdPath))
            {
                storageLocation = this.Select([
                    new StorageLocation("Internal", mmcPath),
                    new StorageLocation("SD Card", sdPath),
                ]);

                if (storageLocation is null)
                {
                    return;
                }
            }
            else
            {
                storageLocation = new StorageLocation("Internal", mmcPath);
            }

            _consoleRenderer.WriteLine("Getting details...");

            string source = await _httpClient.GetStringAsync(url);

            string mediaId = source.From("name=\"mediaId\" value=\"").To("\"");

            string play = "https://vimm.net/vault/?p=play&mediaId=" + mediaId;

            _consoleRenderer.WriteLine("Getting source...");

            string playSource = await _httpClient.GetStringAsync(play);

            string phpFile = playSource.From("EJS_gameUrl = '").To("'");

            _consoleRenderer.WriteLine("Downloading...");

            _consoleRenderer.WriteLine("Waiting for response...");

            using MemoryStream memoryStream = new();

            using (HttpResponseMessage response = await _httpClient.GetAsync(phpFile, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                long? totalBytes = response.Content.Headers.ContentLength;

                using Stream contentStream = await response.Content.ReadAsStreamAsync();

                long totalRead = 0L;
                byte[] buffer = new byte[8192];
                bool isMoreToRead = true;

                do
                {
                    int read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    
                    if (read == 0)
                    {
                        isMoreToRead = false;
                        this.ReportProgress(totalRead, totalBytes);
                        continue;
                    }

                    await memoryStream.WriteAsync(buffer, 0, read);

                    totalRead += read;


                    this.ReportProgress(totalRead, totalBytes);

                } while (isMoreToRead);
            }

            _consoleRenderer.WriteLine();

            _consoleRenderer.WriteLine("Unzipping...");

            using ZipArchive zipArchive = new(memoryStream);

            ZipArchiveEntry rom = zipArchive.Entries.OrderByDescending(z => z.Length).First();

            string romPath = Path.Combine(storageLocation.Path, rom.Name);

            using Stream stream = rom.Open();

            FileInfo target = new(romPath);

            if (!target.Exists)
            {
                if (!target.Directory.Exists)
                {
                    target.Directory.Create();
                }

                using FileStream fileStream = new(romPath, FileMode.Create);

                await stream.CopyToAsync(fileStream);

                _consoleRenderer.WriteLine($"Downloaded to: {romPath}");

                _consoleRenderer.WriteLine("Download complete. Press any key to continue.");
            }
            else
            {
                _consoleRenderer.WriteLine("File already exists. Press any key to continue.");
            }

            _gamePadReader.ClearBuffer();
            _gamePadReader.WaitForInput();

            _consoleRenderer.Clear();
        }

        private void ReportProgress(long totalRead, long? totalBytes)
        {
            _consoleRenderer.ClearLine(false);

            int progressBarWith = _consoleRenderer.Width - "Progress [ ]".Length;

            if (totalBytes.HasValue)
            {

                double progress = ((double)totalRead / totalBytes.Value);
                int bars = (int)(progressBarWith * progress);

                string progressBar = new('#', bars);

                progressBar = progressBar.PadRight(progressBarWith, ' ');

                _consoleRenderer.Write($"Progress [{progressBar}]");
            }
            else
            {
                _consoleRenderer.Write($"Downloaded {totalRead} bytes");
            }
        }

        public async Task Execute()
        {
            try
            {
                VimmSystem? vimmSystem = null;
                List<VimmSystem>? systems = null;
                char? toView = null;
                List<VimmGameListItem>? gameListItems = null;
                VimmGameListItem? vimmGameListItem = null;
                List<char?> selectChars = ['#', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];

                do
                {
                    if (vimmSystem is null)
                    {
                        systems ??= await this.GetSystems();

                        vimmSystem = this.Select(systems);

                        if (vimmSystem is null)
                        {
                            System.Environment.Exit(0);
                        }

                        continue;
                    }

                    if (toView is null)
                    {
                        toView = this.Select(selectChars);

                        if (toView is null)
                        {
                            vimmSystem = null;
                        }

                        continue;
                    }

                    if (vimmGameListItem is null)
                    {
                        gameListItems ??= await this.GetListGames(vimmSystem, toView.Value);

                        vimmGameListItem = this.Select(gameListItems);

                        if (vimmGameListItem is null)
                        {
                            toView = null;
                            gameListItems = null;
                        }

                        continue;
                    }

                    await this.ViewDetails(vimmSystem, vimmGameListItem);

                    vimmGameListItem = null;
                } while (true);
            }
            catch (Exception ex)
            {
                _gamePadReader.ClearBuffer();
                _consoleRenderer.WriteLine(ex.Message, Color.Red, Color.Black);

                if ((ex as RomDownloaderException)?.LogStackTrace ?? true)
                {
                    _consoleRenderer.WriteLine(ex.StackTrace, Color.Red, Color.Black);
                }

                _gamePadReader.WaitForInput();

                System.Environment.Exit(1);
            }
        }

        public async Task<List<VimmGameListItem>> GetListGames(VimmSystem system, char letter)
        {
            _consoleRenderer.Clear();
            _consoleRenderer.WriteLine("Getting games...");

            string url = "https://vimm.net" + system.Url + "/" + letter.ToString();

            string source = await _httpClient.GetStringAsync(url);

            source = source.From("</caption").To("/table");

            List<VimmGameListItem> gameListItems = [];

            source = source.Replace("href= \"", "href=\"");

            foreach (string s in source.Split("href=\"").Skip(1))
            {
                if (!s.StartsWith("/vault/"))
                {
                    continue;
                }

                string mediaId = s.From("/vault/").To("\"");

                if (!mediaId.All(char.IsDigit))
                {
                    continue;
                }

                string gameUrl = s.To("\"");

                string gameName = HttpUtility.HtmlDecode(s.From(">").To("<"));

                gameListItems.Add(new VimmGameListItem() { Name = gameName, Url = gameUrl });
            }

            return gameListItems;
        }

        public async Task<List<VimmSystem>> GetSystems()
        {
            _consoleRenderer.Clear();

            _consoleRenderer.WriteLine("Getting systems...");

            using HttpResponseMessage response = await _httpClient.GetAsync("https://vimm.net/vault/");

            if (!response.IsSuccessStatusCode)
            {
                throw new RemoteException();
            }

            string source = await response.Content.ReadAsStringAsync();

            if (source.Contains("Error: Database is not responding."))
            {
                throw new RemoteException("Remote database is not responding");
            }

            List<VimmSystem> systems = [.. GetVimmSystems(source, "Consoles"), .. GetVimmSystems(source, "Handhelds")];

            systems = systems.Where(s => DirectoryMapper.GetDirectory(s.Name) != null).ToList();

            return systems;
        }

        private static List<VimmSystem> GetVimmSystems(string source, string caption)
        {
            string consolesSource = source.From($"<caption>{caption}</caption>").To("</table>");

            List<VimmSystem> systems = [];

            foreach (string list in consolesSource.Split("<a").Skip(1))
            {
                string name = list.From(">").To("<");

                string url = list.From("href=\"").To("\"");

                systems.Add(new VimmSystem() { Name = name, Url = url });
            }

            return systems;
        }

        private T? Select<T>(List<T> list)
        {
            int currentIndex = 0;
            int windowStartIndex = 0;
            int displayCount = _consoleRenderer.Height - 1;

            _consoleRenderer.AutoFlush = false;

            try
            {
                do
                {
                    _consoleRenderer.Clear(false);

                    for (int i = 0; i < displayCount; i++)
                    {
                        int itemIndex = windowStartIndex + i;

                        if (itemIndex < list.Count)
                        {
                            Color foreground = itemIndex == currentIndex ? Color.Black : Color.White;
                            Color background = itemIndex == currentIndex ? Color.White : Color.Black;

                            string toWrite = list[itemIndex]?.ToString() ?? string.Empty;

                            if (toWrite.Length > _consoleRenderer.Width)
                            {
                                toWrite = toWrite[.._consoleRenderer.Width];
                            }

                            _consoleRenderer.WriteLine(toWrite, foreground, background);
                        }
                    }

                    _consoleRenderer.Flush();

                    _gamePadReader.ClearBuffer();

                    GamepadKey key = _gamePadReader.WaitForInput();

                    if (key == GamepadKey.UP)
                    {
                        currentIndex = Math.Max(0, currentIndex - 1);

                        // Adjust windowStartIndex if necessary
                        if (currentIndex < windowStartIndex)
                        {
                            windowStartIndex = currentIndex;
                        }
                    }

                    if (key == GamepadKey.DOWN)
                    {
                        currentIndex = Math.Min(list.Count - 1, currentIndex + 1);

                        // Adjust windowStartIndex if necessary
                        if (currentIndex >= windowStartIndex + displayCount)
                        {
                            windowStartIndex = currentIndex - displayCount + 1;
                        }
                    }

                    if (key is GamepadKey.START_DOWN or GamepadKey.A_DOWN)
                    {
                        return list[currentIndex];
                    }

                    if (key is GamepadKey.MENU_DOWN || key is GamepadKey.B_DOWN)
                    {
                        return default;
                    }
                } while (true);
            }
            finally
            {
                _consoleRenderer.AutoFlush = true;
            }
        }

        private async Task ViewDetails(VimmSystem system, VimmGameListItem vimmGameListItem)
        {
            _consoleRenderer.Clear();
            string? boxArt = null;
            string url = string.Empty;

            try
            {
                _consoleRenderer.AutoFlush = false;

                url = "https://vimm.net" + vimmGameListItem.Url;

                string source = await _httpClient.GetStringAsync(url);

                boxArt = source.Split("<img").Skip(1)
                                             .Select(i => i.To(">"))
                                             .FirstOrDefault(i => i.Contains("?type=box&"))
                                             ?.From("src=\"")
                                             ?.To("\"");

                source = source.From("<table ").From(">").To("</table>");

                List<string> trs = [];

                foreach (int trIndex in source.AllIndexesOf("<tr"))
                {
                    string tr = source[trIndex..].From(">").To("</tr>");

                    trs.Add(tr);
                }

                foreach (string tr in trs)
                {
                    List<string> tds = [];

                    foreach (int tdIndex in tr.AllIndexesOf("<td"))
                    {
                        string td = tr[tdIndex..].From(">").To("</td>").Trim();

                        td = Regex.Replace(td, "<.*?>", string.Empty); // Removes all HTML tags

                        td = HttpUtility.HtmlDecode(td).Trim();

                        td = td.Replace("\n", string.Empty);

                        tds.Add(td);
                    }

                    if (tds.Count == 3)
                    {
                        if (_skipTags.Contains(tds[0]))
                        {
                            continue;
                        }

                        _consoleRenderer.Write(tds[0]);
                        _consoleRenderer.Write(": ");
                        _consoleRenderer.Write(tds[2]);
                        _consoleRenderer.WriteLine();
                    }
                }

                _consoleRenderer.WriteLine("");
                _consoleRenderer.WriteLine("[PRESS A TO DOWNLOAD]", Color.Green, Color.Black);
            }
            finally
            {
                _consoleRenderer.AutoFlush = true;
                _consoleRenderer.Flush();
            }

            if (!string.IsNullOrWhiteSpace(boxArt))
            {
                boxArt = HttpUtility.HtmlDecode(boxArt);

                string imageurl = "https://vimm.net" + boxArt;

                // Create a new HTTP request for the image
                HttpRequestMessage request = new(HttpMethod.Get, imageurl);
                // Set the Referrer header to the originating URL
                request.Headers.Referrer = new Uri(url);

                // Send the request and get the response
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                byte[] data = await response.Content.ReadAsByteArrayAsync();

                Image toRender = await ImageResizer.ResizeImageAsync(data, _consoleRenderer.FrameBuffer.Width / 2, _consoleRenderer.FrameBuffer.Height);

                Bitmap bitmap = toRender.ToBitmap();

                _consoleRenderer.FrameBuffer.Draw(bitmap, _consoleRenderer.FrameBuffer.Width / 2, 0);
            }

            _gamePadReader.ClearBuffer();

            do
            {
                GamepadKey key = _gamePadReader.WaitForInput();

                if (key is GamepadKey.A_DOWN or GamepadKey.START_DOWN)
                {
                    await this.Download(system, url);
                    return;
                }

                if (key is GamepadKey.B_DOWN or GamepadKey.MENU_DOWN)
                {
                    return;
                }
            } while (true);
        }
    }
}