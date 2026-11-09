using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AutoFrame.SG
{
    #region Enum
    /// <summary>
    /// 错误码
    /// </summary>
    internal enum RC
    {
        /// <summary>
        /// 执行OK
        /// </summary>
        RC_OK = 0x0000,                 // The operation is completed successfully.

        // Communication error from controller notification
        /// <summary>
        /// 指令错误
        /// </summary>
        RC_NAK_COMMAND = 0x1001,        // Command error
        /// <summary>
        /// 指令长度错误
        /// </summary>
        RC_NAK_COMMAND_LENGTH,          // Command length error
        /// <summary>
        /// 指令超时
        /// </summary>
        RC_NAK_TIMEOUT,                 // Timeout
        /// <summary>
        /// 参数计数错误
        /// </summary>
        RC_NAK_CHECKSUM,                // Check sum error
        /// <summary>
        /// 状态错误
        /// </summary>
        RC_NAK_INVALID_STATE,           // Status error
        /// <summary>
        /// 其他错误
        /// </summary>
        RC_NAK_OTHER,                   // Other error
        /// <summary>
        /// 参数错误
        /// </summary>
        RC_NAK_PARAMETER,               // Parameter error
        /// <summary>
        /// 计算中重复使用一个OUT值的次数超出限制
        /// </summary>
        RC_NAK_OUT_STAGE,               // OUT calculation count limitation error
        /// <summary>
        /// 
        /// </summary>
        RC_NAK_OUT_HEAD_NUM,            // No. of used head/OUT over error
        RC_NAK_PARAM_RANGE_DATA_STORED, // OUT No which data reserved over active out count
        RC_NAK_OUT_INVALID_CALC,        // OUT which cannot be used for calculation was specified for calculation.
        RC_NAK_OUT_VOID,                // OUT which specified for calculation is not found.
        RC_NAK_INVALID_CYCLE,           // Unavailable sampling cycle
        RC_NAK_CTRL_ERROR,              // Main unit error
        RC_NAK_SRAM_ERROR,              // Setting value error

        // Communication DLL error notification
        RC_ERR_OPEN_DEVICE = 0x2000,// Opening the device failed.
        RC_ERR_NO_DEVICE,           // The device is not open.
        RC_ERR_SEND,                // Command sending error
        RC_ERR_RECEIVE,             // Response receiving error
        RC_ERR_TIMEOUT,             // Timeout
        RC_ERR_NODATA,              // No data
        RC_ERR_NOMEMORY,            // No free memory
        RC_ERR_DISCONNECT,          // Cable disconnection suspected
        RC_ERR_UNKNOWN,             // Undefined error
        RC_ERR_DEVID_OVER,          // over dev_id scale
        RC_ERR_NET_NO_CONN,         // no connected at the deviceID 
        RC_ERR_IP_EXISTED,
        RC_ERR_SELECT,              // select of /socket error
        RC_ERR_NULL_PARAMETER,      // NULL point exception from parameter
        RC_ERR_DIY_FUNC,            // error due to dir function
        RC_ERR_OVER_PARAMENTER,		// paramenter over the limition
    }
    /// <summary>
    /// 设备型号/设备类别
    /// </summary>
    internal enum SGIF_DEVICE_TYPE
    {
        SG3035 = 0,
        SG3030 = 1,
        SG3085 = 2,
        SG3080 = 3,
        SG5025 = 4,
        SG5020 = 5,
        SG5055 = 6,
        SG5050 = 7,
        SG5085 = 8,
        SG5080 = 9,
        SG5155 = 10,
        SG5150 = 11,
        SC04025 = 12,
        SC03560 = 13,
        SGI500 = 14,
        SGI505 = 15,
        SGI400 = 16,
        SGI405 = 17,
        SGI150 = 18,
        SGI155 = 19,
        SGI080 = 20,
        SGI085 = 21,
        SGI050 = 22,
        SGI055 = 23,
        SGI030 = 24,
        SGI035 = 25,
        SGI020 = 26,
        SGI025 = 27,
        SG3155 = 28,
        SG3150 = 29,
        SC01045 = 30,
        SC10015 = 31,
        SC20011 = 32,
        SC01236 = 33,
        SCI04025 = 34,
        SCI03560 = 35,
        SCI01045 = 36,
        SCI10015 = 37,
        SCI20011 = 38,
        SCI01236 = 39,
    }
    /// <summary>
    /// 数据类型
    /// </summary>
    internal enum SGIF_FLOATRESULT
    {
        /// <summary>
        /// 有效数据
        /// </summary>
        SGIF_FLOATRESULT_VALID,         // valid data
        /// <summary>
        /// 超上限的值
        /// </summary>
        SGIF_FLOATRESULT_RANGEOVER_P,   // over range at positive (+) side
        /// <summary>
        /// 超下限的值
        /// </summary>
        SGIF_FLOATRESULT_RANGEOVER_N,   // over range at negative (-) side
        /// <summary>
        /// 等待比较器结果
        /// </summary>
        SGIF_FLOATRESULT_WAITING,       // Wait for comparator result
        /// <summary>
        /// 错误值
        /// </summary>
        SGIF_FLOATRESULT_ALARM,         // alarm
        /// <summary>
        /// 无效数据
        /// </summary>
        SGIF_FLOATRESULT_INVALID,		// Invalid (error, etc.)
    }
    /// <summary>
    /// 光亮模式
    /// </summary>
    internal enum SGIF_ABLEMODE
    {
        /// <summary>
        /// 自动
        /// </summary>
        SGIF_ABLEMODE_AUTO,
        /// <summary>
        /// 手动
        /// </summary>
        SGIF_ABLEMODE_MANUAL
    }
    /// <summary>
    /// 保持模式
    /// </summary>
    internal enum SGIF_CALCMODE
    {
        /// <summary>
        /// 标准
        /// </summary>
        SGIF_CALCMODE_NORMAL,
        /// <summary>
        /// 峰值保持
        /// </summary>
        SGIF_CALCMODE_PEAKHOLD,
        /// <summary>
        /// 谷值保持
        /// </summary>
        SGIF_CALCMODE_BOTTOMHOLD,
        /// <summary>
        /// 峰值至峰值保持
        /// </summary>
        SGIF_CALCMODE_PEAKTOPEAKHOLD,
        /// <summary>
        /// 采样保持
        /// </summary>
        SGIF_CALCMODE_SAMPLEHOLD,
    }
    /// <summary>
    /// 测量方法
    /// </summary>
    internal enum SGIF_MEASUREMODE
    {
        /// <summary>
        /// 标准
        /// </summary>
        SGIF_MEASUREMODE_NORMAL,
        /// <summary>
        /// 半透明对象表面
        /// </summary>
        SGIF_MEASUREMODE_HALF_T,
        /// <summary>
        /// 透明对象
        /// </summary>
        SGIF_MEASUREMODE_TRAN_1,
        /// <summary>
        /// 透明对象2
        /// </summary>
        SGIF_MEASUREMODE_TRAN_2,
        /// <summary>
        /// 反光树脂
        /// </summary>
        SGIF_MEASUREMODE_MRS,
        /// <summary>
        /// 运算
        /// </summary>
        SGIF_MEASUREMODE_OPAQUE = SGIF_MEASUREMODE_MRS
    }
    /// <summary>
    /// 基点
    /// </summary>
    internal enum SGIF_BASICPOINT
    {
        /// <summary>
        /// NEAR
        /// </summary>
        SGIF_BASICPOINT_NEAR,
        /// <summary>
        /// FAR
        /// </summary>
        SGIF_BASICPOINT_FAR,			// FAR
    }
    /// <summary>
    /// 反射类型
    /// </summary>
    internal enum SGIF_REFLECTIONMODE
    {
        /// <summary>
        /// 漫反射
        /// </summary>
        SGIF_REFLECTIONMODE_DIFFUSION,
        /// <summary>
        /// 镜面反射
        /// </summary>
        SGIF_REFLECTIONMODE_MIRROR,     // specular reflection
    }

    /// <summary>
    /// 中位数
    /// </summary>
    internal enum SGIF_MEDIAN
    {
        /// <summary>
        /// 关闭
        /// </summary>
        SGIF_MEDIAN_OFF,
        /// <summary>
        /// 7
        /// </summary>
        SGIF_MEDIAN_7,
        /// <summary>
        /// 15
        /// </summary>
        SGIF_MEDIAN_15,
        /// <summary>
        /// 31
        /// </summary>
        SGIF_MEDIAN_31
    }

    /// <summary>
    /// LASER CTRL组
    /// </summary>
    internal enum SGIF_LASER_CTRL_GROUP
    {
        /// <summary>
        /// LASER CTRL 1
        /// </summary>
        SGIF_LASER_CTRL_GROUP_1,
        /// <summary>
        /// LASER CTRL 2
        /// </summary>
        SGIF_LASER_CTRL_GROUP_2,
    }

    /// <summary>
    /// ？区域？
    /// </summary>
    internal enum SGIF_RANGE
    {
        /// <summary>
        /// 中心
        /// </summary>
        SGIF_RANGE_CENTER,              // CENTER
        /// <summary>
        /// 远端
        /// </summary>
        SGIF_RANGE_FAR,                 // FAR
    }

    // Set Mutual Interference Prevention Group
    internal enum SGIF_MUTUAL_INTERFERENCE_PREVENTION_GROUP
    {
        SGIF_MUTUAL_INTERFERENCE_PREVENTION_GROUP_A,    // Group A
        SGIF_MUTUAL_INTERFERENCE_PREVENTION_GROUP_B,    // Group B
        SGIF_MUTUAL_INTERFERENCE_PREVENTION_GROUP_C,    // Group C
    }

    // Set calculation method.
    internal enum SGIF_CALCMETHOD
    {
        SGIF_CALCMETHOD_HEADA,                  // head A
        SGIF_CALCMETHOD_HEADB,                  // head B
        SGIF_CALCMETHOD_HEAD_HEADA_PLUS_HEADB,  // head A+head B
        SGIF_CALCMETHOD_HEAD_HEADA_MINUS_HEADB, // head A-head B
        SGIF_CALCMETHOD_HEAD_HEADA_TRANSPARENT, // head A transparent object
        SGIF_CALCMETHOD_HEAD_HEADB_TRANSPARENT, // head B transparent object

        SGIF_CALCMETHOD_HEAD = 0,   // head
        SGIF_CALCMETHOD_OUT,        // OUT
        SGIF_CALCMETHOD_ADD,        // ADD
        SGIF_CALCMETHOD_SUB,        // SUB
        SGIF_CALCMETHOD_AVE,        // AVE
        SGIF_CALCMETHOD_MAX,        // MAX
        SGIF_CALCMETHOD_MIN,        // MIN
        SGIF_CALCMETHOD_PP,         // P-P
    }

    // Set Target Surface
    internal enum SGIF_CALCTARGET
    {
        SGIF_CALCTARGET_PEAK_1,     // peak 1
        SGIF_CALCTARGET_PEAK_2,     // peak 2
        SGIF_CALCTARGET_PEAK_3,     // peak 3
        SGIF_CALCTARGET_PEAK_4,     // peak 4
        SGIF_CALCTARGET_PEAK_1_2,   // peak 1-peak 2
        SGIF_CALCTARGET_PEAK_1_3,   // peak 1-peak 3
        SGIF_CALCTARGET_PEAK_1_4,   // peak 1-peak 4
        SGIF_CALCTARGET_PEAK_2_3,   // peak 2-peak 3
        SGIF_CALCTARGET_PEAK_2_4,   // peak 2-peak 4
        SGIF_CALCTARGET_PEAK_3_4,   // peak 3-peak 4
    }

    /// <summary>
    /// 滤波方式
    /// </summary>
    internal enum SGIF_FILTERMODE
    {
        /// <summary>
        /// 移动平均
        /// </summary>
        SGIF_FILTERMODE_MOVING_AVERAGE,         // moving average
    }
    /// <summary>
    /// 移动平均
    /// </summary>
    internal enum SGIF_FILTERPARA
    {
        /// <summary>
        /// 1
        /// </summary>
        SGIF_FILTERPARA_AVE_1 = 0,
        /// <summary>
        /// 4
        /// </summary>
        SGIF_FILTERPARA_AVE_4,
        /// <summary>
        /// 16
        /// </summary>
        SGIF_FILTERPARA_AVE_16,
        /// <summary>
        /// 64
        /// </summary>
        SGIF_FILTERPARA_AVE_64,
        /// <summary>
        /// 256
        /// </summary>
        SGIF_FILTERPARA_AVE_256,
        /// <summary>
        /// 1024
        /// </summary>
        SGIF_FILTERPARA_AVE_1024,
        /// <summary>
        /// 4096
        /// </summary>
        SGIF_FILTERPARA_AVE_4096,
        /// <summary>
        /// 16384
        /// </summary>
        SGIF_FILTERPARA_AVE_16384,
        /// <summary>
        /// 65536
        /// </summary>
        SGIF_FILTERPARA_AVE_65536,
        /// <summary>
        /// 262144
        /// </summary>
        SGIF_FILTERPARA_AVE_262144
    }
    /// <summary>
    /// 外部触发模式
    /// </summary>
    internal enum SGIF_TRIGGERMODE
    {
        /// <summary>
        /// ?外部触发1?
        /// </summary>
        SGIF_TRIGGERMODE_EXT1,      // external trigger 1
        /// <summary>
        /// ?外部触发1?
        /// </summary>
        SGIF_TRIGGERMODE_EXT2,      // external trigger 2
    }
   ;
    /// <summary>
    /// 最小显示单位
    /// </summary>
    internal enum SGIF_DISPLAYUNIT
    {
        /// <summary>
        /// 0.01mm
        /// </summary>
        SGIF_DISPLAYUNIT_0000_01MM = 0,
        /// <summary>
        /// 0.001mm
        /// </summary>
        SGIF_DISPLAYUNIT_000_001MM,
        /// <summary>
        /// 0.0001mm
        /// </summary>
        SGIF_DISPLAYUNIT_00_0001MM,
        /// <summary>
        /// 0.00001mm
        /// </summary>
        SGIF_DISPLAYUNIT_0_00001MM,
        /// <summary>
        /// 0.1um
        /// </summary>
        SGIF_DISPLAYUNIT_00000_1UM,
        /// <summary>
        /// 0.01um
        /// </summary>
        SGIF_DISPLAYUNIT_0000_01UM,
        /// <summary>
        /// 0.001um
        /// </summary>
        SGIF_DISPLAYUNIT_000_001UM,

    }
    /// <summary>
    /// OUT同步设定
    /// </summary>
    internal enum SGIF_OUTNO
    {
        /// <summary>
        /// OUT1
        /// </summary>
        SGIF_OUTNO_01 = 0x0001,
        /// <summary>
        /// OUT2
        /// </summary>
        SGIF_OUTNO_02 = 0x0002,
        /// <summary>
        /// OUT3
        /// </summary>
        SGIF_OUTNO_03 = 0x0004,
        /// <summary>
        /// OUT4
        /// </summary>
        SGIF_OUTNO_04 = 0x0008,
        /// <summary>
        /// 全选
        /// </summary>
        SGIF_OUTNO_ALL = 0x000F,        // All OUTs
    }

    /// <summary>
    /// 采样周期
    /// </summary>
    internal enum SGIF_STORAGECYCLE
    {
        /// <summary>
        /// 1X
        /// </summary>
        SGIF_STORAGECYCLE_1,
        /// <summary>
        /// 2X
        /// </summary>
        SGIF_STORAGECYCLE_2,
        /// <summary>
        /// 5X
        /// </summary>
        SGIF_STORAGECYCLE_5,
        /// <summary>
        /// 10X
        /// </summary>
        SGIF_STORAGECYCLE_10,
        /// <summary>
        /// 20X
        /// </summary>
        SGIF_STORAGECYCLE_20,
        /// <summary>
        /// 50X
        /// </summary>
        SGIF_STORAGECYCLE_50,
        /// <summary>
        /// 100X
        /// </summary>
        SGIF_STORAGECYCLE_100,
        /// <summary>
        /// 200X
        /// </summary>
        SGIF_STORAGECYCLE_200,
        /// <summary>
        /// 500X
        /// </summary>
        SGIF_STORAGECYCLE_500,
        /// <summary>
        /// 1000X
        /// </summary>
        SGIF_STORAGECYCLE_1000,
        /// <summary>
        /// 同步输入
        /// </summary>
        SGIF_STORAGECYCLE_TIMING,
    }

    /// <summary>
    /// 采样频率
    /// </summary>
    internal enum SGIF_SAMPLINGCYCLE
    {
        /// <summary>
        /// SG5000-590KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_590KHZ,
        /// <summary>
        /// SG5000-400KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_400KHZ,
        /// <summary>
        /// SG5000-200KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_200KHZ,
        /// <summary>
        /// SG5000-88KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_88KHZ,
        /// <summary>
        /// SG5000-50KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_50KHZ,
        /// <summary>
        /// SG5000-20KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_20KHZ,
        /// <summary>
        /// SG5000-10KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_10KHZ,
        /// <summary>
        /// SG5000-5KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_5KHZ,
        /// <summary>
        /// SG5000-1KHZ
        /// </summary>
        SGIF_5000_SAMPLINGCYCLE_1KHZ,
        /// <summary>
        /// SG3000-88KHZ
        /// </summary>
        SGIF_3000_SAMPLINGCYCLE_88KHZ = 10,
        /// <summary>
        /// SG3000-88KHZ
        /// </summary>
        SGIF_3000_SAMPLINGCYCLE_50KHZ,
        /// <summary>
        /// SG3000-20KHZ
        /// </summary>
        SGIF_3000_SAMPLINGCYCLE_20KHZ,
        /// <summary>
        /// SG3000-10KHZ
        /// </summary>
        SGIF_3000_SAMPLINGCYCLE_10KHZ,
        /// <summary>
        /// SG3000-5KHZ
        /// </summary>
        SGIF_3000_SAMPLINGCYCLE_5KHZ,
        ///// <summary>
        ///// SG3000-2KHZ
        ///// </summary>
        //SGIF_3000_SAMPLINGCYCLE_2KHZ,
        /// <summary>
        /// SG3000-1KHZ
        /// </summary>
        SGIF_3000_SAMPLINGCYCLE_1KHZ
    }

    /// <summary>
    /// 防干扰模式
    /// </summary>
    internal enum SGIF_MUTUAL_INTERFERENCE_PREVENTION
    {
        /// <summary>
        /// OFF
        /// </summary>
        SGIF_MUTUAL_INTERFERENCE_PREVENTION_OFF,
        /// <summary>
        /// AB-ON
        /// </summary>
        SGIF_MUTUAL_INTERFERENCE_PREVENTION_AB_ON,
        /// <summary>
        /// ABC-ON
        /// </summary>
        SGIF_MUTUAL_INTERFERENCE_PREVENTION_ABC_ON,
    }
    /// <summary>
    /// 选通时间
    /// </summary>
    internal enum SGIF_STOROBETIME
    {
        /// <summary>
        /// 2ms
        /// </summary>
        SGIF_STOROBETIME_2MS,
        /// <summary>
        /// 5ms
        /// </summary>
        SGIF_STOROBETIME_5MS,
        /// <summary>
        /// 10ms
        /// </summary>
        SGIF_STOROBETIME_10MS,
        /// <summary>
        /// 20ms
        /// </summary>
        SGIF_STOROBETIME_20MS,
    }
    /// <summary>
    /// 操作模式
    /// </summary>
    internal enum SGIF_MODE
    {
        /// <summary>
        /// 一般模式
        /// </summary>
        SGIF_MODE_NORMAL,
        /// <summary>
        /// 通信模式
        /// </summary>
        SGIF_MODE_COMMUNICATION
    }

    /// <summary>
    /// 运行模式
    /// </summary>
    internal enum SGIF_EmRunningMode
    {
        /// <summary>
        /// OFF
        /// </summary>
        SGIF_EmRunningMode_cont,
        /// <summary>
        /// 定时
        /// </summary>
        SGIF_EmRunningMode_time,
        /// <summary>
        /// 触发
        /// </summary>
        SGIF_EmRunningMode_trig,
    }
    /// <summary>
    /// 检测方向
    /// </summary>
    internal enum SGIF_EmDirection
    {
        /// <summary>
        /// 正负计时
        /// </summary>
        SGIF_EmDirection_all,
        /// <summary>
        /// 正计时
        /// </summary>
        SGIF_EmDirection_pos,
        /// <summary>
        /// 负计时
        /// </summary>
        SGIF_EmDirection_neg,
    }
    /// <summary>
    /// 编码器模式
    /// </summary>
    internal enum SGIF_EmEncoderInput
    {
        /// <summary>
        /// 一相一递增
        /// </summary>
        SGIF_EmEncoderInput_1x1,
        /// <summary>
        /// 二相一递增
        /// </summary>
        SGIF_EmEncoderInput_2x1,
        /// <summary>
        /// 二相二递增
        /// </summary>
        SGIF_EmEncoderInput_2x2,
        /// <summary>
        /// 二相四递增
        /// </summary>
        SGIF_EmEncoderInput_2x4,
    }
    /// <summary>
    /// 脉宽最低输入时间
    /// </summary>
    internal enum SGIF_EmInputTime
    {
        /// <summary>
        /// 100ns
        /// </summary>
        SGIF_EmInputTime_100 = 100,
        /// <summary>
        /// 200ns
        /// </summary>
        SGIF_EmInputTime_200 = 200,
        /// <summary>
        /// 500ns
        /// </summary>
        SGIF_EmInputTime_500 = 500,
        /// <summary>
        /// 1000ns
        /// </summary>
        SGIF_EmInputTime_1000 = 1000,
        /// <summary>
        /// 2000ns
        /// </summary>
        SGIF_EmInputTime_2000 = 2000,
        /// <summary>
        /// 5000ns
        /// </summary>
        SGIF_EmInputTime_5000 = 5000,
        /// <summary>
        /// 10000ns
        /// </summary>
        SGIF_EmInputTime_10000 = 10000,
        /// <summary>
        /// 20000ns
        /// </summary>
        SGIF_EmInputTime_20000 = 20000,
    }
    #endregion

    #region Structure
    /// <summary>
    /// 实时数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SGIF_FLOATVALUE_OUT
    {
        /// <summary>
        /// OUT编号
        /// </summary>
        internal int OutNo;
        /// <summary>
        /// 数据类型
        /// </summary>
        internal SGIF_FLOATRESULT FloatResult;
        /// <summary>
        /// 数据值
        /// </summary>
        internal float Value;
    }
    /// <summary>
    /// 存储区数据
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SGIF_FLOATVALUE
    {
        internal SGIF_FLOATVALUE(bool IsValid)
        {
            if (IsValid)
            {
                FloatResult = SGIF_FLOATRESULT.SGIF_FLOATRESULT_VALID;
                Value = 0F;
            }
            else
            {
                FloatResult = SGIF_FLOATRESULT.SGIF_FLOATRESULT_INVALID;
                Value = -999.999F;
            }
        }
        /// <summary>
        /// 数据类型
        /// </summary>
        internal SGIF_FLOATRESULT FloatResult;
        /// <summary>
        /// 数据值
        /// </summary>
        internal float Value;
    }
    /// <summary>
    /// 通讯地址
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct SGIF_OPENPARAM_ETHERNET
    {
        internal byte ID1;
        internal byte ID2;
        internal byte ID3;
        internal byte ID4;
        internal SGIF_OPENPARAM_ETHERNET(string iIpAddress)
        {
            try
            {
                string[] mIpArr = iIpAddress.Split('.');
                if (mIpArr.Length < 4)
                {
                    ID1 = ID2 = ID3 = ID4 = 0;
                }
                else
                {
                    ID1 = Convert.ToByte(mIpArr[0]);
                    ID2 = Convert.ToByte(mIpArr[1]);
                    ID3 = Convert.ToByte(mIpArr[2]);
                    ID4 = Convert.ToByte(mIpArr[3]);
                }
            }
            catch (Exception)
            {
                ID1 = ID2 = ID3 = ID4 = 0;
            }
        }
    }
    #endregion

    #region Delegate
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void SGIF_DataStorageOneTimeCallBack(int DeviceID);
    #endregion

    #region  Method
    internal static class SGLinkFuc
    {
        #region 未知类别
        /// <summary>
        /// 存储（OUT编号设定）获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="Status">状态</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetStorageTarget(int DeviceID, int OutNo, out bool Status);
        /// <summary>
        /// 触发模式获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="TriggerMode">触发模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetTriggerMode(int DeviceID, int OutNo, out SGIF_TRIGGERMODE TriggerMode);
        /// <summary>
        /// 触发模式设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="TriggerMode">触发模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetTriggerMode(int DeviceID, int OutNo, SGIF_TRIGGERMODE TriggerMode);
        #endregion

        #region 设备连接与断开
        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OpenParam">连接参数</param>
        /// <param name="TimeOut">连接超时</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        extern internal static RC SGIF_OpenDeviceETHER(int DeviceID, out SGIF_OPENPARAM_ETHERNET OpenParam, uint TimeOut = 3000);
        /// <summary>
        /// 连接设备
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OpenParam">连接参数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_OpenDeviceETHER_test(int DeviceID, out SGIF_OPENPARAM_ETHERNET OpenParam);
        /// <summary>
        /// 断开设备连接
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_CloseDevice(int DeviceID);
        /// <summary>
        /// 设备状态刷新
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_Refresh(int DeviceID);
        #endregion

        #region 设备信息
        /// <summary>
        /// 获取设备类型
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="DeviceType">设备类型</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetDeviceType(int DeviceID, out SGIF_DEVICE_TYPE DeviceType);
        /// <summary>
        /// 获取光斑类型
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="EissionSpotType">光斑类型【0=小光斑；1=大光斑】</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetCameraSpotType(int DeviceID, out int EissionSpotType);
        /// <summary>
        /// 获取当前程序号
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="ProgramNo">程序编号（0到7）</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetProgramNo(int DeviceID, out int ProgramNo);
        /// <summary>
        /// 切换指定程序号
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="ProgramNo">程序编号（0到7）</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetProgramNo(int DeviceID, int ProgramNo);
        #endregion

        #region 操作模式变更
        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Mode">模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetMode(int DeviceID, SGIF_MODE Mode);
        #endregion

        #region 测量设定
        /// <summary>
        /// 主动传感头计数获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="NumOfUsedHeads">传感头计数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetNumOfUsedHeads(int DeviceID, out int NumOfUsedHeads);
        /// <summary>
        /// 主动OUT计数获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="NumOfUsedOut">OUT计数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetNumOfUsedOut(int DeviceID, out int NumOfUsedOut);
        /// <summary>
        /// 选通时间获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="StrobeTime">输出形式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetStrobeTime(int DeviceID, out SGIF_STOROBETIME StrobeTime);
        /// <summary>
        /// 防止互相干扰获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Status">状态</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetMutualInterferencePrevention(int DeviceID, out SGIF_MUTUAL_INTERFERENCE_PREVENTION Status);
        /// <summary>
        /// 采样周期获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="SamplingCycle">采样周期</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetSamplingCycle(int DeviceID, out SGIF_SAMPLINGCYCLE SamplingCycle);
        /// <summary>
        /// 同步设定获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="Status">状态</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetSynchronization(int DeviceID, out int OutNo, out bool Status);
        /// <summary>
        /// 显示最小单位获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="DisplayUnit">显示最小单位</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetDisplayUnit(int DeviceID, out int OutNo, out SGIF_DISPLAYUNIT DisplayUnit);
        /// <summary>
        /// 测量方法获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="CalcMode">测量方法</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetCalcMode(int DeviceID, out int OutNo, out SGIF_CALCMODE CalcMode);

        /// <summary>
        /// 偏移获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="Offset">偏移量</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetOffset(int DeviceID, out int OutNo, out int Offset);

        /// <summary>
        /// 滤波器设定获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="FilterMode">滤波器模式</param>
        /// <param name="FilterPara">滤波参数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetFilter(int DeviceID, out int OutNo, out SGIF_FILTERMODE FilterMode, out SGIF_FILTERPARA FilterPara);
        /// <summary>
        /// 缩放设定获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="InputValue1">点1的测量值</param>
        /// <param name="OutputValue1">点1的显示值</param>
        /// <param name="InputValue2">点2的测量值</param>
        /// <param name="OutputValue2">点2的显示值</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetScaling(int DeviceID, out int OutNo, out int InputValue1, out int OutputValue1, out int InputValue2, out int OutputValue2);

        /// <summary>
        /// 待计算的OUT(Ave,Max,Min,P-P)获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="TargetOut">多个OUT或HEAD组合</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetOutOperation(int DeviceID, out int OutNo, out SGIF_OUTNO TargetOut);
        /// <summary>
        /// 待计算的OUT(Add,Sub)获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="AddSub1">待计算的OUT编号1</param>
        /// <param name="AddSub2">待计算的OUT编号2</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetOutAddSub(int DeviceID, out int OutNo, out int AddSub1, out int AddSub2);
        /// <summary>
        /// 测量目标获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="CalcTarget">计算标签</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetCalcTarget(int DeviceID, out int OutNo, out SGIF_CALCTARGET CalcTarget);
        /// <summary>
        /// 计算方式获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="CalcMethod">计算方法</param>
        /// <param name="HeadOutNo">传感头或OUT编号</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetCalcMethod(int DeviceID, out int OutNo, out SGIF_CALCMETHOD CalcMethod, out int HeadOutNo);
        /// <summary>
        /// 防止互相干扰组获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="Group">组</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetMutualInterferencePreventionGroup(int DeviceID, int HeadNo, out SGIF_MUTUAL_INTERFERENCE_PREVENTION_GROUP Group);
        /// <summary>
        /// Laser Ctrl组获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="LaserCtrlGroup">LaserCtrl组</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetLaserCtrlGroup(int DeviceID, int HeadNo, out SGIF_LASER_CTRL_GROUP LaserCtrlGroup);
        /// <summary>
        /// 中位数获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="Median">中位数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetMedian(int DeviceID, int HeadNo, out SGIF_MEDIAN Median);
        /// <summary>
        /// 屏蔽设定获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="Status">状态</param>
        /// <param name="Pos1">位置1</param>
        /// <param name="Pos2">位置2</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetMask(int DeviceID, int HeadNo, out bool Status, out int Pos1, out int Pos2);
        /// <summary>
        /// 安装模式获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="ReflectionMode">反射模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetReflectionMode(int DeviceID, int HeadNo, out SGIF_REFLECTIONMODE ReflectionMode);
        /// <summary>
        /// 警报级别获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="AlarmLevel">警报级别</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetAlarmLevel(int DeviceID, int HeadNo, out int AlarmLevel);
        /// <summary>
        /// 警报恢复次数获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="RecoveryNum">恢复次数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetNumRecovery(int DeviceID, int HeadNo, out int RecoveryNum);
        /// <summary>
        /// 警报处理次数获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="AlarmNum">警报次数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetNumAlarm(int DeviceID, int HeadNo, out int AlarmNum);
        /// <summary>
        /// 基准点获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="BasicPoint">基准点</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetBasicPoint(int DeviceID, int HeadNo, out SGIF_BASICPOINT BasicPoint);
        /// <summary>
        /// 测量模式获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="MeasureMode">测量模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetMeasureMode(int DeviceID, int HeadNo, out SGIF_MEASUREMODE MeasureMode);
        /// <summary>
        /// Able控制范围获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="Min">最小值</param>
        /// <param name="Max">最大值</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetAbleMinMax(int DeviceID, int HeadNo, out int Min, out int Max);
        /// <summary>
        /// Able设定获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="AbleMode">Able模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetAbleMode(int DeviceID, int HeadNo, out SGIF_ABLEMODE AbleMode);
        /// <summary>
        /// 公差设定获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="UpperLimit">上限</param>
        /// <param name="LowerLimit">下限</param>
        /// <param name="Hysteresis">滞后</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetTolerance(int DeviceID, int OutNo, out int UpperLimit, out int LowerLimit, out int Hysteresis);
        /// <summary>
        /// 选通时间设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="StrobeTime">选通时间</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetStrobeTime(int DeviceID, SGIF_STOROBETIME StrobeTime);
        /// <summary>
        /// 防止互相干扰组设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Status">组设定</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetMutualInterferencePrevention(int DeviceID, SGIF_MUTUAL_INTERFERENCE_PREVENTION Status);
        /// <summary>
        /// 采样周期设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="SamplingCycle">采样周期</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetSamplingCycle(int DeviceID, int SamplingCycle);

        /// <summary>
        /// 存储（OUT编号规格）设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="Status">状态</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetStorageTarget(int DeviceID, int OutNo, bool Status);
        /// <summary>
        /// 同步设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="Status">状态</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetSynchronization(int DeviceID, int OutNo, bool Status);
        /// <summary>
        /// 设定显示单位设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="DisplayUnit">显示单位</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetDisplayUnit(int DeviceID, int OutNo, SGIF_DISPLAYUNIT DisplayUnit);
        /// <summary>
        /// 检测方法设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="CalcMode">检测模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetCalcMode(int DeviceID, int OutNo, SGIF_CALCMODE CalcMode);
        /// <summary>
        /// 偏移设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="Offset">偏移量</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetOffset(int DeviceID, int OutNo, int Offset);
        /// <summary>
        /// 滤波器设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="FilterMode">滤波模式</param>
        /// <param name="FilterPar">滤波参数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetFilter(int DeviceID, int OutNo, SGIF_FILTERMODE FilterMode, SGIF_FILTERPARA FilterPar);
        /// <summary>
        /// 缩放设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="InputValue1">点1的测量值</param>
        /// <param name="OutputValue1">点1的显示值</param>
        /// <param name="InputValue2">点2的测量值</param>
        /// <param name="OutputValue2">点2的显示值</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetScaling(int DeviceID, int OutNo, int InputValue1, int OutputValue1, int InputValue2, int OutputValue2);
        /// <summary>
        /// 待计算的OUT(Ave,Max,Min,P-P)设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="TargetOut">多个OUT组合</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetOutOperation(int DeviceID, int OutNo, SGIF_OUTNO TargetOut);
        /// <summary>
        /// 待计算的OUT(Add,Sub)设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="AddSub1">待计算的OUT</param>
        /// <param name="AddSub2">待计算的OUT</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetOutAddSub(int DeviceID, int OutNo, int AddSub1, int AddSub2);
        /// <summary>
        /// 测量目标设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="CalcTarget">计算标签</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetCalcTarget(int DeviceID, int OutNo, SGIF_CALCTARGET CalcTarget);
        /// <summary>
        /// 计算方式设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="CalcMethod">计算方法</param>
        /// <param name="HeadOutNo">传感头输出编号</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetCalcMethod(int DeviceID, int OutNo, SGIF_CALCMETHOD CalcMethod, int HeadOutNo);
        /// <summary>
        /// 防止互相干扰组设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="Group">组</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetMutualInterferencePreventionGroup(int DeviceID, int HeadNo, SGIF_MUTUAL_INTERFERENCE_PREVENTION_GROUP Group);
        /// <summary>
        /// LaserCtrl组设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="LaserCtrlGroup">LaserCtrl组</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetLaserCtrlGroup(int DeviceID, int HeadNo, SGIF_LASER_CTRL_GROUP LaserCtrlGroup);

        /// <summary>
        /// 中位数设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="Median">中位数</param>
        /// <returns></returns>

        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetMedian(int DeviceID, int HeadNo, SGIF_MEDIAN Median);

        /// <summary>
        /// 屏蔽设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="OnOff">开关</param>
        /// <param name="Pos1">点位1</param>
        /// <param name="Pos2">点位2</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetMask(int DeviceID, int HeadNo, bool OnOff, int Pos1, int Pos2);


        /// <summary>
        /// 安装模式设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="ReflectionMode">反射模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetReflectionMode(int DeviceID, int HeadNo, SGIF_REFLECTIONMODE ReflectionMode);
        /// <summary>
        /// Able校准取消
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_AbleCancel(int DeviceID);
        /// <summary>
        /// Able校准完成
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_AbleStop(int DeviceID);

        /// <summary>
        /// Able校准开始
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_AbleStart(int DeviceID, int HeadNo);

        /// <summary>
        /// 警报级别设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="AlarmLevel">警报级别</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetAlarmLevel(int DeviceID, int HeadNo, int AlarmLevel);
        /// <summary>
        /// 恢复数量设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="RecoveryNum">恢复次数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetNumRecovery(int DeviceID, int HeadNo, int RecoveryNum);
        /// <summary>
        /// 警报数量设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="AlarmNum">警报数量</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetNumAlarm(int DeviceID, int HeadNo, int AlarmNum);
        /// <summary>
        /// 基准点设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="BasicPoint">基准点</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetBasicPoint(int DeviceID, int HeadNo, SGIF_BASICPOINT BasicPoint);
        /// <summary>
        /// 测量模式设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="MeasureMode">测量模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetMeasureMode(int DeviceID, int HeadNo, SGIF_MEASUREMODE MeasureMode);
        /// <summary>
        /// Able控制范围设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="Min">下限</param>
        /// <param name="Max">上限</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetAbleMinMax(int DeviceID, int HeadNo, int Min, int Max);
        /// <summary>
        /// Able模式设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="HeadNo">传感头编号</param>
        /// <param name="AbleMode">模式</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetAbleMode(int DeviceID, int HeadNo, SGIF_ABLEMODE AbleMode);
        /// <summary>
        /// 公差设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">传感头编号</param>
        /// <param name="UpperLimit">上限</param>
        /// <param name="LowerLimit">下限</param>
        /// <param name="Hystersys">滞后</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetTolerance(int DeviceID, int OutNo, int UpperLimit, int LowerLimit, int Hystersys);
        #endregion

        #region 环境设定
        /// <summary>
        /// 运行模式设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="RunningMode">运行模式 0:OFF/1:定时/2:触发</param>
        /// <param name="CatchDirection">检测方向 0:正负计时/1:正计时/2:负计时</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetRunningMode(int DeviceID, SGIF_EmRunningMode RunningMode, SGIF_EmDirection CatchDirection);
        /// <summary>
        /// 编码器输入模式设置
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="Inputmode">运行模式 0:1相1递增/1:2相1递增/2:2相2递增,3:2相4递增</param>
        /// <param name="InputTime">最小输入时间</param>
        /// <param name="TrigInterval">触发间隔</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetEncoder(int DeviceID, SGIF_EmEncoderInput Inputmode, SGIF_EmInputTime InputTime, int TrigInterval);
        #endregion

        #region 数据存储器
        /// <summary>
        /// 存储周期获取
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="NumStorage">存储数据编号</param>
        /// <param name="StorageCycle">存储周期</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetDataStorage(int DeviceID, out int NumStorage, out SGIF_STORAGECYCLE StorageCycle);
        /// <summary>
        /// 存储周期设定
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="NumStorage">待存储数据的编号</param>
        /// <param name="StorageCycle">存储周期</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetDataStorage(int DeviceID, int NumStorage, SGIF_STORAGECYCLE StorageCycle);
        /// <summary>
        /// 数据存储状态输出
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="StorageState">存储状态</param>
        /// <param name="StorageNum">各输出口存储数量数组（int[]）</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_DataStorageGetStatus(int DeviceID, out bool StorageState, IntPtr StorageNum);

        /// <summary>
        /// 数据存储初始化
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_DataStorageInit(int DeviceID);

        /// <summary>
        /// 数据存储数据输出
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">OUT编号</param>
        /// <param name="NumOfBuffer">数据大小</param>
        /// <param name="OutBuffer">数据数组(float[])</param>
        /// <param name="NumReceived">实际接收的数据个数</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_DataStorageGetData(int DeviceID, int OutNo, int NumOfBuffer, IntPtr OutBuffer, out int NumReceived);

        //[DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        //internal static extern RC SGIF_DataStorageGetDatanew(int DeviceID, int OutNo, int NumOfBuffer, IntPtr OutBuffer, out int NumReceived);

        /// <summary>
        /// 数据存储停止
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_DataStorageStop(int DeviceID);
        /// <summary>
        /// 数据存储开始
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]


        internal static extern RC SGIF_DataStorageStart(int DeviceID);
        /// <summary>
        /// 重置（同步）
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetResetSync(int DeviceID);
        /// <summary>
        /// 重置（多个）
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetResetMulti(int DeviceID, SGIF_OUTNO OutNo);
        /// <summary>
        /// 重置（多个）
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetResetSingle(int DeviceID, int OutNo);
        #endregion

        #region 实时操作
        /// <summary>
        /// 自动归零（同步）
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OnOff">开或关</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetZeroSync(int DeviceID, bool OnOff);
        /// <summary>
        /// 自动归零（多个）
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <param name="OnOff">开或关</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetZeroMulti(int DeviceID, SGIF_OUTNO OutNo, bool OnOff);
        /// <summary>
        /// 自动归零（一个）
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <param name="OnOff">开或关</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetZeroSingle(int DeviceID, int OutNo, bool OnOff);
        /// <summary>
        /// 计时ON/OFF(同步)
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OnOff">开或关</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetTimingSync(int DeviceID, bool OnOff);
        /// <summary>
        /// 计时ON/OFF(多个)
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <param name="OnOff">开或关</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetTimingMulti(int DeviceID, SGIF_OUTNO OutNo, bool OnOff);
        /// <summary>
        /// 计时ON/OFF(一个)
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <param name="OnOff">开或关</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetTimingSingle(int DeviceID, int OutNo, bool OnOff);

        /// <summary>
        /// 全部测量值输出
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="CalcData">测量值数组(SGIF_FLOATVALUE_OUT[])</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetCalcDataALL(int DeviceID, IntPtr CalcData);
        /// <summary>
        /// 多个测量值输出
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <param name="CalcData">测量值数组(SGIF_FLOATVALUE_OUT[])</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetCalcDataMulti(int DeviceID, SGIF_OUTNO OutNo, IntPtr CalcData);
        /// <summary>
        /// 单个测量值输出
        /// </summary>
        /// <param name="DeviceID">设备ID</param>
        /// <param name="OutNo">指定OUT</param>
        /// <param name="CalcData">测量值数组</param>
        /// <returns></returns>
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_GetCalcDataSingle(int DeviceID, int OutNo, out SGIF_FLOATVALUE_OUT CalcData);
        #endregion

        #region 回调相关
        [DllImport("SGIFPJ.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        internal static extern RC SGIF_SetOneDataStorage(int DeviceID, SGIF_DataStorageOneTimeCallBack callbackfunc);
        #endregion
    }
    #endregion
}
