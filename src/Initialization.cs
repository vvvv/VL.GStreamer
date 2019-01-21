using Gst;
using System;
using System.IO;

namespace VL.Lib.GStreamer
{
    class Initialization
    {
        static Initialization()
        {
            // Initialize Gstreamer
            var args = System.Array.Empty<string>();
            var path = Environment.GetEnvironmentVariable("PATH") ?? String.Empty;
            var folder = IntPtr.Size == 4 ? "x86" : "x86_64";
            Environment.SetEnvironmentVariable("PATH", path + Path.PathSeparator + $@"C:\gstreamer\1.0\{folder}\bin");
            Application.Init(ref args);
            System.Windows.Forms.Application.Idle += Application_Idle;
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            var context = GLib.MainContext.Default;
            while (context.HasPendingEvents)
                context.RunIteration();
        }

        public static void Init() { }
    }
}
