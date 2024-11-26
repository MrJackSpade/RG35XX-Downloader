using RG35XX.Libraries.Controls;

namespace RomDownloader.Models
{
    internal class GameListItem : Control
    {
        public GameDefinition GameDefinition { get; private set; }

        public GameListItem(GameDefinition gameDefinition)
        {
            GameDefinition = gameDefinition;
        }
    }
}
