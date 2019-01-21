using Gst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VL.GStreamer
{
    class StateChangeException : Exception
    {

    }

    static class Extensions
    {
        public static void SetStateBlocking(this Element element, State state)
        {
            var stateReturn = element.SetState(state);
            switch (stateReturn)
            {
                case StateChangeReturn.Failure:
                    throw new StateChangeException();
                case StateChangeReturn.Success:
                    return;
                case StateChangeReturn.Async:
                    while (true)
                    {
                        switch (element.GetState(out var currentState, out var pendingState, Constants.CLOCK_TIME_NONE))
                        {
                            case StateChangeReturn.Failure:
                                throw new StateChangeException();
                            case StateChangeReturn.Success:
                                return;
                            case StateChangeReturn.Async:
                                continue;
                            case StateChangeReturn.NoPreroll:
                                return;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                case StateChangeReturn.NoPreroll:
                    return;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
