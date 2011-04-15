using System;
using System.Collections.Generic;
using System.Linq;

namespace PdcDownloader
{
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