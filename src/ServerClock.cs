using Gst;
using Gst.Net;
using System;

namespace VL.Lib.GStreamer
{
    public class ServerClock : IDisposable
    {
        static ServerClock()
        {
            Initialization.Init();
        }

        string FAddress;
        int FPort;
        Clock FClock;
        NetTimeProvider FTimeprovider;

        public Clock Update(Clock clock = null, string address = null, int port = 4449)
        {
            if (FTimeprovider == null || clock != FClock || address != FAddress || port != FPort)
            {
                FClock = clock;
                FAddress = address;
                FPort = port;

                FTimeprovider?.Dispose();
                FTimeprovider = new NetTimeProvider(
                    clock ?? SystemClock.Obtain(), 
                    string.IsNullOrWhiteSpace(address) ? null : address, 
                    port);
            }
            return FTimeprovider.Clock;
        }

        public string Address => FTimeprovider.Address;
        public int Port => FTimeprovider.Port;
        public Clock Clock => FTimeprovider.Clock;

        public void Dispose()
        {
            FTimeprovider?.Dispose();
        }
    }
}
