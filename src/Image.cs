using Gst;
using Gst.Video;
using System;
using System.Threading;
using VL.Lib.Basics.Imaging;

namespace VL.Lib.GStreamer
{
    public class Image : IImage
    {
        class Data : IImageData
        {
            readonly Gst.Buffer FBuffer;
            readonly MapInfo FMapInfo;

            public Data(Gst.Buffer buffer, int scanSize)
            {
                FBuffer = buffer;
                ScanSize = scanSize;
                buffer.Map(out FMapInfo, MapFlags.Read);
            }

            public IntPtr Pointer => FMapInfo.DataPtr;

            public int ScanSize { get; }

            public int Size => (int)FMapInfo.Size;

            public void Dispose()
            {
                FBuffer.Unmap(FMapInfo);
            }
        }

        readonly Gst.Buffer FBuffer;
        readonly Sample FSample;
        readonly ImageInfo FInfo;
        bool FIsDisposed;

        public Image(Sample sample)
            : base()
        {
            FSample = sample;
            FBuffer = sample.Buffer;
            using (var caps = sample.Caps)
            using (var structure = caps.GetStructure(0))
            {
                int width, height;
                structure.GetInt("width", out width);
                structure.GetInt("height", out height);
                var format = structure.GetString("format");
                var videoFormat = format.ToVideoFormat();
                var pixelFormat = videoFormat.ToPixelFormat();
                FInfo = new ImageInfo(width, height, pixelFormat);
            }
        }

        public ImageInfo Info => !FIsDisposed ? FInfo : ImageExtensions.Default.Info;
        public bool IsDisposed => FSample == null;
        public bool IsVolatile => true;

        public void Dispose()
        {
            if (!FIsDisposed)
            {
                FIsDisposed = true;
                FBuffer.Dispose();
            }
        }

        public IImageData GetData() => !FIsDisposed ? new Data(FBuffer, Info.ScanSize) : ImageExtensions.Default.GetData();
    }
}
