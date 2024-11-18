using System.Runtime.Serialization;

namespace RomDownloader.Exceptions
{
    internal class RomDownloaderException : Exception
    {
        public bool LogStackTrace { get; set; } = false;

        public RomDownloaderException()
        {
        }

        public RomDownloaderException(string? message) : base(message)
        {
        }

        public RomDownloaderException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected RomDownloaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
