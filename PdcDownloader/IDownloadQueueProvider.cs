using System;
using System.Collections.Generic;

namespace PdcDownloader
{
    public interface IDownloadQueueProvider
    {
        IEnumerable<DownloadQueueItem> GetQueue(VideoQuality quality);
    }

    public static class DownloadQueueProviderRegistry
    {
        private static readonly IDictionary<string, IDownloadQueueProvider> _storage =
            new Dictionary<string, IDownloadQueueProvider>(StringComparer.OrdinalIgnoreCase);

        static DownloadQueueProviderRegistry()
        {
            Register("Mix11", new Mix11DownloadQueueProvider());
            Register("Pdc10", new PdcDownloadQueueProvider());
        }

        public static void Register(string key, IDownloadQueueProvider provider)
        {
            _storage.Add(key, provider);
        }

        public static IDownloadQueueProvider Lookup(string key)
        {
            return !_storage.ContainsKey(key) ? null : _storage[key];
        }
    }
}
