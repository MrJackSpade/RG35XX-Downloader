using RG35XX.Core.Drawing;
using RG35XX.Libraries.Controls;
using RomDownloader.Models;

namespace RomDownloader.Controls
{
    internal class GameListItem : Control
    {
        public GameDefinition GameDefinition { get; private set; }

        public GameListItem(GameDefinition gameDefinition)
        {
            GameDefinition = gameDefinition;

            Label title = new()
            {
                Text = gameDefinition.Title,
                BackgroundColor = Color.Transparent,
                Bounds = new Bounds(0, 0, 1, 0.333f),
            };

            Label genre = new()
            {
                Text = gameDefinition.Genre,
                BackgroundColor = Color.Transparent,
                TextColor = Color.Grey,
                FontSize = 0.4f,
                Bounds = new Bounds(0, 0.333f, 1, 0.2f),
            };

            Label description = new()
            {
                Text = gameDefinition.Description,
                BackgroundColor = Color.Transparent,
                TextColor = Color.Grey,
                FontSize = 0.4f,
                Bounds = new Bounds(0, 0.533f, 1, 0.466f),
            };

            this.AddControl(title);
            this.AddControl(genre);
            this.AddControl(description);
        }
    }
}
