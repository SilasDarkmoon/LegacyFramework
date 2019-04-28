using System;
using System.Collections.Generic;
using XLua;

namespace Capstones.UnityFramework.Network
{
    /// <summary>
    /// lua使用的socket
    /// 对外使用
    /// </summary>
    [LuaCallCSharp]
    public class ClientSocket
    {
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callBack">回调函数，成功/失败都回调</param>
        public static void Connect(string ip, int port, Action<int> callBack)
        {
            if ("".Equals(ip) || ip == null) 
            {
                if (GLog.IsLogInfoEnabled) GLog.LogInfo("ClientSocket Connect step 1");
                return;
            }
            NetReceiver.Init();
            SocketManager.Instance.Connect(ip, port, callBack);
        }

        /// <summary>
        /// 重新连接socket服务器
        /// </summary>
        public static void ReConnect() 
        {
            SocketManager.Instance.ReConnect();
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public static void DisConnect()
        {
            SocketManager.Instance.Close();
            NetReceiver.Destroy();
        }

        /// <summary>
        /// 获取收到的消息
        /// </summary>
        public static void Update()
        {
            SocketManager.Instance.Dispatch();
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="protocallType"></param>
        /// <param name="data"></param>
        public static void Send(uint protocallType, string data)
        {
            SocketManager.Instance.SendMsgStr(protocallType, data);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="protocallType"></param>
        /// <param name="data">字节数组</param>
        public static void Send(uint protocallType, byte[] data)
        {
            SocketManager.Instance.SendMsgBase(protocallType, data);
        }

        /// <summary>
        /// 注册服务器推送回调
        /// </summary>
        /// <param name="protocallType"></param>
        /// <param name="handler"></param>
        public static void AddCallBack(uint protocallType, Action<string> handler)
        {
            NetReceiver.AddHandler(protocallType, handler);
        }

        /// <summary>
        /// 注册服务器推送回调
        /// byte[]
        /// </summary>
        /// <param name="protocallType"></param>
        /// <param name="handler"></param>
        public static void AddCallBackByte(uint protocallType, Action<byte[]> handler)
        {
            NetReceiver.AddHandlerByte(protocallType, handler);
        }

        /// <summary>
        /// 删除对应的回调
        /// </summary>
        /// <param name="protocallType"></param>
        public static void RemoveCallBack(uint protocallType)
        {
            NetReceiver.RemoveHandler(protocallType);
        }

        public static bool GetSocketConnectState() 
        {
            return SocketManager.Instance.IsConnceted;
        }
    }
}