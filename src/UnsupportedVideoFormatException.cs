using Gst.Video;
using System;

namespace VL.Lib.GStreamer
{
    public class UnsupportedVideoFormatException : Exception
    {
        public VideoFormat Format { get; }

        public UnsupportedVideoFormatException(VideoFormat format)
            : base($"Unsupported video format {format}.")
        {
            Format = format;
        }
    }
}
