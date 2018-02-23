using Gst;
using Gst.Net;
using System;
using Constants = Gst.Constants;

namespace VL.Lib.GStreamer
{
    public class ClientClock : IDisposable
    {
        static ClientClock()
        {
            Initialization.Init();
        }

        Clock FClock;
        string FAddress;
        int FPort;
        double FBaseTime;

        public Clock Update(string address = "127.0.0.1", int port = 4449, double baseTime = 0d)
        {
            if (FClock == null || FAddress != address || FPort != port || FBaseTime != baseTime)
            {
                FAddress = address;
                FPort = port;
                FBaseTime = baseTime;

                FClock?.Dispose();
                FClock = new NetClientClock(null, address, port, (ulong)(baseTime * Constants.SECOND));
            }
            return FClock;
        }

        public void Dispose()
        {
            FClock?.Dispose();
        }
    }
}
