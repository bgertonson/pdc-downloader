using System;
using System.Collections.Generic;
using System.Linq;

namespace PdcDownloader
{
    public class Mix11DownloadQueueProvider : IDownloadQueueProvider
    {
        public IEnumerable<DownloadQueueItem> GetQueue(VideoQuality quality)
        {
            var q = (Mix11VideoQuality) quality.Key;

            var service = new Mix11.eventsEntities(new Uri(@"http://live.visitmix.com/odata"));
            var items = service.Sessions.ToList();
            var vidQueue = items.Select(
                    s => new DownloadQueueItem() {Name = s.SessionCode, Url = String.Format(q.UrlFormat, s.SessionCode)}).
                    ToList();
            var pptQueue =
                service.Sessions.ToList().Select(
                    s =>
                    new DownloadQueueItem()
                        {
                            Name = s.SessionCode,
                            Url = String.Format("http://files.ch9.ms/mix/2011/ppt/{0}.pptx", s.SessionCode)
                        }).ToList();

            return vidQueue.Union(pptQueue);
        }

        public class Mix11VideoQuality
        {
            private static List<Mix11VideoQuality> _qualityTypes = new List<Mix11VideoQuality>();

            private const string VideoPathBase = @"http://files.ch9.ms/mix/2011/{0}/{{0}}{1}";

            public string Key { get; private set; }
            public string Name { get; private set; }
            public string Pattern { get; private set; }
            public string Extension { get; private set; }
            public string UrlFormat { get; set; }

            public Mix11VideoQuality(string key, string name, string urlFormat, string pattern, string extension)
            {
                Key = key;
                Name = name;
                UrlFormat = urlFormat;
                Pattern = pattern;
                Extension = extension;
            }

            private static Mix11VideoQuality AddVideoQuality(string key, string name, string pattern, string extension)
            {
                var q = new Mix11VideoQuality(key, name, String.Format(VideoPathBase, extension, pattern), pattern, extension);
                _qualityTypes.Add(q);
                return q;
            }

            public static implicit operator Mix11VideoQuality(string key)
            {
                var result = _qualityTypes.FirstOrDefault(q => q.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                return result ?? WMVLOW;
            }

            public static implicit operator string(Mix11VideoQuality quality)
            {
                return quality.Key;
            }

            public static Mix11VideoQuality WMVHIGH = AddVideoQuality("WMVHIGH", "HD WMV", "-HD.wmv", "wmv-hq");
            public static Mix11VideoQuality WMVLOW = AddVideoQuality("WMVLOW", "Low Bitrate WMV", ".wmv", "wmv");
            public static Mix11VideoQuality MP4HIGH = AddVideoQuality("MP4HIGH", "HD MP4", ".mp4", "mp4");
            //public static Mix11VideoQuality MP4LOW = AddVideoQuality("MP4LOW", "Low Bitrate Mp4", new[] { "low.mp4", "750k.mp4" }, "mp4");
        }
    }
}