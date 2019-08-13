﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;
using Unity.Collections.Concurrent;
using Capstones.UnityEngineEx;

using PlatDependant = Capstones.UnityEngineEx.PlatDependant;
using TaskProgress = Capstones.UnityEngineEx.TaskProgress;

namespace Capstones.Net
{
    public class TCPServer : TCPClient, IPersistentConnectionServer
    {
        public class ServerConnection : TCPClient, IServerConnection
        {
            protected TCPServer _Server;
            protected bool _Connected = false;

            internal ServerConnection(TCPServer server)
            {
                _Server = server;
            }

            protected override void PrepareSocket()
            {
                _Socket = _Server.Accept();
                _Connected = true;
            }
            public bool IsConnected
            {
                get { return _Connected; }
            }
        }

        public TCPServer(int port)
        {
            _Port = port;
        }

        protected int _Port;
        public int Port
        {
            get { return _Port; }
            set
            {
                if (value != _Port)
                {
                    if (IsConnectionAlive)
                    {
                        PlatDependant.LogError("Cannot change port when server started");
                    }
                    else
                    {
                        _Port = value;
                    }
                }
            }
        }

        protected Socket _Socket6;

        protected Socket _AcceptedSocket4;
        protected Socket _AcceptedSocket6;
        protected Semaphore _AcceptedSemaphore = new Semaphore(0, 2);
        protected void BeginAccept4()
        {
            _Socket.BeginAccept(ar =>
            {
                var socket = _Socket.EndAccept(ar);
                Interlocked.Exchange(ref _AcceptedSocket4, socket);
                _AcceptedSemaphore.Release();
            }, null);
        }
        protected void BeginAccept6()
        {
            _Socket6.BeginAccept(ar =>
            {
                var socket = _Socket6.EndAccept(ar);
                Interlocked.Exchange(ref _AcceptedSocket6, socket);
                _AcceptedSemaphore.Release();
            }, null);
        }
        public Socket Accept()
        {
            _AcceptedSemaphore.WaitOne();
            Socket rv = null;
            while (true)
            {
                rv = _AcceptedSocket4;
                if (Interlocked.CompareExchange(ref _AcceptedSocket4, null, rv) == rv)
                {
                    break;
                }
            }
            if (rv != null)
            {
                BeginAccept4();
                return rv;
            }
            while (true)
            {
                rv = _AcceptedSocket6;
                if (Interlocked.CompareExchange(ref _AcceptedSocket6, null, rv) == rv)
                {
                    break;
                }
            }
            if (rv != null)
            {
                BeginAccept6();
                return rv;
            }
            return null;
        }

        protected override void ConnectWork()
        {
            try
            {
                var address4 = IPAddress.Any;
                _Socket = new Socket(address4.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _Socket.Bind(new IPEndPoint(address4, _Port));
                _Socket.Listen(CONST.MAX_SERVER_PENDING_CONNECTIONS);

                var address6 = IPAddress.IPv6Any;
                _Socket6 = new Socket(address6.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _Socket6.Bind(new IPEndPoint(address6, _Port));
                _Socket6.Listen(CONST.MAX_SERVER_PENDING_CONNECTIONS);

                BeginAccept4();
                BeginAccept6();

                while (!_ConnectWorkCanceled)
                {
                    _HaveDataToSend.WaitOne();
                }

                //_Socket.Shutdown(SocketShutdown.Both);
                //_Socket6.Shutdown(SocketShutdown.Both);
            }
            catch (ThreadAbortException)
            {
                Thread.ResetAbort();
            }
            catch (Exception e)
            {
                PlatDependant.LogError(e);
            }
            finally
            {
                _ConnectWorkRunning = false;
                _ConnectWorkCanceled = false;
                if (_PreDispose != null)
                {
                    _PreDispose(this);
                }
                if (_Socket != null)
                {
                    _Socket.Close();
                    _Socket = null;
                }
                if (_Socket6 != null)
                {
                    _Socket6.Close();
                    _Socket6 = null;
                }
                // set handlers to null.
                _OnReceive = null;
                _OnSend = null;
                _OnSendComplete = null;
                _PreDispose = null;
            }
        }
        public override void Send(byte[] data, int cnt)
        {
            _HaveDataToSend.Set();
            StartConnect();
            if (_OnSendComplete != null)
            {
                _OnSendComplete(data, false);
            }
        }

        public void StartListening()
        {
            StartConnect();
        }
        public bool IsAlive { get { return IsConnectionAlive; } }
        public ServerConnection PrepareConnection()
        {
            return new ServerConnection(this);
        }
        IServerConnection IPersistentConnectionServer.PrepareConnection()
        {
            return PrepareConnection();
        }
    }

    public static partial class PersistentConnectionFactory
    {
        private static RegisteredCreator _Reg_TCP = new RegisteredCreator("tcp"
            , url => new TCPClient(url)
            , url =>
            {
                var uri = new Uri(url);
                var port = uri.Port;
                return new TCPServer(port);
            });
    }
}
