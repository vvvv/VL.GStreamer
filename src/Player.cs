using Gst;
using Gst.App;
using Gst.Video;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using VL.Lib.Basics.Imaging;
using Constants = Gst.Constants;

namespace VL.Lib.GStreamer
{
    public class Player : IDisposable
    {
        static Player()
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
            if (context.HasPendingEvents)
                context.RunIteration(may_block: true);
        }

        readonly Subject<IImage> videoFrames = new Subject<IImage>();
        readonly Pipeline playbin;
        readonly Bus bus;
        readonly AppSink videosink;
        readonly Element audiosink;

        string FUri;
        bool FSeeking;
        bool FPlay;
        float FPosition, FDuration = -1f;
        State FState;

        public Player(VideoFormat format = VideoFormat.Bgra)
        {
            // Create the empty pipeline
            playbin = ElementFactory.Make("playbin") as Pipeline;
            bus = playbin.Bus;
            bus.AddSignalWatch();
            bus.Message += Bus_Message;
            
            audiosink = ElementFactory.Make("autoaudiosink", "audiosink");
            videosink = new AppSink("videosink");

            if (playbin == null || videosink == null || audiosink == null)
                throw new Exception("Not all elements could be created");

            videosink.Sync = true;
            videosink.Qos = false;
            videosink.Drop = false;
            var formatString = format.ToFormatString();
            videosink.Caps = Caps.FromString($"video/x-raw, format={formatString}");
            videosink.MaxBuffers = 1;
            videosink.EmitSignals = true;
            videosink.NewPreroll += Videosink_NewPreroll;
            videosink.NewSample += Videosink_NewSample;

            playbin["video-sink"] = videosink;
            playbin["audio-sink"] = audiosink;
        }

        public void Dispose()
        {
            videosink.NewPreroll -= Videosink_NewPreroll;
            videosink.NewSample -= Videosink_NewSample;
            videosink.Dispose();
            audiosink.Dispose();
            videoFrames.Dispose();
            bus.Message -= Bus_Message;
            bus.RemoveSignalWatch();
            bus.Dispose();
            // Free resources
            playbin.SetState(State.Null);
            playbin.Dispose();
        }

        private void Videosink_NewPreroll(object o, NewPrerollArgs args)
        {
            using (var sample = videosink.PullPreroll())
                PushImage(sample);
        }

        private void Videosink_NewSample(object o, NewSampleArgs args)
        {
            using (var sample = videosink.PullSample())
                PushImage(sample);
        }

        private void PushImage(Sample sample)
        {
            if (sample != null)
            {
                using (var image = new Image(sample))
                    videoFrames.OnNext(image);
            }
        }

        private void Bus_Message(object o, MessageArgs args)
        {
            var msg = args.Message;
            //Trace.TraceInformation($"{msg.Type}: {msg}");
            switch (msg.Type)
            {
                case MessageType.Unknown:
                    break;
                case MessageType.Eos:
                    break;
                case MessageType.Error:
                    GLib.GException e;
                    string m;
                    msg.ParseError(out e, out m);
                    Trace.TraceError($"Exception: {e}, Debug: {m}");
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
                    //if (newState == State.Playing)
                    //{
                    //    // Once we are in the playing state, analyze the streams
                    //    AnalyzeStreams();
                    //}
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
                    FSeeking = false;
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

        public IObservable<IImage> VideoFrames => videoFrames;
        public IObservable<System.IO.Stream> AudioFrames { get; } = Observable.Empty<System.IO.Stream>();

        public void Update(
            out State state,
            out float position,
            out float duration,
            string uri = "http://download.blender.org/durian/trailer/sintel_trailer-1080p.mp4", 
            bool play = false,
            bool seek = false,
            float seekTime = 0)
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

            if (play != FPlay)
            {
                FPlay = play;
                if (play)
                    playbin.SetState(State.Playing);
                else
                    playbin.SetState(State.Paused);
            }

            if (seek && !FSeeking)
            {
                FSeeking = true;
                // Seek call can be rather expensive so put to background
                System.Threading.Tasks.Task.Run(() =>
                {
                    if (playbin.SeekSimple(Format.Time, SeekFlags.Flush, (long)(seekTime * Gst.Constants.SECOND)))
                    {
                        FSeeking = true;
                    }
                    else
                        FSeeking = false;
                });
            }

            state = FState;
            if (state >= State.Paused)
            {
                if (!FSeeking)
                {
                    long p;
                    playbin.QueryPosition(Format.Time, out p);
                    FPosition = ((float)p) / Gst.Constants.SECOND;
                }

                if (FDuration == -1)
                {
                    long q;
                    playbin.QueryDuration(Format.Time, out q);
                    FDuration = ((float)q) / Gst.Constants.SECOND;
                }

                position = FPosition;
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

        //// Extract some metadata from the streams and print it on the screen
        //void AnalyzeStreams()
        //{
        //    // Read some properties
        //    var NVideo = (int)playbin["n-video"];
        //    var NAudio = (int)playbin["n-audio"];
        //    var NText = (int)playbin["n-text"];

        //    Trace.TraceInformation("{0} video stream(s), {1} audio stream(s), {2} text stream(s)", NVideo, NAudio, NText);

        //    for (int i = 0; i < NVideo; i++)
        //    {
        //        // Retrieve the stream's video tags
        //        var tags = (TagList)playbin.Emit("get-video-tags", new object[] { i });
        //        if (tags != null)
        //        {
        //            Trace.TraceInformation("video stream {0}", i);
        //            string str;
        //            tags.GetString(Constants.TAG_VIDEO_CODEC, out str);
        //            Trace.TraceInformation("  codec: {0}", str != null ? str : "unknown");
        //        }
        //    }

        //    for (int i = 0; i < NAudio; i++)
        //    {
        //        // Retrieve the stream's audio tags
        //        var tags = (TagList)playbin.Emit("get-audio-tags", new object[] { i });
        //        if (tags != null)
        //        {
        //            Trace.TraceInformation("audio stream {0}", i);
        //            string str;
        //            if (tags.GetString(Constants.TAG_AUDIO_CODEC, out str))
        //            {
        //                Trace.TraceInformation("  codec: {0}", str);
        //            }
        //            if (tags.GetString(Constants.TAG_LANGUAGE_CODE, out str))
        //            {
        //                Trace.TraceInformation("  language: {0}", str);
        //            }
        //            uint rate;
        //            if (tags.GetUint(Constants.TAG_BITRATE, out rate))
        //            {
        //                Trace.TraceInformation("  bitrate: {0}", rate);
        //            }
        //        }
        //    }

        //    for (int i = 0; i < NText; i++)
        //    {
        //        // Retrieve the stream's subtitle tags
        //        var tags = (TagList)playbin.Emit("get-text-tags", new object[] { i });
        //        if (tags != null)
        //        {
        //            Trace.TraceInformation("subtitle stream {0}", i);
        //            string str;
        //            if (tags.GetString(Constants.TAG_LANGUAGE_CODE, out str))
        //            {
        //                Trace.TraceInformation("  language: {0}", str);
        //            }
        //        }
        //    }

        //    var CurrentAudio = (int)playbin["current-audio"];
        //    var CurrentVideo = (int)playbin["current-video"];
        //    var CurrentText = (int)playbin["current-text"];

        //    Trace.TraceInformation("Currently playing video stream {0}, audio stream {1} and text stream {2}", CurrentVideo, CurrentAudio, CurrentText);
        //}
    }
}
