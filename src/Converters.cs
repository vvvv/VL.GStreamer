using Gst.Video;
using System;
using VL.Lib.Basics.Imaging;

namespace VL.Lib.GStreamer
{
    public static class Converters
    {
        public static VideoFormat ToVideoFormat(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Unknown:
                    return VideoFormat.Unknown;
                case PixelFormat.R8:
                    return VideoFormat.Gray8;
                case PixelFormat.R8G8B8:
                    return VideoFormat.Rgb;
                case PixelFormat.R8G8B8X8:
                    return VideoFormat.Rgbx;
                case PixelFormat.R8G8B8A8:
                    return VideoFormat.Rgba;
                case PixelFormat.B8G8R8X8:
                    return VideoFormat.Bgrx;
                case PixelFormat.B8G8R8A8:
                    return VideoFormat.Bgra;
                default:
                    throw new UnsupportedPixelFormatException(format);
            }
        }

        public static PixelFormat ToPixelFormat(this VideoFormat format)
        {
            switch (format)
            {
                case VideoFormat.Unknown:
                    return PixelFormat.Unknown;
                case VideoFormat.Encoded:
                    break;
                case VideoFormat.I420:
                    break;
                case VideoFormat.Yv12:
                    break;
                case VideoFormat.Yuy2:
                    break;
                case VideoFormat.Uyvy:
                    break;
                case VideoFormat.Ayuv:
                    break;
                case VideoFormat.Rgbx:
                    return PixelFormat.R8G8B8X8;
                case VideoFormat.Bgrx:
                    return PixelFormat.B8G8R8X8;
                case VideoFormat.Xrgb:
                    break;
                case VideoFormat.Xbgr:
                    break;
                case VideoFormat.Rgba:
                    return PixelFormat.R8G8B8A8;
                case VideoFormat.Bgra:
                    return PixelFormat.B8G8R8A8;
                case VideoFormat.Argb:
                    break;
                case VideoFormat.Abgr:
                    break;
                case VideoFormat.Rgb:
                    return PixelFormat.R8G8B8;
                case VideoFormat.Bgr:
                    break;
                case VideoFormat.Y41b:
                    break;
                case VideoFormat.Y42b:
                    break;
                case VideoFormat.Yvyu:
                    break;
                case VideoFormat.Y444:
                    break;
                case VideoFormat.V210:
                    break;
                case VideoFormat.V216:
                    break;
                case VideoFormat.Nv12:
                    break;
                case VideoFormat.Nv21:
                    break;
                case VideoFormat.Gray8:
                    return PixelFormat.R8;
                case VideoFormat.Gray16Be:
                    break;
                case VideoFormat.Gray16Le:
                    break;
                case VideoFormat.V308:
                    break;
                case VideoFormat.Rgb16:
                    break;
                case VideoFormat.Bgr16:
                    break;
                case VideoFormat.Rgb15:
                    break;
                case VideoFormat.Bgr15:
                    break;
                case VideoFormat.Uyvp:
                    break;
                case VideoFormat.A420:
                    break;
                case VideoFormat.Rgb8p:
                    break;
                case VideoFormat.Yuv9:
                    break;
                case VideoFormat.Yvu9:
                    break;
                case VideoFormat.Iyu1:
                    break;
                case VideoFormat.Argb64:
                    break;
                case VideoFormat.Ayuv64:
                    break;
                case VideoFormat.R210:
                    break;
                case VideoFormat.I42010be:
                    break;
                case VideoFormat.I42010le:
                    break;
                case VideoFormat.I42210be:
                    break;
                case VideoFormat.I42210le:
                    break;
                case VideoFormat.Y44410be:
                    break;
                case VideoFormat.Y44410le:
                    break;
                case VideoFormat.Gbr:
                    break;
                case VideoFormat.Gbr10be:
                    break;
                case VideoFormat.Gbr10le:
                    break;
                case VideoFormat.Nv16:
                    break;
                case VideoFormat.Nv24:
                    break;
                case VideoFormat.Nv1264z32:
                    break;
                case VideoFormat.A42010be:
                    break;
                case VideoFormat.A42010le:
                    break;
                case VideoFormat.A42210be:
                    break;
                case VideoFormat.A42210le:
                    break;
                case VideoFormat.A44410be:
                    break;
                case VideoFormat.A44410le:
                    break;
                case VideoFormat.Nv61:
                    break;
                case VideoFormat.P01010be:
                    break;
                case VideoFormat.P01010le:
                    break;
                case VideoFormat.Iyu2:
                    break;
                case VideoFormat.Vyuy:
                    break;
                case VideoFormat.Gbra:
                    break;
                case VideoFormat.Gbra10be:
                    break;
                case VideoFormat.Gbra10le:
                    break;
                case VideoFormat.Gbr12be:
                    break;
                case VideoFormat.Gbr12le:
                    break;
                case VideoFormat.Gbra12be:
                    break;
                case VideoFormat.Gbra12le:
                    break;
                case VideoFormat.I42012be:
                    break;
                case VideoFormat.I42012le:
                    break;
                case VideoFormat.I42212be:
                    break;
                case VideoFormat.I42212le:
                    break;
                case VideoFormat.Y44412be:
                    break;
                case VideoFormat.Y44412le:
                    break;
                default:
                    break;
            }
            return PixelFormat.Unknown;
        }

        public static string ToFormatString(this VideoFormat format)
        {
            switch (format)
            {
                case VideoFormat.Rgbx:
                    return "RGBx";
                case VideoFormat.Bgrx:
                    return "BGRx";
                case VideoFormat.Xrgb:
                    return "xRGB";
                case VideoFormat.Xbgr:
                    return "xBGR";
                case VideoFormat.Gray16Be:
                    return "GRAY16_BE";
                case VideoFormat.Gray16Le:
                    return "GRAY16_LE";
                case VideoFormat.V308:
                    return "v308";
                case VideoFormat.R210:
                    return "r210";
                case VideoFormat.I42010be:
                    return "I420_10BE";
                case VideoFormat.I42010le:
                    return "I420_10LE";
                case VideoFormat.I42210be:
                    return "I422_10BE";
                case VideoFormat.I42210le:
                    return "I422_10LE";
                case VideoFormat.Y44410be:
                    return "I444_10BE";
                case VideoFormat.Y44410le:
                    return "I444_10LE";
                case VideoFormat.Gbr10be:
                    return "GBR_10BE";
                case VideoFormat.Gbr10le:
                    return "GBR_10LE";
                case VideoFormat.Nv1264z32:
                    return "NV12_64Z32";
                case VideoFormat.A42010be:
                    return "A420_10BE";
                case VideoFormat.A42010le:
                    return "A420_10LE";
                case VideoFormat.A42210be:
                    return "A422_10BE";
                case VideoFormat.A42210le:
                    return "A422_10LE";
                case VideoFormat.A44410be:
                    return "A444_10BE";
                case VideoFormat.A44410le:
                    return "A444_10LE";
                case VideoFormat.P01010be:
                    return "P010_10BE";
                case VideoFormat.P01010le:
                    return "P010_10LE";
                case VideoFormat.Gbra10be:
                    return "GBRA_10BE";
                case VideoFormat.Gbra10le:
                    return "GBRA_10LE";
                case VideoFormat.Gbr12be:
                    return "GBR_12BE";
                case VideoFormat.Gbr12le:
                    return "GBR_12LE";
                case VideoFormat.Gbra12be:
                    return "GBRA_12BE";
                case VideoFormat.Gbra12le:
                    return "GBRA_12LE";
                case VideoFormat.I42012be:
                    return "I420_12BE";
                case VideoFormat.I42012le:
                    return "I420_12LE";
                case VideoFormat.I42212be:
                    return "I422_12BE";
                case VideoFormat.I42212le:
                    return "I422_12LE";
                case VideoFormat.Y44412be:
                    return "I444_12BE";
                case VideoFormat.Y44412le:
                    return "I444_12LE";
                default:
                    return format.ToString().ToUpperInvariant();
            }
        }

        public static VideoFormat ToVideoFormat(this string format)
        {
            switch (format)
            {
                case "GRAY16_BE":
                    return VideoFormat.Gray16Be;
                case "GRAY16_LE":
                    return VideoFormat.Gray16Le;
                case "I420_10BE":
                    return VideoFormat.I42010be;
                case "I420_10LE":
                    return VideoFormat.I42010le;
                case "I422_10BE":
                    return VideoFormat.I42210be;
                case "I422_10LE":
                    return VideoFormat.I42210le;
                case "I444_10BE":
                    return VideoFormat.Y44410be;
                case "I444_10LE":
                    return VideoFormat.Y44410le;
                case "GBR_10BE":
                    return VideoFormat.Gbr10be;
                case "GBR_10LE":
                    return VideoFormat.Gbr10le;
                case "NV12_64Z32":
                    return VideoFormat.Nv1264z32;
                case "A420_10BE":
                    return VideoFormat.A42010be;
                case "A420_10LE":
                    return VideoFormat.A42010le;
                case "A422_10BE":
                    return VideoFormat.A42210be;
                case "A422_10LE":
                    return VideoFormat.A42210le;
                case "A444_10BE":
                    return VideoFormat.A44410be;
                case "A444_10LE":
                    return VideoFormat.A44410le;
                case "P010_10BE":
                    return VideoFormat.P01010be;
                case "P010_10LE":
                    return VideoFormat.P01010le;
                case "GBRA_10BE":
                    return VideoFormat.Gbra10be;
                case "GBRA_10LE":
                    return VideoFormat.Gbra10le;
                case "GBR_12BE":
                    return VideoFormat.Gbr12be;
                case "GBR_12LE":
                    return VideoFormat.Gbr12le;
                case "GBRA_12BE":
                    return VideoFormat.Gbra12be;
                case "GBRA_12LE":
                    return VideoFormat.Gbra12le;
                case "I420_12BE":
                    return VideoFormat.I42012be;
                case "I420_12LE":
                    return VideoFormat.I42012le;
                case "I422_12BE":
                    return VideoFormat.I42212be;
                case "I422_12LE":
                    return VideoFormat.I42212le;
                case "I444_12BE":
                    return VideoFormat.Y44412be;
                case "I444_12LE":
                    return VideoFormat.Y44412le;
                default:
                    VideoFormat result;
                    if (Enum.TryParse(format, true, out result))
                        return result;
                    return VideoFormat.Unknown;
            }
        }
    }
}
