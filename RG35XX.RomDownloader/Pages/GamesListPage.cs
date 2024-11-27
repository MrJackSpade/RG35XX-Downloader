using RG35XX.Core.Drawing;
using RG35XX.Core.GamePads;
using RG35XX.Core.Interfaces;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RomDownloader.Controls;
using RomDownloader.Models;

namespace RomDownloader.Pages
{
    internal class GamesListPage : Page
    {
        private readonly Application _application;

        private readonly List<GameDefinition> _games;

        private readonly ListBox _gamesList;

        private readonly TextBox _searchBox;

        private readonly IStorageProvider _storageProvider;

        private readonly string _systemName;

        private string _filter = string.Empty;

        public GamesListPage(Application application, string systemName, List<GameDefinition> games, IStorageProvider storageProvider)
        {
            _application = application;

            _games = games;

            _systemName = systemName;

            _storageProvider = storageProvider;

            _gamesList = new()
            {
                ItemHeight = 0.2f,
                Bounds = new Bounds(0f, 0.1f, 1f, 0.9f),
            };

            _searchBox = new()
            {
                Bounds = new Bounds(0.1f, 0f, 0.8f, 0.1f),
                BackgroundColor = Color.White,
                PlaceHolder = "Search",
            };

            this.UpdateList();

            this.AddControl(_gamesList);
            this.AddControl(_searchBox);

            _gamesList.OnKeyPressed += this.GamesList_OnKeyPressed;
            _searchBox.ValueChanged += this.SearchBox_ValueChanged;
            _searchBox.OnKeyPressed += this.SearchBox_OnKeyPressed;
        }

        private void GamesList_OnKeyPressed(object? sender, GamepadKey e)
        {
            if (e.IsAccept())
            {
                if (_gamesList.SelectedItem is not GameListItem gameListItem)
                {
                    return;
                }

                GameDetailPage gameDetailPage = new(_application, _systemName, gameListItem.GameDefinition, _storageProvider);

                _application.OpenPage(gameDetailPage);
            }

            if (e.IsCancel())
            {
                _application.ClosePage();
            }

            if (e == GamepadKey.LEFT)
            {
                this.SelectPrevious();
            }

            if (e == GamepadKey.RIGHT)
            {
                this.SelectNext();
            }
        }

        private void SearchBox_OnKeyPressed(object? sender, GamepadKey e)
        {
            if (e == GamepadKey.LEFT)
            {
                this.SelectPrevious();
            }

            if (e == GamepadKey.RIGHT)
            {
                this.SelectNext();
            }
        }

        private void SearchBox_ValueChanged(object? sender, string? e)
        {
            _filter = e ?? string.Empty;
            this.UpdateList();
        }

        private void UpdateList()
        {
            _gamesList.Clear();

            foreach (GameDefinition game in _games)
            {
                if (game.MediaId == 0)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(_filter) && !game.Title.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                GameListItem control = new(game)
                {
                    BackgroundColor = Color.Transparent
                };

                _gamesList.AddControl(control);
            }
        }
    }
}