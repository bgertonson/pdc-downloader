using System;
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
            if(args.Length < 1)
            {
                Console.WriteLine("You must say which videos you want to download (MIX11, PDC10)");
                return;
            }
            var providerKey = args[0];
            var videoQuality = args.Length > 1 ? (VideoQuality) args[1] : VideoQuality.WMVLOW;

            var context = ApplicationContext.Current;
            var spinner = new[] {"|", "/", "-", "\\"};
            var badChars = new[] {"\\", "/", "<", ">", "|", "?", "*", ":", "\""};
            var spinnerIndex = 0;

            var provider = DownloadQueueProviderRegistry.Lookup(providerKey);

            if(provider == null)
            {
                Console.WriteLine("There is no provider for videos from '{0}'", providerKey);
            }

            foreach (var s in provider.GetQueue(videoQuality))
            {
                if (context.ShouldAbort) break;
                //Console.WriteLine(s.FullTitle);
                    if (context.ShouldAbort) break;
                    var extension = getExtension(s.Url);

                    var title = badChars.Aggregate(s.Name, (t, replace) => t.Replace(replace, ""));
                    var filename = String.Format("{0}.{1}", title, extension);
                    if (File.Exists(filename)) continue;
                    var client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ClientDownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(ClientDownloadFileCompleted);
                    
                    client.DownloadFileAsync(new Uri(s.Url), filename, null);

                    Console.Write("Downloading [{1}] {0}...    0%", s.Name.Substring(0, Math.Min(s.Name.Length, 45)), extension.ToUpper());
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
                                context.SkipFile();
                                client.CancelAsync();
                                break;
                            case ConsoleKey.Q:
                                context.Abort();
                                client.CancelAsync();
                                break;
                        }
                    }
                    Console.CursorLeft -= 6;
                    if(context.Skip) File.Delete(filename);
                    Console.WriteLine(context.Skip ? " Canceled! " : " Done! ");
                
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
        public bool Skip { get; set; }
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
            Skip = false;
            ShouldAbort = false;
            PercentComplete = 0;
        }

        public void SkipFile()
        {
            Skip = true;
        }

        public void Abort()
        {
            ShouldAbort = true;
            Skip = true;
        }

        public void Complete()
        {
            IsReadyToDownload = true;
        }

        public void StartDownload()
        {
            PercentComplete = 0;
            IsReadyToDownload = false;
            Skip = false;
        }
    }
}
