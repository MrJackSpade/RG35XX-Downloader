namespace RomDownloader.Models
{
    internal class StorageLocation
    {
        public string Name { get; }

        public string Path { get; }

        public StorageLocation()
        { }

        public StorageLocation(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}