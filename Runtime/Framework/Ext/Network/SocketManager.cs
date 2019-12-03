using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
#if !NET_4_6 && !NET_STANDARD_2_0
using Unity.Collections.Concurrent;
#else
using System.Collections.Concurrent;
#endif

namespace Capstones.UnityFramework.Network
{
    /// <summary>
    /// socket状态
    /// </summary>
    public enum SocketState
    {
        NONE = -1,
        SUCCESS = 0,
        TIME_OUT = 1000,
        CONNECT_FAIL = 1001,
        SEND_ERR = 1002,   // 发送时发现断线
        SOCKET_ERR = 1003, // 接收时，发现断线
        IP_ERR = 1004,     // ip为空
        IP_ILLEGAL = 1005, // ip非法
        NETWORK_ERR = 1006,  // 网络连接失败
    }

    /// <summary>
    /// socket管理类
    /// </summary>
    public class SocketManager
    {
        private static SocketManager _instance;
        public static SocketManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SocketManager();
                }
                return _instance;
            }
        }

        public bool IsConnceted
        {
            get
            {
                if (_clientSocket == null)
                {
                    return false;
                }
                else
                {
                    return _clientSocket.Connected;
                }
            }
        }

        private string _currIP;

        private int _currPort;

        private volatile Socket _clientSocket;

        private volatile Thread _receiveThread;

        private volatile Thread _connectThread;

        private volatile SocketState _currSocketState = SocketState.NONE;
        private volatile SocketState _currDispatchSocketState = SocketState.NONE;
        public SocketState CurrSocketState { get { return _currSocketState; } }

        private DataBuffer _databuffer = new DataBuffer();

        private byte[] _tmpReceiveBuff = new byte[4096];

        private volatile static bool _isInitThreadStart = false;
        /// <summary>
        /// 消息接收队列
        /// </summary>
        private ConcurrentQueue<TmpSocketData> _recvQueue = new ConcurrentQueue<TmpSocketData>();
        /// <summary>
        /// 发送消息时，如果网络断开，
        /// 先把发送数据保存起来
        /// </summary>
        private Queue<SendDataCaching> _sendDataCaching = new Queue<SendDataCaching>();
        /// <summary>
        /// 连接服务器超时 单位:毫秒
        /// </summary>
        private int TIME_OUT = 3000;
        /// <summary>
        /// lua的回调
        /// </summary>
        private Action<int> _luaCallback;

        private float _dispatchTime = 0;

        public Action connetcSuccess;

        private void _init()
        {
            if (_databuffer == null) _databuffer = new DataBuffer();
            if (_tmpReceiveBuff == null) _tmpReceiveBuff = new byte[4096];
            if (_recvQueue == null) _recvQueue = new ConcurrentQueue<TmpSocketData>();
            if (_sendDataCaching != null) _sendDataCaching = new Queue<SendDataCaching>();
        }

        private void _onConnetThread()
        {
            if (GLog.IsLogInfoEnabled) GLog.LogInfo("SocketManager _onConnetThread step 1");
            _connectThread = new Thread(_onConnet);
            _connectThread.IsBackground = true;
            _connectThread.Start();
        }
        /// <summary>
        /// 连接
        /// </summary>
        private void _onConnet()
        {
            try
            {
                _init();
                // 解析IP地址
                IPAddress[] ips = Dns.GetHostAddresses(_currIP);
                IPAddress ipAddress = ips[0];
                // 支持ipv4和ipv6
                _clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, _currPort);
                // 异步连接
                IAsyncResult result = _clientSocket.BeginConnect(ipEndpoint, new AsyncCallback(_onConnect_complete), _clientSocket);
                bool success = result.AsyncWaitHandle.WaitOne(TIME_OUT, true);
                // 超时
                if (!success) OnChanageSocketState(SocketState.TIME_OUT);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException("_onConnet Exception = " + e.Message);

                if (e is ThreadAbortException)
                {
                    // ThreadAbortException异常比较特殊，只要Abort一定会抛出此异常，属于正常现象，不需要记录错误。
                    // 但是，ThreadAbortException即便被catch了，也会再次抛出，然而抛出异常可能导致游戏闪退，所以必须阻止此异常再次抛出。
                    // 因此，这里调用Thread.ResetAbort阻止线程被中止，并且break跳出循环以便线程正常结束。
                    Thread.ResetAbort();
                    return;
                }

                if (e is FormatException || e is ArgumentException || e is SocketException || e is ArgumentOutOfRangeException || e is ArgumentNullException)
                {
                    OnChanageSocketState(SocketState.IP_ERR);
                    return;
                }
                OnChanageSocketState(SocketState.CONNECT_FAIL);
            }
        }
        /// <summary>
        /// 连接操作完成
        /// 可能成功，可能失败
        /// </summary>
        /// <param name="iar"></param>
        private void _onConnect_complete(IAsyncResult iar)
        {
            try
            {
                Socket client = (Socket)iar.AsyncState;
                client.EndConnect(iar);

                if (GLog.IsLogInfoEnabled) GLog.LogInfo("_onConnect_complete step 1");
                var clientSocket = _clientSocket;
                if (clientSocket != null && !clientSocket.Connected)
                {
                    if (GLog.IsLogInfoEnabled) GLog.LogInfo("_onConnect_complete step 2");
                    OnChanageSocketState(SocketState.CONNECT_FAIL);
                    _close();
                    return;
                }

                _isInitThreadStart = true;
                _receiveThread = new Thread(_onReceiveSocket);
                _receiveThread.IsBackground = true;
                _receiveThread.Start();
                if (connetcSuccess != null)
                {
                    connetcSuccess();
                }
                OnChanageSocketState(SocketState.SUCCESS);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException("_onConnect_complete = " + e.Message);
                if (e is ThreadAbortException)
                {
                    // ThreadAbortException异常比较特殊，只要Abort一定会抛出此异常，属于正常现象，不需要记录错误。
                    // 但是，ThreadAbortException即便被catch了，也会再次抛出，然而抛出异常可能导致游戏闪退，所以必须阻止此异常再次抛出。
                    // 因此，这里调用Thread.ResetAbort阻止线程被中止，并且break跳出循环以便线程正常结束。
                    Thread.ResetAbort();
                    return;
                }
                _close();
            }
        }
        /// <summary>
        /// 接受网络数据
        /// 在接收线程中
        /// </summary>
        private void _onReceiveSocket()
        {
            while (true)
            {
                try
                {
                    var receiveThread = _receiveThread;
                    if (!_isInitThreadStart || _clientSocket == null || receiveThread == null || !receiveThread.IsAlive) return;
                    if (_clientSocket != null && !_clientSocket.Connected)
                    {
                        OnChanageSocketState(SocketState.SOCKET_ERR);
                        return;
                    }
                    ///Available 属性用于确定在网络缓冲区中排队等待读取的数据的量。 如果数据可用，可调用 Read 获取数据。 如果无数据可用，则 Available 属性返回 0
                    if (_clientSocket.Available <= 0)
                    {
                        Thread.Sleep(0);
                        continue;
                    }
                    int receiveLength = _clientSocket.Receive(_tmpReceiveBuff);
                    if (receiveLength <= 0)
                    {
                        Thread.Sleep(0);
                        continue;
                    }
                    // 将收到的数据添加到缓存器中
                    _databuffer.AddBuffer(_tmpReceiveBuff, receiveLength);
                    TmpSocketData _socketData;
                    // 取出一条完整数据
                    while (_databuffer.GetData(out _socketData))
                    {
                        var recvQueue = _recvQueue;
                        if (recvQueue != null) recvQueue.Enqueue(_socketData);
                    }
                    Thread.Sleep(0);
                }
                catch (Exception e)
                {
                    if (GLog.IsLogErrorEnabled) GLog.LogException("_onReceiveSocket Exception = " + e.Message);
                    if (e is ThreadAbortException)
                    {
                        // ThreadAbortException异常比较特殊，只要Abort一定会抛出此异常，属于正常现象，不需要记录错误。
                        // 但是，ThreadAbortException即便被catch了，也会再次抛出，然而抛出异常可能导致游戏闪退，所以必须阻止此异常再次抛出。
                        // 因此，这里调用Thread.ResetAbort阻止线程被中止，并且break跳出循环以便线程正常结束。
                        Thread.ResetAbort();
                        return;
                    }
                    _close();
                    return;
                }
            }
        }
        /// <summary>
        /// 发送消息基本方法
        /// </summary>
        /// <param name="_protocalType"></param>
        /// <param name="_data">不包含头信息</param>
        public void SendMsgBase(uint _protocalType, byte[] _data)
        {
            var clientSocket = _clientSocket;
            if (clientSocket == null || !clientSocket.Connected)
            {
                if (_sendDataCaching != null)
                {
                    SendDataCaching sendData = new SendDataCaching();
                    sendData.data = _data;
                    sendData.protocallType = _protocalType;
                    _sendDataCaching.Enqueue(sendData);
                }
                OnChanageSocketState(SocketState.SEND_ERR);
                return;
            }

            byte[] _msgdata = DataToBytes(_protocalType, _data);
            clientSocket.BeginSend(_msgdata, 0, _msgdata.Length, SocketFlags.None, new AsyncCallback(_onSendMsg), clientSocket);
        }

        public void OnChanageSocketState(SocketState value)
        {
            _currSocketState = value;
        }

        /// <summary>
        /// 断开
        /// </summary>
        private void _close()
        {
            _isInitThreadStart = false;
            try
            {
                if (_clientSocket != null)
                {
                    _clientSocket.Shutdown(SocketShutdown.Both);
                    _clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException("socket 关闭 _clientSocket :" + e.Message);
            }
            _clientSocket = null;
            CloseThread(_receiveThread);
            _receiveThread = null;
            CloseThread(_connectThread);
            _connectThread = null;
            _currDispatchSocketState = SocketState.NONE;
            if (GLog.IsLogWarningEnabled) GLog.LogWarning("socket 关闭完成");
        }
        /// <summary>
        /// 关闭线程
        /// </summary>
        private void CloseThread(Thread thread)
        {
            try
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                    thread.Join();
                    thread = null;
                }
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException("CloseThread ：" + e.Message);
            }
        }
        /// <summary>
        /// 重连
        /// </summary>
        private void _ReConnect()
        {
            if (GLog.IsLogErrorEnabled) GLog.LogWarning("socket 断开重连。。。");
            _close();
            _onConnetThread();
        }
        /// <summary>
        /// 发送消息结果回掉，可判断当前网络状态
        /// </summary>
        /// <param name="asyncSend"></param>
        private void _onSendMsg(IAsyncResult asyncSend)
        {
            try
            {
                Socket client = (Socket)asyncSend.AsyncState;
                client.EndSend(asyncSend);
            }
            catch (Exception e)
            {
                if (GLog.IsLogErrorEnabled) GLog.LogException("send msg exception:" + e.Message);
            }
        }
        /// <summary>
        /// 数据转网络结构
        /// TODO 这个转换没什么用，可以去掉
        /// </summary>
        /// <param name="_protocalType"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        private TmpSocketData BytesToSocketData(uint _protocalType, byte[] _data)
        {
            TmpSocketData tmpSocketData = new TmpSocketData();
            tmpSocketData.buffLength = (uint)(Constants.HEAD_LEN + _data.Length);
            tmpSocketData.dataLength = (uint)_data.Length;
            tmpSocketData.protocallType = _protocalType;
            tmpSocketData.data = _data;
            return tmpSocketData;
        }

        /// <summary>
        /// 网络结构转数据
        /// </summary>
        /// <param name="tmpSocketData"></param>
        /// <returns></returns>
        private byte[] SocketDataToBytes(TmpSocketData tmpSocketData)
        {
            byte[] _tmpBuff = new byte[tmpSocketData.buffLength];
            byte[] _dataLength = BitConverter.GetBytes(tmpSocketData.buffLength - Constants.HEAD_LEN);
            byte[] _protocallTypeLength = BitConverter.GetBytes((uint)tmpSocketData.protocallType);

            // 消息体长度
            Array.Copy(_dataLength, 0, _tmpBuff, 0, Constants.HEAD_DATA_LEN);
            // 协议类型
            Array.Copy(_protocallTypeLength, 0, _tmpBuff, Constants.HEAD_DATA_LEN, Constants.HEAD_TYPE_LEN);
            // 协议数据
            Array.Copy(tmpSocketData.data, 0, _tmpBuff, Constants.HEAD_LEN, tmpSocketData.dataLength);

            return _tmpBuff;
        }

        /// <summary>
        /// 合并协议，数据
        /// </summary>
        /// <param name="_protocalType"></param>
        /// <param name="_data"></param>
        /// <returns></returns>
        private byte[] DataToBytes(uint _protocalType, byte[] _data)
        {
            uint _buffLength = (uint)(Constants.HEAD_LEN + _data.Length);
            uint _dataLength = (uint)_data.Length;

            byte[] _tmpBuff = new byte[_buffLength];
            byte[] _dataLengthByte = BitConverter.GetBytes(_dataLength);
            byte[] _protocallTypeLengthByte = BitConverter.GetBytes((uint)_protocalType);

            // 消息体长度
            Array.Copy(_dataLengthByte, 0, _tmpBuff, 0, Constants.HEAD_DATA_LEN);
            // 协议类型
            Array.Copy(_protocallTypeLengthByte, 0, _tmpBuff, Constants.HEAD_DATA_LEN, Constants.HEAD_TYPE_LEN);
            // 协议数据
            Array.Copy(_data, 0, _tmpBuff, Constants.HEAD_LEN, _dataLength);

            return _tmpBuff;
        }

        /// <summary>
        /// 判断是否合法ip
        /// </summary>
        /// <param name="IP"></param>
        /// <returns></returns>
        private bool _iPCheck(string IP)
        {
            return Regex.IsMatch(IP, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }
        /// <summary>
        /// 连接服务器
        /// </summary>
        /// <param name="_currIP"></param>
        /// <param name="_currPort"></param>
        public void Connect(string currIP, int currPort, Action<int> callback = null)
        {
            if ((currIP == null || "".Equals(currIP)) && callback != null)
            {
                callback((int)SocketState.IP_ERR);
                return;
            }
            if (IsConnceted) return;
            _currIP = currIP;
            _currPort = currPort;
            _luaCallback = callback;
            _onConnetThread();
        }

        /// <summary>
        /// 以二进制方式发送
        /// </summary>
        /// <param name="_protocalType"></param>
        /// <param name="_byteStreamBuff"></param>
        public void SendMsg(uint _protocalType, ByteStreamBuff _byteStreamBuff)
        {
            SendMsgBase(_protocalType, _byteStreamBuff.ToArray());
        }

        /// <summary>
        /// 直接发字符串
        /// </summary>
        /// <param name="_protocalType"></param>
        /// <param name="data"></param>
        public void SendMsgStr(uint _protocalType, string data)
        {
            SendMsgBase(_protocalType, Encoding.UTF8.GetBytes(data));
        }

        public void Close()
        {
            _close();
            _currSocketState = SocketState.NONE;
            _luaCallback = null;
            _databuffer = null;
            _tmpReceiveBuff = null;
            _dispatchTime = 0;
            _recvQueue = null;
            if (_sendDataCaching != null) _sendDataCaching.Clear();
            _sendDataCaching = null;

        }
        /// <summary>
        /// 这个方法要在update中轮询
        /// </summary>
        /// <returns></returns>
        public void Dispatch()
        {
            if (_currSocketState == SocketState.NONE) return;
            DispatchCheckSocketState();
            _dispatchTime += Time.unscaledDeltaTime;
            if (_dispatchTime < 0.5f) return;
            _dispatchTime = 0;
            DispatchRecvQueue(_recvQueue);
            DispatchSendMsgBase();
        }

        private void DispatchSendMsgBase()
        {
            if (_clientSocket == null || !_clientSocket.Connected || _sendDataCaching == null) return;
            try
            {
                SendDataCaching data;
                while (_sendDataCaching.Count > 0)
                {
                    data = _sendDataCaching.Dequeue();
                    SendMsgBase(data.protocallType, data.data);
                }
            }
            catch (Exception e)
            {
                GLog.LogException("DispatchSendMsgBase ==> " + e.Message);
            }
        }

        private void DispatchCheckSocketState()
        {
            if (_currDispatchSocketState == _currSocketState || _luaCallback == null) return;
            try
            {
                if (GLog.IsLogWarningEnabled) GLog.LogWarning("socket DispatchCheckSocketState 派发事件 currDispatchSocketState :[" + _currDispatchSocketState + "]  _currSocketState :" + _currSocketState);
                _currDispatchSocketState = _currSocketState;
                _luaCallback((int)_currSocketState);
            }
            catch (Exception e)
            {
                GLog.LogException("DispatchCheckSocketState ==> " + e.Message);
            }
        }

        private void DispatchRecvQueue(ConcurrentQueue<TmpSocketData> recvQueue)
        {
            if (recvQueue == null) return;
            bool isShowLog = false;
#if UNITY_EDITOR
            isShowLog = true;
#endif
            try
            {
                while (recvQueue.Count > 0)
                {
                    TmpSocketData data;
                    if (!recvQueue.TryDequeue(out data)) continue;
                    if (data.protocallType < 10000)
                    {
                        Action<string> handler = NetReceiver.GetHandler(data.protocallType);
                        string content = Encoding.UTF8.GetString(data.data);
                        if (handler != null)
                        {
                            if (isShowLog && GLog.IsLogInfoEnabled) GLog.LogInfo("protocallType = " + data.protocallType + ", content = " + content);
                            handler(content);
                            continue;
                        }
                        if (isShowLog && GLog.IsLogErrorEnabled) GLog.LogError("The (" + data.protocallType + ") do not have handler!");
                        continue;
                    }
                    else
                    {
                        Action<byte[]> handler = NetReceiver.GetHandlerByte(data.protocallType);
                        if (handler != null)
                        {
                            if (isShowLog && GLog.IsLogInfoEnabled) GLog.LogInfo("protocallType = " + data.protocallType);
                            handler(data.data);
                            continue;
                        }
                        if (isShowLog && GLog.IsLogErrorEnabled) GLog.LogError("The (" + data.protocallType + ") do not have handler!");
                    }
                }
            }
            catch (Exception e)
            {
                GLog.LogException("DispatchRecvQueue ==> " + e.Message);
            }
        }
        /// <summary>
        /// 重新连接
        /// </summary>
        public void ReConnect()
        {
            _ReConnect();
        }
    }
}