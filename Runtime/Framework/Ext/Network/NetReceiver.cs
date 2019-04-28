using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace Capstones.UnityFramework.Network 
{
    /// <summary>
    /// 注册消息回调
    /// </summary>
    public class NetReceiver
    {
        private static Dictionary<uint, Action<string>> rpcReqHandlerDict;

        private static Dictionary<uint, Action<byte[]>> rpcReqHandlerDictByte;

        /// <summary>
        /// 需要使用socket时调用
        /// </summary>
        public static void Init()
        {
            if (rpcReqHandlerDict == null) 
            {
                rpcReqHandlerDict = new Dictionary<uint, Action<string>>();
            }

            if (rpcReqHandlerDictByte == null)
            {
                rpcReqHandlerDictByte = new Dictionary<uint, Action<byte[]>>();
            }
        }

        /// <summary>
        /// 添加消息监听
        /// </summary>
        /// <param name="protocallType"></param>
        /// <param name="handler"></param>
        public static void AddHandler(uint protocallType, Action<string> handler)
        {
            if (rpcReqHandlerDict == null)
            {
                Init();
            }
            rpcReqHandlerDict.Add(protocallType, handler);
        }

        public static void AddHandlerByte(uint protocallType, Action<byte[]> handler)
        {
            if (rpcReqHandlerDictByte == null)
            {
                Init();
            }

            rpcReqHandlerDictByte.Add(protocallType, handler);
        }

        /// <summary>
        /// 删除监听
        /// </summary>
        /// <param name="protocallType"></param>
        public static void RemoveHandler(uint protocallType)
        {
            if (protocallType < 10000)
            {
                if (rpcReqHandlerDict != null)
                {
                    rpcReqHandlerDict.Remove(protocallType);
                }
            }
            else
            {
                if (rpcReqHandlerDictByte != null)
                {
                    rpcReqHandlerDictByte.Remove(protocallType);
                }
            }
        }

        /// <summary>
        /// 获得消息回调
        /// </summary>
        /// <param name="protocallType"></param>
        public static Action<String> GetHandler(uint protocallType)
        {
            if (rpcReqHandlerDict == null) 
            {
                return null;
            }

            Action<String> handler;
            rpcReqHandlerDict.TryGetValue(protocallType, out handler);

            return handler;
        }

        /// <summary>
        /// 获得消息回调
        /// byte[]
        /// </summary>
        /// <param name="protocallType"></param>
        /// <returns></returns>
        public static Action<byte[]> GetHandlerByte(uint protocallType)
        {
            if (rpcReqHandlerDictByte == null)
            {
                return null;
            }

            Action<byte[]> handler;
            rpcReqHandlerDictByte.TryGetValue(protocallType, out handler);

            return handler;
        }

        public static void Destroy()
        {
            if (rpcReqHandlerDict != null)
            {
                rpcReqHandlerDict.Clear();
                rpcReqHandlerDict = null;
            }

            if (rpcReqHandlerDictByte != null)
            {
                rpcReqHandlerDictByte.Clear();
                rpcReqHandlerDictByte = null;
            }
        }
    }
}