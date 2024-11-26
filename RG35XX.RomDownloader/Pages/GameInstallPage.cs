using RG35XX.Core.Interfaces;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RG35XX.Libraries.Dialogs;
using RG35XX.Libraries.Extensions;
using RomDownloader.Models;
using System.IO.Compression;

namespace RomDownloader.Pages
{
    internal class GameInstallPage : Page
    {
        private readonly Application _application;

        private readonly HttpClient _httpClient;

        private readonly int _mediaId;

        private readonly ProgressBar _progressBar;

        private readonly IStorageProvider _storageProvider;

        private readonly string _system;

        public GameInstallPage(string system, HttpClient httpClient, IStorageProvider storageProvider, int mediaId, Application application)
        {
            _system = system;
            _httpClient = httpClient;
            _storageProvider = storageProvider;
            _mediaId = mediaId;
            _application = application;

            _progressBar = new()
            {
                Bounds = new Bounds(0.1f, 0.45f, 0.8f, 0.1f)
            };

            this.AddControl(_progressBar);
        }

        public async Task Download()
        {
            try
            {
                StorageLocation? storageLocation = null;

                string? romDir = DirectoryMapper.GetDirectory(_system);

                if (string.IsNullOrWhiteSpace(romDir))
                {
                    return;
                }

                string mmcPath = Path.Combine(_storageProvider.MMC, "Roms", romDir);
                string sdPath = Path.Combine(_storageProvider.SD, "Roms", romDir);

                if (Directory.Exists(sdPath))
                {
                    storageLocation = new StorageLocation("SD Card", sdPath);
                }
                else
                {
                    storageLocation = new StorageLocation("Internal", mmcPath);
                }

                string play = "https://vimm.net/vault/?p=play&mediaId=" + _mediaId;

                string playSource = await _httpClient.GetStringAsync(play);

                string phpFile = playSource.From("EJS_gameUrl = '").To("'");

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
                }
                else
                {
                }

                Alert alert = new("Download Complete", "The game has been downloaded and installed.");

                await _application.ShowDialog(alert);
            }
            catch (Exception ex)
            {
                Alert errorAlert = new("Download Failed", ex.Message);

                await _application.ShowDialog(errorAlert);
            }
            finally
            {
                this.Close();
            }
        }

        private void ReportProgress(long totalRead, long? totalBytes)
        {
            int percent = (int)(totalRead / (double)totalBytes * 100);

            _progressBar.Value = percent;
        }
    }
}