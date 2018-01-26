using Gst;
using System;
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

        readonly Sample FSample;
        readonly Gst.Buffer FBuffer;

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
                Info = new ImageInfo(width, height, PixelFormat.X8R8G8B8);
            }
            
        }

        public ImageInfo Info { get; }

        public void Dispose()
        {
            FBuffer.Dispose();
            FSample.Dispose();
        }

        public IImageData GetData() => new Data(FBuffer, Info.ScanSize);
    }
}
