using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Capstones.UnityFramework.Network
{
    /// <summary>
    /// 主要处理接收服务器数据轮询和心跳
    /// </summary>
    [LuaCallCSharp]
    public class SocketDispath : MonoBehaviour
    {
        private float lastHeartBeatTime = 0.0f;

        private float lastPongTime = 0.0f;
        
        /// <summary>
        /// 心跳包时间间隔 15秒
        /// </summary>
        private int HEART_BEAT_INTERVAL = 15;

        /// <summary>
        /// 双向验证网络状况 60秒
        /// </summary>
        private int PONG_INTERVAL = 60;

        /// <summary>
        /// 检查网络
        /// </summary>
        private float lastCheckTime = 0.0f;

        private int Check_Time = 3;

        void Start()
        {
            SocketManager.Instance.connetcSuccess = OnConnectSuccess;
            ClientSocket.AddCallBack(3, OnPong);
        }

        // Update is called once per frame
        void Update()
        {
            if (lastHeartBeatTime == 0)
            {
                lastHeartBeatTime = Time.unscaledTime;
            }

            if (lastPongTime == 0)
            {
                lastPongTime = Time.unscaledTime;
            }

            // 检测网络状态
            if ((Time.unscaledTime - lastPongTime) >= PONG_INTERVAL)
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("SocketDispatch Update step 1 NETWORK_ERR");
                SocketManager.Instance.OnChanageSocketState(SocketState.NETWORK_ERR);
                lastPongTime = 0;
            }

            if ((Time.unscaledTime - lastHeartBeatTime >= HEART_BEAT_INTERVAL) &&
                (ClientSocket.GetSocketConnectState()))
            {
                ClientSocket.Send(2, "{}");
                lastHeartBeatTime = Time.unscaledTime;
            }

#if UNITY_EDITOR
            if (lastCheckTime == 0)
            {
                lastCheckTime = Time.unscaledTime;
            }

            if (Time.unscaledTime - lastCheckTime >= Check_Time)
            {
                if (!ClientSocket.GetSocketConnectState())
                {
                    if(GLog.IsLogErrorEnabled) GLog.LogError("SocketDispatch step 2 socket DisConnect ！！！");
                }
                else
                {
                    //if (GLog.IsLogInfoEnabled) GLog.LogInfo("SocketDispatch step 2 ");
                }

                lastCheckTime = Time.unscaledTime;
            }
#endif

            ClientSocket.Update();
        }

        void OnApplicationQuit()
        {
            ClientSocket.DisConnect();
        }

        private void OnPong(string msg)
        {
            lastPongTime = Time.unscaledTime;
        }

        private void OnConnectSuccess()
        {
            lastPongTime = 0.0f;
        }
    }
}