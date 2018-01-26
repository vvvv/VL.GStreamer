using Gst;
using Gst.App;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using VL.Lib.Basics.Imaging;
using VL.Lib.Basics.Resources;

namespace VL.Lib.GStreamer
{
    public class Player : IDisposable
    {
        static Player()
        {

        }

        private readonly Pipeline playbin;
        private readonly AppSink videosink;
        private readonly Element audiosink;
        private readonly IObservable<IResourceProvider<IImage>> videoFrames;

        private string FUri;
        private string FFormat;
        private float FSeekTime;
        private bool FPlay;
        private long FDuration = -1;
        private State FState;

        public Player()
        {
            // Create the empty pipeline
            playbin = ElementFactory.Make("playbin") as Pipeline;
            using (var bus = playbin.Bus)
            {
                bus.AddSignalWatch();
                bus.Message += Bus_Message;
            }
            
            // TODO: Figure out why audio has such high CPU usage - so disable it for now.
            audiosink = ElementFactory.Make("autoaudiosink", "audiosink");
            videosink = new AppSink("videosink");

            if (playbin == null || videosink == null || audiosink == null)
                throw new Exception("Not all elements could be created");

            videosink.Sync = true;
            videosink.Qos = true;
            videosink.Drop = true;
            videosink.Caps = Caps.FromString("video/x-raw, format=BGRx");
            videosink.MaxBuffers = 2;
            videosink.EmitSignals = true;

            playbin["video-sink"] = videosink;
            playbin["audio-sink"] = audiosink;

            var eosSignals = Observable.FromEventPattern<System.EventArgs>(videosink, nameof(AppSink.Eos));
            var newPrerolls = Observable.FromEventPattern<NewPrerollArgs>(videosink, nameof(AppSink.NewPreroll));
            var newSamples = Observable.FromEventPattern<NewSampleArgs>(videosink, nameof(AppSink.NewSample));
            videoFrames = newSamples.Select(a => videosink.PullSample())
                .Where(s => s != null)
                .Select(s => ResourceProvider.New(() => new Image(s))
                    // The frame is read-only so share it in parallel until a new sample arrives or an EOS is received
                    .ShareInParallel(Observable.Merge<object>(newSamples.Skip(1), newPrerolls, eosSignals)))
                // Keep a single subscription
                .Publish().RefCount();
        }

        private void Bus_Message(object o, MessageArgs args)
        {
            var msg = args.Message;
            Trace.TraceInformation($"{msg.Type}: {msg}");
            switch (msg.Type)
            {
                case MessageType.Unknown:
                    break;
                case MessageType.Eos:
                    break;
                case MessageType.Error:
                    break;
                case MessageType.Warning:
                    break;
                case MessageType.Info:
                    break;
                case MessageType.Tag:
                    break;
                case MessageType.Buffering:
                    break;
                case MessageType.StateChanged:
                    State oldState, newState, pending;
                    msg.ParseStateChanged(out oldState, out newState, out pending);
                    FState = newState;
                    break;
                case MessageType.StateDirty:
                    break;
                case MessageType.StepDone:
                    break;
                case MessageType.ClockProvide:
                    break;
                case MessageType.ClockLost:
                    break;
                case MessageType.NewClock:
                    break;
                case MessageType.StructureChange:
                    break;
                case MessageType.StreamStatus:
                    break;
                case MessageType.Application:
                    break;
                case MessageType.Element:
                    break;
                case MessageType.SegmentStart:
                    break;
                case MessageType.SegmentDone:
                    break;
                case MessageType.DurationChanged:
                    FDuration = -1;
                    break;
                case MessageType.Latency:
                    break;
                case MessageType.AsyncStart:
                    break;
                case MessageType.AsyncDone:
                    break;
                case MessageType.RequestState:
                    break;
                case MessageType.StepStart:
                    break;
                case MessageType.Qos:
                    break;
                case MessageType.Progress:
                    break;
                case MessageType.Toc:
                    break;
                case MessageType.ResetTime:
                    break;
                case MessageType.StreamStart:
                    break;
                case MessageType.NeedContext:
                    break;
                case MessageType.HaveContext:
                    break;
                case MessageType.Extended:
                    break;
                case MessageType.DeviceAdded:
                    break;
                case MessageType.DeviceRemoved:
                    break;
                case MessageType.PropertyNotify:
                    break;
                case MessageType.StreamCollection:
                    break;
                case MessageType.StreamsSelected:
                    break;
                case MessageType.Redirect:
                    break;
                case MessageType.Any:
                    break;
                default:
                    break;
            }
        }

        public IObservable<IResourceProvider<IImage>> VideoFrames => videoFrames;
        public IObservable<System.IO.Stream> AudioFrames { get; } = Observable.Empty<System.IO.Stream>();

        public void Update(
            out State state,
            out float position,
            out float duration,
            string uri = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4", 
            string format = "BGRx", 
            float seekTime = 0,
            bool play = false)
        {
            if (!Util.UriIsValid(uri))
            {
                System.Uri _uri;
                if (System.Uri.TryCreate(uri, UriKind.Absolute, out _uri))
                    uri = _uri.AbsoluteUri;
            }
            if (uri != FUri)
            {
                FUri = uri;
                playbin["uri"] = uri;
            }

            if (format != FFormat)
            {
                FFormat = format;
                videosink.Caps = Caps.FromString($"video/x-raw, format={format}");
            }

            if (seekTime != FSeekTime)
            {
                FSeekTime = seekTime;
                playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, (long)(seekTime * Gst.Constants.SECOND));
                //using (var query = Query.NewSeeking(Format.Time))
                //{
                //    if (playbin.Query(query))
                //    {
                //        Format f;
                //        bool seekable;
                //        long segmentStart, segmentEnd;
                //        query.ParseSeeking(out f, out seekable, out segmentStart, out segmentEnd);
                //        if (seekable)
                //            playbin.SeekSimple(Format.Time, SeekFlags.Flush | SeekFlags.KeyUnit, (long)(seekTime * Gst.Constants.SECOND));
                //    }
                //}
            }

            if (play != FPlay)
            {
                FPlay = play;
                if (play)
                    playbin.SetState(State.Playing);
                else
                    playbin.SetState(State.Paused);
            }

            state = FState;
            long p, q;
            if (state >= State.Paused)
            {
                playbin.QueryPosition(Format.Time, out p);
                position = p / Gst.Constants.SECOND;

                if (FDuration == -1)
                {
                    playbin.QueryDuration(Format.Time, out q);
                    FDuration = q / Gst.Constants.SECOND;
                }

                duration = FDuration;
            }
            else
            {
                position = -1;
                duration = -1;
            }

            // This is the blocking approach
            //if (!videosink.IsEos)
            //{
            //    using (var sample = videosink.PullSample())
            //    {
            //        var provider = CreateProvider(sample);
            //        // Fix for https://bugzilla.gnome.org/show_bug.cgi?id=677708
            //        gst_mini_object_unref(sample.Handle);
            //        return provider;
            //    }
            //}
            //return ResourceProvider.Return(VideoFrame.Empty);
        }

        public void Dispose()
        {
            // Free resources
            playbin.SetState(State.Null);
            videosink.Dispose();
            audiosink.Dispose();
            using (var bus = playbin.Bus)
                bus.Message -= Bus_Message;
            playbin.Dispose();
        }
    }
}
