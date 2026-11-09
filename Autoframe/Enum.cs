
namespace Autoframe
{
    #region IO、Cylinder、Axis

    /// <summary>
    /// 数字量输入
    /// </summary>
    public enum DI
    {
        急停,
        启动,
        复位,
        暂停,
        安全门,
        总气压源,
        上料仓安全光栅,
        上料站料仓1料盘有无,
        上料站料仓1料盘到位,
        上料站料仓2料盘有无,
        上料站料仓2料盘到位,
        上料站供料料盘有无,
        上料仓启动,
        安全门前1,
        安全门前2,
        安全门前3,
        安全门前4,
        取料站夹料盘夹紧位,
        DI1_X18,
        取料站托料盘夹紧位,
        DI1_X20,
        入料移载站解锁伸出位,
        入料移载站解锁缩回位,
        入料移载站升降伸出位,
        入料移载站升降缩回位,
        DI1_X25,
        出料移载站解锁伸出位,
        出料移载站解锁缩回位,
        出料移载站升降伸出位,
        出料移载站升降缩回位,
        DI1_X30,
        DI1_X31,
        暂放平台启动,
        暂放平台料盘有无,
        下料仓A启动,
        下料仓B启动,
        下料仓C启动,
        DI2_X05,
        中转取放站料盘夹紧位,
        中转取放站料盘松开位,
        DI2_X08,
        中转暂放平台伸出位,
        中转暂放平台缩回位,
        DI2_X11,
        DI2_X12,
        下料仓安全光栅,
        DI2_X14,
        DI2_X15,
        DI2_X16,
        DI2_X17,
        DI2_X18,
        DI2_X19,
        DI2_X20,
        DI2_X21,
        DI2_X22,
        DI2_X23,
        DI2_X24,
        DI2_X25,
        DI2_X26,
        DI2_X27,
        DI2_X28,
        DI2_X29,
        DI2_X30,
        DI2_X31,
        下料仓A上料前解锁伸出位,
        DI3_X01,
        下料仓A上料后解锁伸出位,
        DI3_X03,
        下料仓B上料前解锁伸出位,
        DI3_X05,
        下料仓B上料后解锁伸出位,
        DI3_X07,
        下料仓C上料前解锁伸出位,
        DI3_X09,
        下料仓C上料后解锁伸出位,
        DI3_X11,
        下料仓A下料前解锁伸出位,
        DI3_X13,
        下料仓A下料后解锁伸出位,
        DI3_X15,
        下料仓B下料前解锁伸出位,
        DI3_X17,
        下料仓B下料后解锁伸出位,
        DI3_X19,
        下料仓C下料前解锁伸出位,
        DI3_X21,
        下料仓C下料后解锁伸出位,
        下料仓A下料侧推伸出位,
        下料仓A下料侧推缩回位,
        下料仓B下料侧推伸出位,
        下料仓B下料侧推缩回位,
        下料仓C下料侧推伸出位,
        下料仓C下料侧推缩回位,
        下料仓A下料满料位,
        下料仓B下料满料位,
        下料仓C下料满料位,
        下料站吸嘴1真空到位,
        下料站吸嘴2真空到位,
        下料站吸嘴3真空到位,
        下料站吸嘴4真空到位,
        DI4_X04,
        DI4_X05,
        DI4_X06,
        DI4_X07,
    }


    public enum DO
    {
        红灯,
        黄灯,
        绿灯,
        蜂鸣器,
        安全门锁前1,
        安全门锁前2,
        安全门锁前3,
        安全门锁前4,
        DO1_Y08,
        日光灯,
        启动灯,
        上料仓启动灯,
        DO1_Y12,
        入料移载站解锁伸出,
        入料移载站解锁缩回,
        入料移载站升降伸出,
        入料移载站升降缩回,
        DO1_Y17,
        出料移载站解锁伸出,
        出料移载站解锁缩回,
        出料移载站升降伸出,
        出料移载站升降缩回,
        DO1_Y22,
        DO1_Y23,
        取料站夹料盘夹紧,
        取料站夹料盘松开,
        DO1_Y26,
        取料站托料盘夹紧,
        取料站托料盘松开,
        DO1_Y29,
        DO1_Y30,
        DO1_Y31,
        暂放平台启动灯,
        DO2_Y01,
        下料仓A启动灯,
        下料仓B启动灯,
        下料仓C启动灯,
        DO2_Y05,
        DO2_Y06,
        CCD1上料定位拍照,
        CCD2下AOI检测拍照,
        CCD3上AOI检测拍照,
        CCD4下AOI检测拍照,
        CCD5下料定位拍照,
        CCD6下料定位拍照,
        CCD7料盘定位拍照,
        CCD2硬触发复位,
        CCD3硬触发复位,
        CCD4硬触发复位,
        DO2_Y17,
        DO2_Y18,
        DO2_Y19,
        DO2_Y20,
        DO2_Y21,
        DO2_Y22,
        DO2_Y23,
        DO2_Y24,
        DO2_Y25,
        DO2_Y26,
        DO2_Y27,
        DO2_Y28,
        DO2_Y29,
        DO2_Y30,
        DO2_Y31,
        下料站吸嘴1吸真空,
        下料站吸嘴1破真空,
        下料站吸嘴2吸真空,
        下料站吸嘴2破真空,
        下料站吸嘴3吸真空,
        下料站吸嘴3破真空,
        下料站吸嘴4吸真空,
        下料站吸嘴4破真空,
        下料仓A上料前解锁伸出,
        下料仓A上料前解锁缩回,
        下料仓A上料后解锁伸出,
        下料仓A上料后解锁缩回,
        下料仓B上料前解锁伸出,
        下料仓B上料前解锁缩回,
        下料仓B上料后解锁伸出,
        下料仓B上料后解锁缩回,
        下料仓C上料前解锁伸出,
        下料仓C上料前解锁缩回,
        下料仓C上料后解锁伸出,
        下料仓C上料后解锁缩回,
        下料仓A下料前解锁伸出,
        下料仓A下料前解锁缩回,
        下料仓A下料后解锁伸出,
        下料仓A下料后解锁缩回,
        下料仓B下料前解锁伸出,
        下料仓B下料前解锁缩回,
        下料仓B下料后解锁伸出,
        下料仓B下料后解锁缩回,
        下料仓C下料前解锁伸出,
        下料仓C下料前解锁缩回,
        下料仓C下料后解锁伸出,
        下料仓C下料后解锁缩回,
        中转取放站料盘夹紧,
        中转取放站料盘松开,
        中转暂放平台伸出,
        中转暂放平台缩回,
        下料仓A下料侧推伸出,
        下料仓A下料侧推缩回,
        下料仓B下料侧推伸出,
        下料仓B下料侧推缩回,
        下料仓C下料侧推伸出,
        下料仓C下料侧推缩回,
        DO2_OUT11,
        DO2_OUT12,
        DO2_OUT13,
        DO2_OUT14,
        DO2_OUT15,
        DO2_OUT16,
        DO2_OUT17,
        DO2_OUT18,
        DO2_OUT19,
        DO2_OUT20,
        DO2_OUT21,
        DO2_OUT22,
        DO2_OUT23,
        DO2_OUT24,
    }

    /// <summary>
    /// 模拟量输入枚举
    /// </summary>
    public enum AI
    {
        //    温度,
        //    气压,
    }

    /// <summary>
    /// 气缸
    /// </summary>
    public enum Cylinder
    {
        入料移载站解锁,
        入料移载站升降,
        出料移载站解锁,
        出料移载站升降,
        取料站夹料盘,
        取料站托料盘,
        下料仓A上料前解锁,
        下料仓A上料后解锁,
        下料仓B上料前解锁,
        下料仓B上料后解锁,
        下料仓C上料前解锁,
        下料仓C上料后解锁,
        下料仓A下料前解锁,
        下料仓A下料后解锁,
        下料仓B下料前解锁,
        下料仓B下料后解锁,
        下料仓C下料前解锁,
        下料仓C下料后解锁,
        中转取放站料盘,
        中转暂放平台,
        下料仓A下料侧推,
        下料仓B下料侧推,
        下料仓C下料侧推,
    }

    /// <summary>
    /// 轴
    /// </summary>
    public enum AXIS
    {
        料盘移载Y轴 = 0,
        料盘供料Z轴,
        上料取放X轴,
        上料取放大Z轴,
        上料取放小Z轴,
        位移激光检测Y轴,
        下AOI1检测Y轴,
        下AOI1检测Z轴,
        上AOI检测Y轴,
        上AOI检测Z轴,
        下AOI2检测Y轴,
        下AOI2检测Z轴,
        中转取放X轴,
        中转取放Y轴,
        中转取放Z轴,
        下料取放X轴,
        下料取放Y轴,
        下料取放吸嘴1Z轴,
        下料取放吸嘴1U轴,
        下料取放吸嘴2Z轴,
        下料取放吸嘴2U轴,
        下料取放吸嘴3Z轴,
        下料取放吸嘴3U轴,
        下料取放吸嘴4Z轴,
        下料取放吸嘴4U轴,
        下料仓移载X1轴,
        下料仓顶升Z1轴,
        下料仓移载X2轴,
        下料仓顶升Z2轴,
        下料仓移载X3轴,
        下料仓顶升Z3轴,
    }

    #endregion

    #region 网口、串口

    /// <summary>
    /// TCP客户端
    /// </summary>
    public enum TCP_Client
    {
        CCD1,
        CCD2,
        CCD3,
        CCD4,
        CCD5,
        CCD6,
        CCD7,
        Laser,
        PLC,
        机械手,
        扫码枪,
        PDCA,
        HIVE,
    };

    /// <summary>
    /// TCP服务器
    /// </summary>
    public enum TCP_Server
    {
        //Server0,
        //Server1,
    }

    /// <summary>
    /// 串口
    /// </summary>
    public enum COM
    {
        光源,
    };

    #endregion


    #region 寄存器
    /// <summary>
    /// 位寄存器
    /// </summary>
    public enum RegBit
    {
        //上料仓站
        上料仓站初始化完成,
        上料取放站可取料,

        上料仓站初始化可放料,


        //上站取放站
        上料取放站初始化完成,
        上料取放站取料完成,
        上料取放站放料完成,

        上料取放站拍照完成,

        上料取放站初始化安全,

        上料取放站初始化放料完成,
        通知入料接驳站料盘有无拍照完成,
        CCD1拍料盘有无结果,

        //入料接驳站
        入料接驳站初始化完成,
        入料接驳站通知可放料,

        入料接驳站通知可拍照,
        入料接驳站通知拍料盘有无,

        //激光位移检测站
        激光位移检测站初始化完成,


        //下相机AOI1站
        下相机AOI1站初始化完成,


        //上相机AOI检测站
        上相机AOI站初始化完成,

        //下相机AOI2站
        下相机AOI2站初始化完成,

        //出料接驳站
        出料接驳站初始化完成,
        出料接驳站通知可取料,


        //中转取放站
        中转取放站初始化完成,
        下料仓A移载放料完成,
        下料仓B移载放料完成,
        下料仓C移载放料完成,
        暂放平台有料,
        中转取放站取料完成,

        //下料仓站

        下料仓A站通知中转可放料,
        下料仓B站通知中转可放料,
        下料仓C站通知中转可放料,



        //下料站
        UnloadGantryInitFinished,

        TranferGantryInitSafety,
        BinANotifyTranferInitSafety,
        BinBNotifyTranferInitSafety,
        BinCNotifyTranferInitSafety,

        BinAInitSnapRequest,
        BinBInitSnapRequest,
        BinCInitSnapRequest,

        BinAInitSnapFinish,
        BinBInitSnapFinish,
        BinCInitSnapFinish,

        BinAInitSnapHaveTray,
        BinBInitSnapHaveTray,
        BinCInitSnapHaveTray,

        UnloadInitSnapProcessDone,

        BinARequestGantryLocateTray,
        BinBRequestGantryLocateTray,
        BinCRequestGantryLocateTray,

        UnloadGantryInitSafety,//
        UnloadGantrySnapAFinish,
        UnloadGantrySnapBFinish,
        UnloadGantrySnapCFinish,

        UnloadGantrySnapAOK,
        UnloadGantrySnapBOK,
        UnloadGantrySnapCOK,
    }
    /// <summary>
    /// 整型寄存器
    /// </summary>
    public enum RegInt
    {
        /// <summary>
        /// 站位运行进度百分比，用于界面显示进度条      
        /// </summary>
        Int_Process_Step,
        SelectMaintenanceState,
        BufferBin,
        LoadingBin,
        UnLoadingBin,

        Int_ReworkCount
    };

    /// <summary>
    /// 浮点型寄存器
    /// </summary>
    public enum RegDouble
    {
        CT,
        nRowdis,//料盘行间距,垂直显示器为行，
        nColdis//料盘列间距,平行显示器为列
    };

    /// <summary>
    /// 字符串寄存器
    /// </summary>
    public enum RegString
    {
        Platform2ProductGUID,
        str_WorkMode,//返工模式
        OrderSN,
        OrderCreatTime,
        Str_OutputTransMoveID,
    }

    public enum WritePLC
    {
        //入料接驳站
        磁浮拍照工位到位信号 = 1000,//拍照
        磁浮拍照工位完成信号 = 1001,//拍照

        磁浮上料工位到位信号 = 1064,//上料
        磁浮上料工位完成信号 = 1065,//上料


        //激光位移检测站
        高度检测步长 = 1010,
        高度检测行数 = 1012,
        高度检测完成信号位 = 1015,
        高度检测到位信号位 = 1014,

        //下相机AOI1检测站
        缺陷检测工位1步长 = 1020,
        缺陷检测工位1行数 = 1022,
        缺陷检测工位1完成信号位 = 1025,
        缺陷检测工位1到位信号位 = 1024,

        //上相机AOI检测站
        缺陷检测工位2步长 = 1030,
        缺陷检测工位2行数 = 1032,
        缺陷检测工位2完成信号位 = 1035,
        缺陷检测工位2到位信号位 = 1034,

        //下相机AOI2检测站
        缺陷检测工位3步长 = 1040,
        缺陷检测工位3行数 = 1042,
        缺陷检测工位3完成信号位 = 1045,
        缺陷检测工位3到位信号位 = 1044,

        //出料接驳站
        下料工位完成信号 = 1051,
        下料工位到位信号 = 1050,

        上位机准备ready = 1060,
        上位机安全信号 = 1063,

        使用的载具编号 = 1062,
        TrayIDRegister = 1082,
        TrayName = 1084


    }

    public enum ReadPLC
    {
        //入料接驳站
        磁浮拍照工位到位信号 = 1000,
        磁浮拍照工位动子号 = 1002,

        磁浮上料工位到位信号 = 1064,//上料
        磁浮上料工位动子号 = 1066,//上料

        //激光位移检测站
        高度检测工位动子号 = 1013,
        高度检测到位信号位 = 1014,

        //下相机AOI1检测
        缺陷检测工位1工位动子号 = 1023,
        缺陷检测工位1到位信号位 = 1024,

        //上相机AOI检测
        缺陷检测工位2工位动子号 = 1033,
        缺陷检测工位2到位信号位 = 1034,

        //下相机AOI2检测
        缺陷检测工位3工位动子号 = 1043,
        缺陷检测工位3到位信号位 = 1044,

        //出料接驳站
        下料工位到位信号 = 1050,
        下料工位动子号 = 1052,


        PLC准备ready = 1061,
        使用的载具编号 = 1062,
        TrayIDRegister = 1082



    }
    #endregion

    #region 物料、事件
    /// <summary>
    /// 生产主料枚举
    /// </summary>
    public enum Material
    {
        //主料，固定为0，不能更改
        //Chassis = 0,

        //辅料
        //Screw1,
        //Screw2,
        OpticalLens = 0,
    }

    /// <summary>
    /// 抛料原因
    /// </summary>
    public enum ThrowReason
    {
        //抛料原因固定格式为：物料名_抛料原因
        //Chassis_螺孔定位NG,
        //Chassis_相机定位NG,

        //Screw1_相机定位NG,
        //Screw1_吸嘴真空NG,

        //Screw2_相机定位NG,
        //Screw2_吸嘴真空NG,
        OpticalLens_模型异常 = 0,
        OpticalLens_点子 = 1,
        OpticalLens_印子 = 2,
        OpticalLens_划伤 = 3,
        OpticalLens_异物 = 4,
        OpticalLens_切割不良 = 5,
        OpticalLens_崩点 = 6,
        OpticalLens_膜裂 = 7,
        OpticalLens_物料放反 = 8,
        OpticalLens_欠胶 = 9,
        OpticalLens_胶气泡 = 10,
        OpticalLens_外框不良 = 11,
        OpticalLens_丝印透光 = 12,

        OpticalLens_定位失败 = 999
    }

    /// <summary>
    /// 计划停机枚举
    /// </summary>
    public enum PlannedDown
    {
        吃饭,
        维护保养,
        换胶水,
    }

    /// <summary>
    /// 非计划停机枚举
    /// </summary>
    public enum ErrCustom
    {
        自定义错误1,
        自定义错误2,
        自定义错误3,
    }
    #endregion

    

    /// <summary>
    /// 自动流程
    /// </summary>
    public enum AutoProcess
    {
        //0--99：做料流程
        正常生产 = 0,
        空跑,
        CPK,
        //GRR,

        //100--199：标定类型
        相机标定 = StationEx.ProcessCalib,
        CCD5第1个吸嘴相机9点标定,

        CCD6第1个吸嘴相机9点标定,

        CCD5第1个吸嘴的5点标定,
        CCD5第2个吸嘴的5点标定,
        CCD5第3个吸嘴的5点标定,
        CCD5第4个吸嘴的5点标定,

        CCD6第1个吸嘴的5点标定,
        CCD6第2个吸嘴的5点标定,
        CCD6第3个吸嘴的5点标定,
        CCD6第4个吸嘴的5点标定,

        CCD5注册模板吸嘴1,
        CCD5注册模板吸嘴2,
        CCD5注册模板吸嘴3,
        CCD5注册模板吸嘴4,

        CCD6注册模板吸嘴1,
        CCD6注册模板吸嘴2,
        CCD6注册模板吸嘴3,
        CCD6注册模板吸嘴4,
        CCD7与CCD5关联,
        CCD7与CCD6关联,



        //200--299：GRR类型
        CCD1相机静态 = StationEx.ProcessGrr,
        CCD2相机静态,
        CCD3相机静态,
        CCD4相机静态,
        CCD5吸嘴1静态测试,

        CCD6吸嘴1静态测试,

        CCD5吸嘴1动态测试,

        CCD6吸嘴1动态测试,

        CCD7静态测试,
        CCD7动态测试,

        CCD1相机动态,
        CCD2相机动态,
        CCD3相机动态,
        CCD4相机动态,



        //300--......：自定义测试项
        测试1 = 300,
    }
}
