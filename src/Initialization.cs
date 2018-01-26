using Gst;
using System;
using System.IO;
using System.Threading;

namespace VL.Lib
{
    // Importer will look for static class VL.Lib.Initialization in each assembly and invoke its static constructor
    static class Initialization
    {
        static Initialization()
        {
            // Initialize Gstreamer
            var args = System.Array.Empty<string>();
            var path = Environment.GetEnvironmentVariable("PATH") ?? String.Empty;
            Environment.SetEnvironmentVariable("PATH", path + Path.PathSeparator + @"C:\gstreamer\1.0\x86_64\bin");
            Application.Init(ref args);
            System.Windows.Forms.Application.Idle += Application_Idle;
        }

        private static void Application_Idle(object sender, EventArgs e)
        {
            var context = GLib.MainContext.Default;
            if (context.HasPendingEvents)
                context.RunIteration(may_block: true);
        }
    }
}
