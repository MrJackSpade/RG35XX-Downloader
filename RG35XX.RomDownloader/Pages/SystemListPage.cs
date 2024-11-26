using RG35XX.Core.Drawing;
using RG35XX.Core.GamePads;
using RG35XX.Core.Interfaces;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RomDownloader.Models;
using System.Diagnostics;

namespace RomDownloader.Pages
{
    internal class SystemListPage : Page
    {
        private readonly Application _application;

        private readonly Dictionary<string, List<GameDefinition>> _gameDefinitions;

        private readonly IStorageProvider _storageProvider;

        private readonly IconView icons = new();

        public SystemListPage(Application application, Dictionary<string, List<GameDefinition>> gameDefinitions, IStorageProvider storageProvider)
        {
            _gameDefinitions = gameDefinitions;
            _application = application;
            _storageProvider = storageProvider;

            foreach (string system in gameDefinitions.Keys)
            {
                if (string.IsNullOrWhiteSpace(DirectoryMapper.GetDirectory(system)))
                {
                    continue;
                }

                string iconName = system.ToLower().Replace(" ", "_");

                Bitmap? bitmap = null;

                try
                {
                    bitmap = new Bitmap($"Images/{iconName}.png");
                }
                catch
                {
                    Debug.WriteLine("Failed to load icon: " + iconName);
                }

                Icon icon = new()
                {
                    Text = system,
                    Image = bitmap
                };

                icons.AddControl(icon);
            }

            this.AddControl(icons);

            icons.OnKeyPressed += this.Icons_OnKeyPressed;
        }

        private void Icons_OnKeyPressed(object? sender, GamepadKey e)
        {
            if (e.IsAccept())
            {
                Icon? selected = icons.SelectedItem as Icon;

                List<GameDefinition> gameDefinitions = _gameDefinitions[selected.Text];

                GamesListPage gamesListPage = new(_application, selected.Text, gameDefinitions, _storageProvider);

                _application.OpenPage(gamesListPage);
            }

            if (e.IsCancel())
            {
                System.Environment.Exit(0);
            }
        }
    }
}