using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;

public enum Tp2GameStatus
{
    FRONTEND    = 1,
    BACKEND     = 2
}

public static class TssSdk2
{	
	//
	private const int TssSdkCmd_IsEmulator = 10;
	
	//
	public static void Tp2SdkInitEx(int gameId, string appKey)
	{
		tp2_sdk_init_ex(gameId, appKey);
	}
	public static void Tp2UserLogin(int account_type, int world_id, string open_id, string role_id)
	{
		tp2_setuserinfo(account_type, world_id, open_id, role_id);
	}
	public static void Tp2SetGamestatus(Tp2GameStatus status)
    {
        switch(status)
        {
		case Tp2GameStatus.FRONTEND:
            tp2_setoptions(0x1000);
            break;
		case Tp2GameStatus.BACKEND:
            tp2_setoptions(0x2000);;
            break;
        default:
            break;
        }
    }
	/**
	 * 判断当前运行环境是否为模拟器,TssSDK会在启动的时候就进行模拟器检测,当调用Tp2GetEmulatorName的时候,
	 * 如果检测完毕,将返回检测结果,否则,将返回上一次进程生命周期内的缓存结果,当不存在缓存结果时,判为非模拟器
	 * 
	 * 参数:
	 * 		wait -- 是否等待检测结果执行完毕
	 * 
	 * 返回值: 
	 * 	null -- 不是模拟器
	 *  非null -- 模拟器的名称
	 */
	public static string Tp2GetEmulatorName(bool wait)
	{
		//组装调用参数
		StringBuilder sb = new StringBuilder();
		sb.Append("files_dir=");
		sb.Append(Application.persistentDataPath);
		sb.Append("|wait=");
		if (wait) sb.Append("1");
		else sb.Append("0");
		
		//调用native实现
		IntPtr addr = tp2_sdk_ioctl(TssSdkCmd_IsEmulator, sb.ToString());
		if (addr == IntPtr.Zero)
		{
			return null;
		}
		//解析返回结果
		TssSdk.AntiDataInfo info = new TssSdk.AntiDataInfo();
		info.anti_data_len = (ushort)Marshal.ReadInt16(addr, 0);
		info.anti_data = ReadIntPtr(addr, 2);

		byte[] bytes = new byte[info.anti_data_len];
		Marshal.Copy(info.anti_data, bytes, 0, info.anti_data_len);
		
		//释放native的结果
		tp2_free_anti_data(addr);
		
		//
		string emulator_name = null;
		//
		string response_buf = System.Text.Encoding.ASCII.GetString(bytes);
		//
		string[] key_values = response_buf.Split('|');
		for (int i = 0; i < key_values.Length; i++)
		{
			string[] key_value_pair = key_values[i].Split('=');
			if (key_value_pair.Length >= 2)
			{
				string k = key_value_pair[0];
				string v = key_value_pair[1];
				
				if (string.Compare(k, "emulator_name") == 0)
				{
					emulator_name = v;
				}
			}
		}
		return emulator_name;
	}
	
	/**
	 * 读取指针
	 * 
	 * 说明:直接使用Marshal.ReadIntPtr是不可以的，测试的时候，在一些老版本的Unity(4.*)上，在编译成64ios工程后会编译不过
	 * 		所以需要先按机器位数(32/64)读整形，再转成IntPtr指针
	 */
	private static IntPtr ReadIntPtr(IntPtr addr, int off)
	{
		IntPtr ptr = IntPtr.Zero;
		if (TssSdk.Is64bit())
		{
			Int64 v64 = Marshal.ReadInt64(addr, off);
			ptr = new IntPtr(v64);
		}
		else
		{
			Int32 v32 = Marshal.ReadInt32(addr, off);
			ptr = new IntPtr(v32);
		}
		return ptr;
	}
	
#if TENCENT_RELEASE
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport("tersafe")]
#endif
	private static extern int tp2_sdk_init_ex(int gameId, string appKey);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else	
	[DllImport("tersafe")]
#endif
	private static extern int tp2_setuserinfo(int accountType, int worldId, string openId, string roleId);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else	
	[DllImport("tersafe")]
#endif
	private static extern int tp2_setoptions(int options);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport("tersafe")]
#endif
	private static extern IntPtr tp2_sdk_ioctl(int request, string param);
	
#if UNITY_IOS
	[DllImport("__Internal")]
#else
	[DllImport("tersafe")]
#endif
	private static extern int tp2_free_anti_data(IntPtr info);
#else
    private static int tp2_sdk_init_ex(int gameId, string appKey)
    {
        return 0;
    }

    private static int tp2_setuserinfo(int accountType, int worldId, string openId, string roleId)
    {
        return 0;
    }

    private static int tp2_setoptions(int options)
    {
        return 0;
    }

    private static IntPtr tp2_sdk_ioctl(int request, string param)
    {
        return IntPtr.Zero;
    }

    private static int tp2_free_anti_data(IntPtr info)
    {
        return 0;
    }

#endif
}


public static class TssSdk
{
	public enum ESERAILIZETAG{
		TAG_INT = 0x00,
		TAG_TYPE = 0x01,
		TAG_GAME_ID = 0x02,
		TAG_GAME_STATUS = 0x03,
		TAG_ENTRY_ID = 0x04,
		TAG_WORLD_ID = 0x05,
		TAG_STR = 0x40,
		TAG_APPID = 0x41,
		TAG_OPENID = 0x42,
		TAG_ROLEID = 0x43
	}

	public enum ESERIALIZETYPE{
		TYPE_INIT = 0x01,
		TYPE_SETUSERINFO = 0x02,
		TYPE_SETGAMESTATUS = 0x03
	}
	public enum EUINTYPE
	{
		UIN_TYPE_INT = 1, // integer format
		UIN_TYPE_STR = 2  // string format
	}
	
	public enum EAPPIDTYPE
	{
		APP_ID_TYPE_INT = 1, // integer format
		APP_ID_TYPE_STR = 2  // string format
	}
	
	public enum EENTRYID
	{
        ENTRY_ID_QQ         = 1,       // QQ
		ENTRY_ID_QZONE      = 1,	   // QQ
		ENTRY_ID_MM			= 2,	   // WeChat
        ENTRY_ID_WX         = 2,       // 微信
        ENTRT_ID_FACEBOOK   = 3,       // facebook
        ENTRY_ID_TWITTER    = 4,       // twitter
        ENTRY_ID_LINE       = 5,       // line
        ENTRY_ID_WHATSAPP   = 6,       // whatsapp
        ENTRY_ID_OTHERS     = 99,      // 其他平台
	}
	
	public enum EGAMESTATUS
	{
		GAME_STATUS_FRONTEND = 1,  // running in front-end
		GAME_STATUS_BACKEND = 2,   // running in back-end
        GAME_STATUS_START_PVP = 3, // start pvp
        GAME_STATUS_END_PVP = 4,   // end of pvp
        GAME_STATUS_UPDATE_FINISHED = 5
	}
	
	public enum AntiEncryptResult
	{
		ANTI_ENCRYPT_OK = 0,
		ANTI_NOT_NEED_ENCRYPT = 1,
	}
	
	public enum AntiDecryptResult
	{
		ANTI_DECRYPT_OK = 0,
		ANTI_DECRYPT_FAIL = 1,
	}
	
	// sdk anti-data info
	[StructLayout(LayoutKind.Sequential)]
	public class AntiDataInfo
	{
		//[FieldOffset(0)]
		public ushort anti_data_len;
		//[FieldOffset(2)]
		public IntPtr anti_data;
	};
	
	[StructLayout(LayoutKind.Explicit, Size = 20)]
	public class EncryptPkgInfo
	{
		[FieldOffset(0)]
		public int cmd_id_;				/* [in] game pkg cmd */
		[FieldOffset(4)]
		public IntPtr game_pkg_;		/* [in] game pkg */
		[FieldOffset(8)]
		public uint game_pkg_len_;		/* [in] the length of game data packets, maximum length less than 65,000 */
		[FieldOffset(12)]
		public IntPtr encrpty_data_;	/* [in/out] assembling encrypted game data package into anti data, memory allocated by the caller, 64k at the maximum */
		[FieldOffset(16)]
		public uint encrypt_data_len_;	/* [in/out] length of anti_data when input, actual length of anti_data when output */
	}
	
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	public class DecryptPkgInfo
	{
		[FieldOffset(0)]
		public IntPtr encrypt_data_;		/* [in] anti data received by game client */
		[FieldOffset(4)]
		public uint encrypt_data_len;       /* [in] length of anti data received by game client */
		[FieldOffset(8)]
		public IntPtr game_pkg_;            /* [out] buffer used to store the decrypted game package, space allocated by the caller */
		[FieldOffset(12)]
		public uint game_pkg_len_;          /* [out] input is size of game_pkg_, output is the actual length of decrypted game package */
	}
	
	public static Boolean Is64bit()
	{
		return IntPtr.Size == 8;
	}
	
	public static Boolean Is32bit()
	{
		return IntPtr.Size == 4;
	}
    class OutputUnityBuffer{
            private byte[] data;
            private uint offset;
            private uint count;
            public OutputUnityBuffer(uint length){
                    this.data = new byte[length];
                    this.offset = 0;
                    this.count = length;
            }

            public void write(byte b){
                    if (offset < count) {
                            this.data [offset] = b;
                            this.offset++;
                    }
            }

            public byte[] toByteArray(){
                    return data;
            }

            public uint getLength(){
                    return this.offset;
            }
    }
    class SerializeUnity{
            public static void putLength(OutputUnityBuffer data, uint length){
                    data.write ((byte)(length >> 24));
                    data.write ((byte)(length >> 16));
                    data.write ((byte)(length >> 8));
                    data.write ((byte)(length));
            }

            public static void putInteger(OutputUnityBuffer data, uint value){
                    data.write ((byte)(value >> 24));
                    data.write ((byte)(value >> 16));
                    data.write ((byte)(value >> 8));
                    data.write ((byte)(value));
            }

            public static void putByteArray(OutputUnityBuffer data, byte[] value){
                    int len = value.Length;
                    for (int i = 0; i < len; i++) {
                            data.write (value [i]);
                    }
                    data.write (0);
            }

            public static void setInitInfo(uint gameId){
                    OutputUnityBuffer data = new OutputUnityBuffer (1 + 4 + 1 + 1 + 4 + 4);
                    data.write((byte)ESERAILIZETAG.TAG_TYPE);
                    putLength (data, 1);
                    data.write ((byte)ESERIALIZETYPE.TYPE_INIT);

                    data.write ((byte)ESERAILIZETAG.TAG_GAME_ID);
                    putLength (data, 4);
                    putInteger (data, gameId);

                    tss_unity_str (data.toByteArray (), data.getLength());

            }

            public static void setGameStatus(EGAMESTATUS gameStatus){
                    OutputUnityBuffer data = new OutputUnityBuffer (1 + 4 + 1 + 1 + 4 + 4);
                    data.write((byte)ESERAILIZETAG.TAG_TYPE);
                    putLength (data, 1);
                    data.write ((byte)ESERIALIZETYPE.TYPE_SETGAMESTATUS);

                    data.write ((byte)ESERAILIZETAG.TAG_GAME_STATUS);
                    putLength (data, 4);
                    putInteger (data, (uint)gameStatus);

                    tss_unity_str (data.toByteArray (), data.getLength());
            }

            public static void setUserInfoEx(EENTRYID entryId,
                    string uin,
                    string appId,
                    uint worldId,
                    string roleId){

                    byte[] valOpenId = System.Text.Encoding.ASCII.GetBytes (uin);
                    byte[] valAppId = System.Text.Encoding.ASCII.GetBytes (appId);
                    byte[] valRoleId = System.Text.Encoding.ASCII.GetBytes (roleId);
                    uint length = 0;
                    OutputUnityBuffer data = new OutputUnityBuffer (6*1 + 6*4 + 1 + 4 + 4 + (uint)valOpenId.Length + 1 + (uint)valAppId.Length + 1 + (uint)valRoleId.Length + 1);
                    data.write((byte)ESERAILIZETAG.TAG_TYPE);
                    putLength (data, 1);
                    data.write ((byte)ESERIALIZETYPE.TYPE_SETUSERINFO);

                    data.write ((byte)ESERAILIZETAG.TAG_ENTRY_ID);
                    putLength (data, 4);
                    putInteger (data, (uint)entryId);

                    data.write ((byte)ESERAILIZETAG.TAG_OPENID);
                    length = (uint)valOpenId.Length + 1;
                    //if (GLog.IsLogInfoEnabled) GLog.LogInfo("openid length:" + length);
                    putLength (data, length);
                    putByteArray (data, valOpenId);

                    data.write ((byte)ESERAILIZETAG.TAG_APPID);
                    length = (uint)valAppId.Length + 1;
                    //if (GLog.IsLogInfoEnabled) GLog.LogInfo("appid length:" + length);
                    putLength (data, length);
                    putByteArray (data, valAppId);

                    data.write ((byte)ESERAILIZETAG.TAG_WORLD_ID);
                    putLength (data, 4);
                    putInteger (data, worldId);

                    data.write ((byte)ESERAILIZETAG.TAG_ROLEID);
                    length = (uint)valRoleId.Length + 1;
                    //if (GLog.IsLogInfoEnabled) GLog.LogInfo("roleid length:" + length);
                    putLength (data, length);
                    putByteArray (data, valRoleId);

                    //if (GLog.IsLogInfoEnabled) GLog.LogInfo("data length:" + data.getLength());
                    tss_unity_str (data.toByteArray (), data.getLength());
            }
    }

	/// <summary>
	/// Tsses the sdk init.
	/// </summary>
	/// <param name='gameId'>
	/// game id provided by sdk provider
	/// </param>
	public static void TssSdkInit(uint gameId)
	{
		SerializeUnity.setInitInfo (gameId);
		tss_enable_get_report_data();
		tss_log_str(TssSdkVersion.GetSdkVersion());
		tss_log_str(TssSdtVersion.GetSdtVersion());
	}
	/// <summary>
	/// Tsses the sdk set game status.
	/// </summary>
	/// <param name='gameStatus'>
	/// back-end or front-end
	/// </param>
	public static void TssSdkSetGameStatus(EGAMESTATUS gameStatus)
	{
		SerializeUnity.setGameStatus(gameStatus);
	}
	
	public static void TssSdkSetUserInfo(EENTRYID entryId,
	                                     string uin,
	                                     string appId)
	{
		TssSdkSetUserInfoEx(entryId, uin, appId, 0, "0");
	}
	
	public static void TssSdkSetUserInfoEx(EENTRYID entryId,
	                                       string uin,
	                                       string appId,
	                                       uint worldId,
	                                       string roleId
	                                       )
	{
				
		if (roleId == null) {
			roleId = "0";
		}
		SerializeUnity.setUserInfoEx (entryId, uin, appId, worldId, roleId);

	}
	
	public static byte[] TssSdkGetSdkReportData2()
	{
		IntPtr addr = tss_get_report_data2();
		if (addr == IntPtr.Zero)
		{
			return null;
		}
		ushort anti_data_len = 0;
		IntPtr anti_data = IntPtr.Zero;
		
		if (TssSdk.Is32bit())
		{
			//FileLog.Log("c#.32bit");
			
			anti_data_len = (ushort)Marshal.ReadInt16(addr, 0);
			
			Int32 anti_data_addr = Marshal.ReadInt32(addr, 2);
			if (anti_data_addr == 0)
			{
				return null;
			}
			anti_data = new IntPtr(anti_data_addr);
		}
		else if (TssSdk.Is64bit())
		{
			//FileLog.Log("c#.64bit");
			
			anti_data_len = (ushort)Marshal.ReadInt16(addr, 0);
			
			Int64 anti_data_addr = Marshal.ReadInt64(addr, 2);
			if (anti_data_addr == 0)
			{
				return null;
			}
			anti_data = new IntPtr(anti_data_addr);
		}
		//
		if (anti_data == IntPtr.Zero)
		{
			return null;
		}
		//
		byte[] data = new byte[anti_data_len];
		Marshal.Copy(anti_data, data, 0, anti_data_len);
		return data;
	}

	public static void TssSdkRcvAntiData(byte[] data, ushort length)
	{
		IntPtr pv = Marshal.AllocHGlobal (2 + IntPtr.Size);
		if (pv != IntPtr.Zero) 
		{
			Marshal.WriteInt16 (pv,0,(short)length);
			//Marshal.WriteIntPtr (pv,2,(IntPtr)data);
			IntPtr p = Marshal.AllocHGlobal(data.Length);
			if (p != IntPtr.Zero)
			{
				Marshal.Copy (data,0,p, data.Length);
				Marshal.WriteIntPtr (pv,2,p);
				tss_sdk_rcv_anti_data (pv);
				Marshal.FreeHGlobal(p);
			}
			
			Marshal.FreeHGlobal(pv);
		}
	}

	public static AntiEncryptResult TssSdkEncrypt(/*[in]*/int cmd_id, /*[in]*/byte[] src, /*[in]*/uint src_len,
	                                              /*[out]*/ref byte[] tar, /*[out]*/ref uint tar_len) 
	{
		AntiEncryptResult ret = AntiEncryptResult.ANTI_NOT_NEED_ENCRYPT;
		GCHandle src_handle = GCHandle.Alloc(src, GCHandleType.Pinned);
		GCHandle tar_handle = GCHandle.Alloc(tar, GCHandleType.Pinned);
		if (src_handle.IsAllocated && tar_handle.IsAllocated) 
		{
			EncryptPkgInfo info = new EncryptPkgInfo();
			info.cmd_id_ = cmd_id;
			info.game_pkg_ = src_handle.AddrOfPinnedObject();
			info.game_pkg_len_ = src_len;
			info.encrpty_data_ = tar_handle.AddrOfPinnedObject();
			info.encrypt_data_len_ = tar_len;
			ret = tss_sdk_encryptpacket(info);
			tar_len = info.encrypt_data_len_;
		}
		if (src_handle.IsAllocated) src_handle.Free();
		if (tar_handle.IsAllocated) tar_handle.Free();
		return ret;
	}

	public static AntiDecryptResult TssSdkDecrypt(/*[in]*/byte[] src, /*[in]*/uint src_len,
	                                              /*[out]*/ref byte[] tar, /*[out]*/ref uint tar_len) 
	{
		AntiDecryptResult ret = AntiDecryptResult.ANTI_DECRYPT_FAIL;
		GCHandle src_handle = GCHandle.Alloc(src, GCHandleType.Pinned);
		GCHandle tar_handle = GCHandle.Alloc(tar, GCHandleType.Pinned);
		if (src_handle.IsAllocated && tar_handle.IsAllocated) 
		{
			DecryptPkgInfo info = new DecryptPkgInfo();
			info.encrypt_data_ = src_handle.AddrOfPinnedObject();
			info.encrypt_data_len = src_len;
			info.game_pkg_ = tar_handle.AddrOfPinnedObject();
			info.game_pkg_len_ = tar_len;
			ret = tss_sdk_decryptpacket(info);
			tar_len = info.game_pkg_len_;
		}
		if (src_handle.IsAllocated) src_handle.Free();
		if (tar_handle.IsAllocated) tar_handle.Free();
		return ret;
	}

	const uint TPMaxSessionDataSize = 4096;

    public static string TssSdkGenSessionData()
    {
        byte[] data = new byte[TPMaxSessionDataSize];
		int dataLen= tss_sdk_gen_session_data(data, (uint)data.Length);
        if (dataLen < 0) {
            return String.Empty;
        }
        return System.Text.Encoding.Unicode.GetString(data, 0, dataLen);
    }

    public static int TssSdkSetToken(string token)
    {
        byte[] tokenBytes = System.Text.Encoding.Unicode.GetBytes(token);
		return tss_sdk_set_token (tokenBytes, (uint)tokenBytes.Length);
    }

    public static int TssSdkWaitVerify(uint timeout)
    {
		return tss_sdk_wait_verify(timeout);
    }

#if TENCENT_RELEASE
    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("tersafe")]
    #endif
    private static extern void tss_log_str(string sdk_version);
    
    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("tersafe")]
    #endif
    private static extern void tss_sdk_rcv_anti_data(IntPtr info);
    
    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("tersafe")]
    #endif
    private static extern AntiEncryptResult tss_sdk_encryptpacket(EncryptPkgInfo info);
    
    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("tersafe")]
    #endif
    private static extern AntiDecryptResult tss_sdk_decryptpacket(DecryptPkgInfo info);

    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("tersafe")]
    #endif
    private static extern Int32 tss_sdk_gen_session_data(byte[] buff, uint buff_size);   

    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("tersafe")]
    #endif
    private static extern Int32 tss_sdk_set_token(byte[] data, uint data_len);

    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("tersafe")]
    #endif
    private static extern Int32 tss_sdk_wait_verify(UInt32 timeout);
	
	#if UNITY_IOS
	[DllImport("__Internal")]
	#else
	[DllImport("tersafe")]
	#endif
	private static extern void tss_enable_get_report_data();
	
	#if UNITY_IOS
	[DllImport("__Internal")]
	#else
	[DllImport("tersafe")]
	#endif
	public static extern IntPtr tss_get_report_data();
	
	#if UNITY_IOS
	[DllImport("__Internal")]
	#else
	[DllImport("tersafe")]
	#endif
	public static extern IntPtr tss_get_report_data2();
	
	#if UNITY_IOS
	[DllImport("__Internal")]
	#else
	[DllImport("tersafe")]
	#endif
	public static extern void tss_del_report_data(IntPtr info);

	#if UNITY_IOS
	[DllImport("__Internal")]
	#else
	[DllImport("tersafe")]
	#endif
	public static extern void tss_unity_str(byte[] data, UInt32 len);

#if (UNITY_IOS || UNITY_IPHONE)
    [DllImport("__Internal")]
    public static extern int tss_unity_is_enable(byte[] data, int len);
#endif
#else
    private static void tss_log_str(string sdk_version)
    {
        
    }

    private static void tss_sdk_rcv_anti_data(IntPtr info)
    {
        
    }

    private static AntiEncryptResult tss_sdk_encryptpacket(EncryptPkgInfo info)
    {
        return AntiEncryptResult.ANTI_ENCRYPT_OK;
    }

    private static AntiDecryptResult tss_sdk_decryptpacket(DecryptPkgInfo info)
    {
        return AntiDecryptResult.ANTI_DECRYPT_OK;
    }

    private static Int32 tss_sdk_gen_session_data(byte[] buff, uint buff_size)
    {
        return 0;
    }

    private static Int32 tss_sdk_set_token(byte[] data, uint data_len)
    {
        return 0;
    }

    private static Int32 tss_sdk_wait_verify(UInt32 timeout)
    {
        return 0;
    }

    private static void tss_enable_get_report_data()
    {
        
    }

    public static IntPtr tss_get_report_data()
    {
        return IntPtr.Zero;
    }

    public static IntPtr tss_get_report_data2()
    {
        return IntPtr.Zero;
    }

    public static void tss_del_report_data(IntPtr info)
    {
        
    }

    public static void tss_unity_str(byte[] data, UInt32 len)
    {
        
    }

    public static int tss_unity_is_enable(byte[] data, int len)
    {
        return 0;
    }

#endif

}

class TssSdkVersion 
{
	private const string  cs_sdk_version = "C# SDK ver:3.3.7.461620";
	public static string GetSdkVersion()
	{
		return cs_sdk_version;	
	}
}

