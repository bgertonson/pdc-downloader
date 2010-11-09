using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace PdcDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            var videoQuality = args.Length > 0 ? (VideoQuality) args[0] : VideoQuality.WMVLOW;

            var patternsToDownload = videoQuality.Pattern.Union(new[] {".pptx"});
            var context = ApplicationContext.Current;
            var spinner = new[] {"|", "/", "-", "\\"};
            var badChars = new[] {"\\", "/", "<", ">", "|", "?", "*", ":", "\""};
            var spinnerIndex = 0;
            var service = new PDC.ScheduleModel(new Uri("http://odata.microsoftpdc.com/ODataSchedule.svc"));
            foreach (var s in service.Sessions.Expand("DownloadableContent").ToList())
            {
                if (context.ShouldAbort) break;
                //Console.WriteLine(s.FullTitle);
                foreach(var c in s.DownloadableContent)
                {
                    if (context.ShouldAbort) break;
                    var currentItem = c;
                    if (!patternsToDownload.Any(e => currentItem.Url.EndsWith(e, StringComparison.CurrentCultureIgnoreCase))) continue;
                    var extension = getExtension(currentItem.Url);

                    var title = badChars.Aggregate(s.FullTitle, (t, replace) => t.Replace(replace, ""));
                    var filename = String.Format("{0}.{1}", title, extension);
                    if (File.Exists(filename)) continue;
                    var client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ClientDownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(ClientDownloadFileCompleted);
                    
                    client.DownloadFileAsync(new Uri(c.Url), filename, null);

                    Console.Write("Downloading [{1}] {0}...    0%", s.FullTitle.Substring(0, Math.Min(s.FullTitle.Length, 45)), extension.ToUpper());
                    context.StartDownload();
                    while(!context.IsReadyToDownload)
                    {
                        Thread.Sleep(100);
                        Console.CursorLeft -= 6;
                        spinnerIndex = (spinnerIndex + 1)%4;
                        Console.Write("{0} {1}%", spinner[spinnerIndex], context.PercentComplete.ToString().PadLeft(3, ' '));
                        if (!Console.KeyAvailable) continue;
                        var keyInfo = Console.ReadKey(true);
                        switch(keyInfo.Key)
                        {
                            case ConsoleKey.S:
                                client.CancelAsync();
                                break;
                            case ConsoleKey.Q:
                                client.CancelAsync();
                                context.ShouldAbort = true;
                                break;
                        }
                    }
                    Console.CursorLeft -= 6;
                    if(context.PercentComplete != 100) File.Delete(filename);
                    Console.WriteLine(context.PercentComplete == 100 ? " Done! " : " Canceled! ");
                }
            }
            Console.WriteLine(context.ShouldAbort ? "Forced Complete" : "Complete");
        }

        private static String getExtension(string url)
        {
            var ioperiod = url.LastIndexOf('.');
            var extension = url.Substring(ioperiod + 1);
            return extension;
        }

        private static void ClientDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ApplicationContext.Current.PercentComplete = e.ProgressPercentage;
            
        }

        private static void ClientDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ApplicationContext.Current.Complete();
        }
    }

    public class ApplicationContext
    {
        public bool ShouldAbort { get; set; }
        public bool IsReadyToDownload { get; set; }
        public int PercentComplete { get; set; }

        public static ApplicationContext _instance = new ApplicationContext();

        public static ApplicationContext Current { get { return _instance; } }

        private ApplicationContext()
        {
            Reset();
        }

        public void Reset()
        {
            IsReadyToDownload = true;
            ShouldAbort = false;
            PercentComplete = 0;
        }

        public void Complete()
        {
            IsReadyToDownload = true;
        }

        public void StartDownload()
        {
            PercentComplete = 0;
            IsReadyToDownload = false;
        }
    }


    public class VideoQuality
    {
        private static List<VideoQuality> _qualityTypes = new List<VideoQuality>();

        public string Key { get; private set; }
        public string Name { get; private set; }
        public string[] Pattern { get; private set; }
        public string Extension { get; private set; }

        public VideoQuality(string key, string name, string[] pattern, string extension)
        {
            Key = key;
            Name = name;
            Pattern = pattern;
            Extension = extension;
        }

        private static VideoQuality AddVideoQuality(string key, string name, string[] pattern, string extension)
        {
            var q = new VideoQuality(key, name, pattern, extension);
            _qualityTypes.Add(q);
            return q;
        }

        public static implicit operator VideoQuality(string key)
        {
            var result = _qualityTypes.FirstOrDefault(q => q.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase));
            return result ?? VideoQuality.WMVLOW;
        }

        public static implicit operator string(VideoQuality quality)
        {
            return quality.Key;
        }

        public static VideoQuality WMVHIGH = AddVideoQuality("WMVHIGH", "HD WMV", new[]{"high.wmv", "2500k.wmv"}, "wmv");
        public static VideoQuality WMVLOW = AddVideoQuality("WMVLOW", "Low Bitrate WMV", new[]{"low.wmv","1000k.wmv"}, "wmv");
        public static VideoQuality MP4HIGH = AddVideoQuality("MP4HIGH", "HD MP4", new[]{"high.wmv","2500k.mp4"}, "mp4");
        public static VideoQuality MP4LOW = AddVideoQuality("MP4LOW", "Low Bitrate Mp4", new[]{"low.mp4","750k.mp4"}, "mp4");
    }
}
