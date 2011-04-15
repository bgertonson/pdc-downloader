using System;
using System.Collections.Generic;
using System.Linq;

namespace PdcDownloader
{
    public class PdcDownloadQueueProvider : IDownloadQueueProvider
    {
        public IEnumerable<DownloadQueueItem> GetQueue(VideoQuality quality)
        {
            var patternsToDownload = quality.Pattern.Union(new[] { ".pptx" });

            var service = new PDC.ScheduleModel(new Uri("http://odata.microsoftpdc.com/ODataSchedule.svc"));

            var items = service.Sessions.Expand("DownloadableContent").ToList()
                .SelectMany(s => s.DownloadableContent
                                     .Select(i => new DownloadQueueItem() { Name = s.FullTitle, Url = i.Url }))
                .Where(
                    q =>
                    patternsToDownload.Any(ext => q.Url.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

            return items;
        }
    }
}