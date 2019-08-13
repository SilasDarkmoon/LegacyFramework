namespace Capstones.Net
{
    public partial class ProtobufReaderAndWriter
    {
        private static RegisteredType _Reg_ServerStatusOp = new RegisteredType(1001, typeof(Protocols.ServerStatusOp), Protocols.ServerStatusOp.Parser);
        private static RegisteredType _Reg_ServerStatusResp = new RegisteredType(1002, typeof(Protocols.ServerStatusResp), Protocols.ServerStatusResp.Parser);
        private static RegisteredType _Reg_Nop = new RegisteredType(101, typeof(Protocols.Nop), Protocols.Nop.Parser);
        private static RegisteredType _Reg_Reset = new RegisteredType(102, typeof(Protocols.Reset), Protocols.Reset.Parser);
        private static RegisteredType _Reg_OpponentConnected = new RegisteredType(103, typeof(Protocols.OpponentConnected), Protocols.OpponentConnected.Parser);
        private static RegisteredType _Reg_OpponentDisconnected = new RegisteredType(104, typeof(Protocols.OpponentDisconnected), Protocols.OpponentDisconnected.Parser);
        private static RegisteredType _Reg_GamersStatus = new RegisteredType(105, typeof(Protocols.GamersStatus), Protocols.GamersStatus.Parser);
        private static RegisteredType _Reg_ConnectToRoomOp = new RegisteredType(1, typeof(Protocols.ConnectToRoomOp), Protocols.ConnectToRoomOp.Parser);
        private static RegisteredType _Reg_ConnectToRoomResp = new RegisteredType(2, typeof(Protocols.ConnectToRoomResp), Protocols.ConnectToRoomResp.Parser);
        private static RegisteredType _Reg_ChangeSideOp = new RegisteredType(3, typeof(Protocols.ChangeSideOp), Protocols.ChangeSideOp.Parser);
        private static RegisteredType _Reg_ChangeSideQuestion = new RegisteredType(4, typeof(Protocols.ChangeSideQuestion), Protocols.ChangeSideQuestion.Parser);
        private static RegisteredType _Reg_ChangeSideResp = new RegisteredType(5, typeof(Protocols.ChangeSideResp), Protocols.ChangeSideResp.Parser);
        private static RegisteredType _Reg_StartMatchOp = new RegisteredType(6, typeof(Protocols.StartMatchOp), Protocols.StartMatchOp.Parser);
        private static RegisteredType _Reg_StartMatchQuestion = new RegisteredType(7, typeof(Protocols.StartMatchQuestion), Protocols.StartMatchQuestion.Parser);
        private static RegisteredType _Reg_StartMatchResp = new RegisteredType(8, typeof(Protocols.StartMatchResp), Protocols.StartMatchResp.Parser);
        private static RegisteredType _Reg_NextBatterInfo = new RegisteredType(9, typeof(Protocols.NextBatterInfo), Protocols.NextBatterInfo.Parser);
        private static RegisteredType _Reg_SelectPitchOp = new RegisteredType(10, typeof(Protocols.SelectPitchOp), Protocols.SelectPitchOp.Parser);
        private static RegisteredType _Reg_SelectPitchResp = new RegisteredType(11, typeof(Protocols.SelectPitchResp), Protocols.SelectPitchResp.Parser);
        private static RegisteredType _Reg_DominatePitchOp = new RegisteredType(12, typeof(Protocols.DominatePitchOp), Protocols.DominatePitchOp.Parser);
        private static RegisteredType _Reg_DominatePitchResp = new RegisteredType(13, typeof(Protocols.DominatePitchResp), Protocols.DominatePitchResp.Parser);
        private static RegisteredType _Reg_BatOp = new RegisteredType(14, typeof(Protocols.BatOp), Protocols.BatOp.Parser);
        private static RegisteredType _Reg_BatResp = new RegisteredType(15, typeof(Protocols.BatResp), Protocols.BatResp.Parser);
        private static RegisteredType _Reg_DominateBatOp = new RegisteredType(16, typeof(Protocols.DominateBatOp), Protocols.DominateBatOp.Parser);
        private static RegisteredType _Reg_DominateBatResp = new RegisteredType(17, typeof(Protocols.DominateBatResp), Protocols.DominateBatResp.Parser);
        private static RegisteredType _Reg_SetDominateOp = new RegisteredType(18, typeof(Protocols.SetDominateOp), Protocols.SetDominateOp.Parser);
        private static RegisteredType _Reg_SetDominateEvent = new RegisteredType(19, typeof(Protocols.SetDominateEvent), Protocols.SetDominateEvent.Parser);
        private static RegisteredType _Reg_SetStealBaseOp = new RegisteredType(20, typeof(Protocols.SetStealBaseOp), Protocols.SetStealBaseOp.Parser);
        private static RegisteredType _Reg_SetStealBaseEvent = new RegisteredType(21, typeof(Protocols.SetStealBaseEvent), Protocols.SetStealBaseEvent.Parser);
        private static RegisteredType _Reg_SetBattingModeOp = new RegisteredType(22, typeof(Protocols.SetBattingModeOp), Protocols.SetBattingModeOp.Parser);
        private static RegisteredType _Reg_SetBattingModeEvent = new RegisteredType(23, typeof(Protocols.SetBattingModeEvent), Protocols.SetBattingModeEvent.Parser);
        private static RegisteredType _Reg_MoveToNextStepOp = new RegisteredType(24, typeof(Protocols.MoveToNextStepOp), Protocols.MoveToNextStepOp.Parser);
        private static RegisteredType _Reg_MoveToNextStepResp = new RegisteredType(25, typeof(Protocols.MoveToNextStepResp), Protocols.MoveToNextStepResp.Parser);
        private static RegisteredType _Reg_PitchPrepareOp = new RegisteredType(26, typeof(Protocols.PitchPrepareOp), Protocols.PitchPrepareOp.Parser);
        private static RegisteredType _Reg_PitchReadyResp = new RegisteredType(27, typeof(Protocols.PitchReadyResp), Protocols.PitchReadyResp.Parser);
        private static RegisteredType _Reg_BatDoneOp = new RegisteredType(28, typeof(Protocols.BatDoneOp), Protocols.BatDoneOp.Parser);
        private static RegisteredType _Reg_VainSwingOp = new RegisteredType(29, typeof(Protocols.VainSwingOp), Protocols.VainSwingOp.Parser);
        private static RegisteredType _Reg_VainSwingEvent = new RegisteredType(30, typeof(Protocols.VainSwingEvent), Protocols.VainSwingEvent.Parser);
        private static RegisteredType _Reg_PitcherUrgeEvent = new RegisteredType(31, typeof(Protocols.PitcherUrgeEvent), Protocols.PitcherUrgeEvent.Parser);
        private static RegisteredType _Reg_BeginBatSwingOp = new RegisteredType(32, typeof(Protocols.BeginBatSwingOp), Protocols.BeginBatSwingOp.Parser);
        private static RegisteredType _Reg_BeginBatSwingEvent = new RegisteredType(33, typeof(Protocols.BeginBatSwingEvent), Protocols.BeginBatSwingEvent.Parser);
        private static RegisteredType _Reg_SelectingBattingTargetEvent = new RegisteredType(34, typeof(Protocols.SelectingBattingTargetEvent), Protocols.SelectingBattingTargetEvent.Parser);
        private static RegisteredType _Reg_FrameSyncBegin = new RegisteredType(2001, typeof(Protocols.FrameSyncBegin), Protocols.FrameSyncBegin.Parser);
        private static RegisteredType _Reg_FrameSyncTick = new RegisteredType(2002, typeof(Protocols.FrameSyncTick), Protocols.FrameSyncTick.Parser);
        private static RegisteredType _Reg_FrameSyncEnd = new RegisteredType(2003, typeof(Protocols.FrameSyncEnd), Protocols.FrameSyncEnd.Parser);
        private static RegisteredType _Reg_RunToBaseReq = new RegisteredType(2004, typeof(Protocols.RunToBaseReq), Protocols.RunToBaseReq.Parser);
        private static RegisteredType _Reg_RunToBaseResp = new RegisteredType(2005, typeof(Protocols.RunToBaseResp), Protocols.RunToBaseResp.Parser);

        private static void AOT_ProtocEnums()
        {
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.TeamSide>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.InningHalf>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.BattingMode>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.TrajectoryType>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.PitchType>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.PitchTypeGrade>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.ManualPitchPuzzleType>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.PitchResult>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.BatDir>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.RunFrameType>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.Role>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.OnFieldRole>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.HandType>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.BatOperationResultType>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.BatOpType>();
            Google.Protobuf.Reflection.ReflectionUtil.ForceInitialize<Protocols.DominateType>();
        }
    }
}
