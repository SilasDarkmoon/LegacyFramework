using System.IO;
using System;
using UnityEngine;

namespace Capstones.UnityFramework.Network 
{
    /// <summary>
    /// 常量数据
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// 消息：数据总长度(4byte) + 数据类型(4byte) + 数据(N byte)
        /// 消息体长度
        /// </summary>
        public static int HEAD_DATA_LEN = 4;

        public static int HEAD_TYPE_LEN = 4;

        /// <summary>
        /// 消息头长度8byte
        /// </summary>
        public static int HEAD_LEN
        {
            get { return HEAD_DATA_LEN + HEAD_TYPE_LEN; }
        }
    }

    /// <summary>
    /// 网络数据临时载体
    /// </summary>
    public struct TmpSocketData
    {
        public byte[] data;

        /// <summary>
        /// 消息号
        /// </summary>
        public uint protocallType;

        /// <summary>
        /// 头部长度+消息体长度
        /// </summary>
        public uint buffLength;

        /// <summary>
        /// 消息体长度
        /// </summary>
        public uint dataLength;
    }

    /// <summary>
    /// 发送消息时，如果网络断开，
    /// 先把发送数据保存起来
    /// </summary>
    public struct SendDataCaching
    {
        /// <summary>
        /// 消息号
        /// </summary>
        public uint protocallType;

        public byte[] data;
    }

    /// <summary>
    /// 网络数据缓存器
    /// </summary>
    public class DataBuffer
    {
        /// <summary>
        /// 自动大小数据缓存器
        /// </summary>
        private uint _minBuffLen;

        private byte[] _buff;

        private uint _curBuffPosition;

        /// <summary>
        /// 消息整个长度（包括消息头部）
        /// </summary>
        private uint _buffLength = 0;

        /// <summary>
        /// 消息体长度
        /// </summary>
        private uint _dataLength;

        private uint _protocalType;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_minBuffLen">最小缓冲区大小</param>
        public DataBuffer(uint _minBuffLen = 1024)
        {
            if (_minBuffLen <= 0)
            {
                this._minBuffLen = 1024;
            }
            else
            {
                this._minBuffLen = _minBuffLen;
            }
            _buff = new byte[this._minBuffLen];
        }

        /// <summary>
        /// 添加缓存数据
        /// </summary>
        /// <param name="_data"></param>
        /// <param name="_dataLen">包含消息头部</param>
        public void AddBuffer(byte[] _data, int _dataLen)
        {
            // 超过当前缓存
            if (_dataLen > _buff.Length - _curBuffPosition)
            {
                byte[] _tmpBuff = new byte[_curBuffPosition + _dataLen];
                Array.Copy(_buff, 0, _tmpBuff, 0, _curBuffPosition);
                Array.Copy(_data, 0, _tmpBuff, _curBuffPosition, _dataLen);
                _buff = _tmpBuff;
                _tmpBuff = null;
            }
            else
            {
                Array.Copy(_data, 0, _buff, _curBuffPosition, _dataLen);
            }

            // 修改当前数据标记
            _curBuffPosition += (uint)_dataLen;
        }

        /// <summary>
        /// 更新数据长度
        /// </summary>
        public void UpdateDataLength()
        {
            if (_dataLength == 0 && _curBuffPosition >= Constants.HEAD_LEN)
            {
                byte[] tmpDataLen = new byte[Constants.HEAD_DATA_LEN];
                Array.Copy(_buff, 0, tmpDataLen, 0, Constants.HEAD_DATA_LEN);
                _dataLength = BitConverter.ToUInt32(tmpDataLen, 0);

                byte[] tmpProtocalType = new byte[Constants.HEAD_TYPE_LEN];
                Array.Copy(_buff, Constants.HEAD_DATA_LEN, tmpProtocalType, 0, Constants.HEAD_TYPE_LEN);
                _protocalType = BitConverter.ToUInt32(tmpProtocalType, 0);

                _buffLength = _dataLength + (uint)Constants.HEAD_LEN;
            }
        }

        /// <summary>
        /// 获取一条可用数据，返回值标记是否有数据
        /// </summary>
        /// <param name="tmpSocketData"></param>
        /// <returns></returns>
        public bool GetData(out TmpSocketData tmpSocketData)
        {
            tmpSocketData = new TmpSocketData();
            if (_buffLength <= 0)
            {
                UpdateDataLength();
            }

            if (_buffLength > 0 && _curBuffPosition >= _buffLength)
            {
                tmpSocketData.buffLength = _buffLength;
                tmpSocketData.dataLength = _dataLength;
                tmpSocketData.protocallType = _protocalType;
                tmpSocketData.data = new byte[_dataLength];
                Array.Copy(_buff, Constants.HEAD_LEN, tmpSocketData.data, 0, _dataLength);
                _curBuffPosition -= _buffLength;

                // 粘包，把剩下的字节取出给_buff
                byte[] _tmpBuff = new byte[_curBuffPosition < _minBuffLen ? _minBuffLen : _curBuffPosition];
                Array.Copy(_buff, _buffLength, _tmpBuff, 0, _curBuffPosition);
                _buff = _tmpBuff;

                _buffLength = 0;
                _dataLength = 0;
                _protocalType = 0;
                return true;
            }
            return false;
        }
    }
}