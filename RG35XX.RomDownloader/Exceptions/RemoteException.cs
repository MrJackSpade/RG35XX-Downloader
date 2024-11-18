namespace RomDownloader.Exceptions
{
    internal class RemoteException : RomDownloaderException
    {
        public RemoteException() : base("The remote service threw an exception. Try again later.")
        {
        }

        public RemoteException(string message) : base(message)
        {
        }
    }
}