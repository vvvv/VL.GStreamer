using Gst;
using Gst.App;
using Gst.Video;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using VL.GStreamer;
using VL.Lib.Basics.Imaging;
using Constants = Gst.Constants;

namespace VL.Lib.GStreamer
{
    public class Player : IDisposable
    {
        static Player()
        {
            Initialization.Init();
        }

        readonly Subject<IImage> videoFrames = new Subject<IImage>();
        readonly Pipeline playbin;
        readonly Bus bus;
        readonly AppSink videosink;
        readonly Element audiosink;

        string FUri;
        bool FSeeking, FPlay, FLoop, FEos;
        long FPosition;
        long FDuration = -1;
        float FRate = 1f;
        long FLoopStartTime = 0;
        long FLoopEndTime = -1;
        State FTargetState;
        float FVolume;

        public Player(VideoFormat format = VideoFormat.Bgra)
        {
            // Create the empty pipeline
            playbin = ElementFactory.Make("playbin3") as Pipeline;
            bus = playbin.Bus;
            bus.AddSignalWatch();
            bus.EnableSyncMessageEmission();
            bus.Message += Bus_Message;
            
            audiosink = ElementFactory.Make("autoaudiosink", "audiosink");
            videosink = new AppSink("videosink");

            if (playbin == null || videosink == null || audiosink == null)
                throw new Exception("Not all elements could be created");

            videosink.Sync = true;
            videosink.Qos = false;
            videosink.Drop = false;
            var formatString = (format != VideoFormat.Unknown ? format : VideoFormat.Bgrx).ToFormatString();
            videosink.Caps = Caps.FromString($"video/x-raw, format={formatString}");
            videosink.MaxBuffers = 1;
            videosink.EmitSignals = true;
            videosink.NewPreroll += Videosink_NewPreroll;
            videosink.NewSample += Videosink_NewSample;

            playbin["video-sink"] = videosink;
            playbin["audio-sink"] = audiosink;
            playbin.SetState(FTargetState = State.Ready);
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
                    FEos = true;
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
                    msg.ParseStateChanged(out var oldState, out var newState, out var pending);
                    FEos = false;
                    //if (newState == State.Playing)
                    //{
                    //    // Once we are in the playing state, analyze the streams
                    //    AnalyzeStreams();
                    //}
                    break;
                case MessageType.StateDirty:
                    break;
                case MessageType.StepDone:
                    {
                        ulong amount, duration;
                        double rate;
                        bool flush, intermediate, eos;
                        msg.ParseStepDone(out Format format, out amount, out rate, out flush, out intermediate, out duration, out eos);
                        break;
                    }
                case MessageType.ClockProvide:
                    //Clock c;
                    //bool retry;
                    //msg.ParseClockProvide(out c, out retry);
                    break;
                case MessageType.ClockLost:
                    break;
                case MessageType.NewClock:
                    //var newClock = msg.ParseNewClock();

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
                    {
                        var seekFlags = SeekFlags.Segment | SeekFlags.Accurate;
                        msg.ParseSegmentDone(out Format format, out long position);
                        if (FLoop && FLoopStartTime <= position && (position <= FLoopEndTime || FLoopEndTime < 0))
                        {
                            if (FRate >= 0)
                            {
                                if (position < FLoopEndTime)
                                    // The loop end time was increased so continue from current position
                                    Seek(seekFlags, SeekType.Set, position, SeekType.Set, FLoopEndTime);
                                else
                                    Seek(seekFlags, SeekType.Set, FLoopStartTime, SeekType.Set, FLoopEndTime);
                            }
                            else
                            {
                                if (FLoopStartTime < position)
                                    // The loop start time was decreased so continune from current position
                                    Seek(seekFlags, SeekType.Set, FLoopStartTime, SeekType.Set, position);
                                else
                                    Seek(seekFlags, SeekType.Set, FLoopStartTime, SeekType.Set, FLoopEndTime);
                            }
                        }
                        else
                        {
                            if (FRate > 0)
                            {
                                if (position < FDuration)
                                    Seek(seekFlags, SeekType.Set, position, SeekType.Set, -1);
                            }
                            else
                            {
                                if (position > 0)
                                    Seek(seekFlags, SeekType.Set, 0, SeekType.Set, position);
                            }
                        }
                        break;
                    }
                case MessageType.DurationChanged:
                    playbin.QueryDuration(Format.Time, out FDuration);
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
            float rate = 1f,
            bool step = false,
            float seekTime = 0f,
            bool seek = false,
            float loopStartTime = 0f,
            float loopEndTime = -1f,
            bool loop = false,
            float volume = 1f)
        {
            // Media to play
            var newStream = false;
            if (!Util.UriIsValid(uri))
            {
                System.Uri _uri;
                if (System.Uri.TryCreate(uri, UriKind.Absolute, out _uri))
                    uri = _uri.AbsoluteUri;
            }
            if (uri != FUri)
            {
                FUri = uri;
                SwitchState(State.Ready);
                playbin["uri"] = uri;
                newStream = true;
            }

            if (volume != FVolume)
            {
                FVolume = volume;
                playbin["volume"] = Math.Min(Math.Max(volume, 0), 1f);
            }

            // Segment selection
            var loopStartTime_ = GetSeekStartTime(loopStartTime);
            if (loopStartTime_ != FLoopStartTime)
            {
                // Needs a seek if playing backward
                if (rate < 0 && loopStartTime_ > FLoopStartTime)
                    Seek(SeekFlags.Segment | SeekFlags.Flush, SeekType.Set, loopStartTime_, SeekType.None, 0);
                FLoopStartTime = loopStartTime_;
            }
            var loopEndTime_ = GetSeekEndTime(loopEndTime);
            if (loopEndTime_ != FLoopEndTime)
            {
                // Needs a seek if playing forward
                if (rate > 0 && loopEndTime_ < FLoopEndTime)
                    Seek(SeekFlags.Segment | SeekFlags.Flush, SeekType.None, 0, SeekType.Set, loopEndTime_);
                FLoopEndTime = loopEndTime_;
            }

            // Looping is handled when we receive the SegmentEnd event
            FLoop = loop;

            if (newStream)
            {
                // Go into paused state so we can query duration and perform initial seek
                SwitchState(State.Paused);

                if (playbin.QueryDuration(Format.Time, out FDuration) && FDuration > 0)
                {
                    // Seek to set position
                    seek = true;
                }
            }

            // Playback rate
            if (rate != FRate)
            {
                FRate = rate;
                if (!seek)
                {
                    playbin.QueryPosition(Format.Time, out long p);
                    seekTime = ((float)p) / Constants.SECOND;
                    seek = true;
                }
            }

            // Seeking
            if (seek)
            {
                if (rate >= 0)
                {
                    // Playing forward
                    var start = GetSeekStartTime(seekTime);
                    var stop = start > FLoopEndTime ? -1 : FLoopEndTime;
                    Seek(SeekFlags.Flush | SeekFlags.Accurate | SeekFlags.Segment, SeekType.Set, start, SeekType.Set, stop);
                }
                else
                {
                    // Playing backward
                    var start = GetSeekEndTime(seekTime);
                    var stop = start < FLoopStartTime ? 0 : FLoopStartTime;
                    Seek(SeekFlags.Flush | SeekFlags.Accurate | SeekFlags.Segment, SeekType.Set, stop, SeekType.Set, start);
                }
            }

            // Stepping
            if (step)
            {
                var stepEvent = Event.NewStep(Format.Buffers, 1UL, Math.Abs(FRate), true, false);
                playbin.SendEvent(stepEvent);
            }

            // Play or pause
            if (play != FPlay)
            {
                FPlay = play;
                FTargetState = play ? State.Playing : State.Paused;
                SwitchState(FTargetState);
            }

            // Current position and duration tracking
            if (playbin.QueryPosition(Format.Time, out FPosition))
                position = ((float)FPosition) / Constants.SECOND;
            else
                position = -1;

            if (FDuration > 0)
                duration = ((float)FDuration) / Constants.SECOND;
            else
                duration = -1;

            state = playbin.CurrentState;

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

        private static long GetSeekEndTime(float seekTime)
        {
            if (seekTime < 0f)
                return -1;
            return (long)(seekTime * Constants.SECOND);
        }

        private static long GetSeekStartTime(float seekTime)
        {
            if (seekTime < 0f)
                return 0;
            return (long)(seekTime * Constants.SECOND);
        }

        bool Seek(SeekFlags flags, SeekType start_type, long start, SeekType stop_type, long stop)
        {
            var format = Format.Time;
            // Seeks without the GST_SEEK_FLAG_FLUSH should only be done when the pipeline is in the PLAYING state. 
            // Executing a non-flushing seek in the PAUSED state might deadlock because the pipeline streaming threads might be blocked in the sinks.
            if (playbin.CurrentState == State.Paused/* || FEos || FSeeking*/)
                flags |= SeekFlags.Flush;
            return FSeeking = playbin.Seek(FRate, format, flags, start_type, start, stop_type, stop);
        }

        public Clock Clock => playbin.Clock;

        void SwitchState(State state)
        {
            playbin.SetStateBlocking(state);
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
