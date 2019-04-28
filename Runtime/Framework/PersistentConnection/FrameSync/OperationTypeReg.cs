using System;
using System.Collections.Generic;

namespace Capstones.Net.FrameSync
{
    public static partial class OperationType
    {
        static OperationType()
        {
            _FrameSyncBeginProtocol = typeof(Protocols.FrameSyncBegin);
            _FrameSyncTickProtocol = typeof(Protocols.FrameSyncTick);
            _FrameSyncEndProtocol = typeof(Protocols.FrameSyncEnd);
            _FrameSyncProtocols.Add(typeof(Protocols.RunToBaseResp));
            // Delegates
            FuncGetFrameSyncBeginInterval = obj => (int)((Protocols.FrameSyncBegin)obj).Interval;
            FuncGetFrameSyncBeginIndex = obj => (int)((Protocols.FrameSyncBegin)obj).Index;
            FuncGetFrameSyncBeginTime = obj => (int)((Protocols.FrameSyncBegin)obj).Time;
            FuncGetFrameSyncTickInterval = obj => (int)((Protocols.FrameSyncTick)obj).Interval;
            FuncGetFrameSyncTickTime = obj => (int)((Protocols.FrameSyncTick)obj).Time;
        }
    }
}
