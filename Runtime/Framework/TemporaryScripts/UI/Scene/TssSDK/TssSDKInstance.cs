using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class TssSDKInstance : MonoBehaviour
{
#if !UNITY_EDITOR && TENCENT_RELEASE
    public static uint Msg_C2S_Security_Data_Report = 10000;
    public static uint Msg_S2C_Security_Data_Report_Resp = 10001;
    static float interval = 5;
    private float lastTime;

    public static void Init()
    {
        TssSdk.TssSdkInit(2662u);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            TssSdk.TssSdkSetGameStatus(TssSdk.EGAMESTATUS.GAME_STATUS_BACKEND);
        }
        else
        {
            TssSdk.TssSdkSetGameStatus(TssSdk.EGAMESTATUS.GAME_STATUS_FRONTEND);
        }
    }

    public static void OnGuestLogin(string openId, string appId, int serverid, string roleId)
    {
        TssSdk.TssSdkSetUserInfoEx(TssSdk.EENTRYID.ENTRY_ID_OTHERS, openId, appId, (uint)serverid, roleId);
    }

    public static void OnQQLogin(string openId, string appId, int serverid, string roleId)
    {
        TssSdk.TssSdkSetUserInfoEx(TssSdk.EENTRYID.ENTRY_ID_QZONE, openId, appId, (uint)serverid, roleId);
    }

    public static void OnWeChatLogin(string openId, string appId, int serverid, string roleId)
    {
        TssSdk.TssSdkSetUserInfoEx(TssSdk.EENTRYID.ENTRY_ID_MM, openId, appId, (uint)serverid, roleId);
    }

    public void StartPVP()
    {
        TssSdk.TssSdkSetGameStatus(TssSdk.EGAMESTATUS.GAME_STATUS_START_PVP);
    }

    public void EndPVP()
    {
        TssSdk.TssSdkSetGameStatus(TssSdk.EGAMESTATUS.GAME_STATUS_END_PVP);
    }

    public void UpdateFinished()
    {
        TssSdk.TssSdkSetGameStatus(TssSdk.EGAMESTATUS.GAME_STATUS_UPDATE_FINISHED);
    }

    private void Start()
    {
        lastTime = Time.realtimeSinceStartup;
        Capstones.UnityFramework.Network.ClientSocket.AddCallBackByte(Msg_S2C_Security_Data_Report_Resp, RecieveServerDataToClient);
    }

    private void OnDestroy()
    {
        Capstones.UnityFramework.Network.ClientSocket.RemoveCallBack(Msg_S2C_Security_Data_Report_Resp);
    }

    private void Update()
    {
        if ((Time.realtimeSinceStartup - lastTime) < interval)
        {
            return;
        }
        // 如果socket还没有连接，则不发送安全sdk数据
        if (!Capstones.UnityFramework.Network.ClientSocket.GetSocketConnectState())
        {
            return;
        }

        lastTime = Time.realtimeSinceStartup;

        IntPtr addr = TssSdk.tss_get_report_data();
        if (addr != IntPtr.Zero)
        {
            TssSdk.AntiDataInfo info = new TssSdk.AntiDataInfo();
            if (TssSdk.Is64bit())
            {
                short anti_data_len = Marshal.ReadInt16(addr, 0);
                Int64 anti_data = Marshal.ReadInt64(addr, 2);
                info.anti_data_len = (ushort)anti_data_len;
                info.anti_data = new IntPtr(anti_data);
            }
            else if (TssSdk.Is32bit())
            {
                short anti_data_len = Marshal.ReadInt16(addr, 0);
                Int32 anti_data = Marshal.ReadInt32(addr, 2);
                info.anti_data_len = (ushort)anti_data_len;
                info.anti_data = new IntPtr(anti_data);
            }

            // 数据长度大于1024的直接丢弃
            if (info.anti_data_len > 1024)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("Tsssdk上报数据长度大于1024，丢弃");
                TssSdk.tss_del_report_data(addr);
                return;
            }

            if (SendDataToServer(info))
            {
                TssSdk.tss_del_report_data(addr);
            }
        }
    }

    private bool SendDataToServer(TssSdk.AntiDataInfo info)
    {
        byte[] data = new byte[info.anti_data_len];
        Marshal.Copy(info.anti_data, data, 0, info.anti_data_len);

        return DoSendDataToServer(data);
    }

    private bool DoSendDataToServer(byte[] data)
    {
        // 加入网络发送队列
        Capstones.UnityFramework.Network.ClientSocket.Send(Msg_C2S_Security_Data_Report, data);
        return true;
    }

    private void RecieveServerDataToClient(byte[] data)
    {
        OnReceiveServerDataToClient(data);
    }

    private void OnReceiveServerDataToClient(byte[] data)
    {
        TssSdk.TssSdkRcvAntiData(data, (ushort)data.Length);
    }

#endif
}
