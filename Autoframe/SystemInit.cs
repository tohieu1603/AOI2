using AF.Card;
using AF.Station;
using AF.Common;
using AF.Data;
using AF.Card.UI;
using Autoframe.UI;
using AF.Upload;
using AF.Upload.UI;
using AF.Communicate;
using AF.Upload.Hive;
using AutoFrame.DBExtend;
using StackExchange.Redis;
using System;
using AutoFrame;
using AF.Station.UI;
using AutoFrame.UI;
using System.Windows.Forms;

namespace Autoframe
{
    static class SystemInit
    {
        /// <summary>
        ///系统初始化
        /// </summary>
        public static void Init()
        {
            //绑定安全门，安全门无法通过 Hardware配置 来逻辑取反，可以在 BindSafetyDoor() 进行逻辑取反。
            SystemIO.BindSafetyDoor(
                new object[] { DI.安全门前1, DI.安全门前2, DI.安全门前3, DI.安全门前4 },
                new object[] { },
                new object[] { },
                new object[] { },
                DI.安全门,
                new object[] { DO.安全门锁前1, DO.安全门锁前2, DO.安全门锁前3, DO.安全门锁前4 },
                new object[] { },
                new object[] { },
                new object[] { },
                new bool[] { false, true });
            //绑定系统IO，急停无法通过 Hardware配置 来逻辑取反。
            SystemIO.BindDI(DI.急停, DI.暂停, DI.复位, DI.启动, new object[] { DI.总气压源 });
            SystemIO.BindDO(DO.红灯, DO.绿灯, DO.黄灯, DO.蜂鸣器, 10);
            //添加安全光栅
            //SystemIO.AddSafetyCurtain(DI.安全光栅, new object[] { AXIS.下料Y, AXIS.复检Y2 }, null);
            //SystemIO.AddSafetyCurtain(DI.上料仓安全光栅, new object[] { AXIS.料盘移载Y轴, AXIS.料盘供料Z轴 }, null);
            //SystemIO.AddSafetyCurtain(DI.下料仓安全光栅, new object[] { AXIS.下料仓移载X1轴, AXIS.下料仓顶升Z1轴, AXIS.下料仓移载X2轴, AXIS.下料仓顶升Z2轴, AXIS.下料仓移载X3轴, AXIS.下料仓顶升Z3轴 }, null);

            //转盘初始化
            //MyRotaryFixture.Init();

            //PDCA，若不需要可屏蔽
            InitPDCA();

            //Hive，若不需要可屏蔽
            //HiveMgr.Init(TCP_Client.HIVE.GetTcp());

            //添加自定义log
            //Logger.AddLogger("自定义1");

            //增加自定义页
            Form_Auto.AddPage(new Form_InspectInfo(), new Form_UnloadInfo());
            Form_Manual.AddPage(new HandleCyForm(), new Frm_SG());
            //Form_Data.AddPage(new Form_Test(), new Form_Test());
            //Form_Setting.AddPage(new Form_Test(), new Form_Test());
            if (HiveMgr.Enable)
                Form_Upload.AddPage(new Form_HiveOnboard());

            //Auto界面的添加自定义面板，使用框架的 Form_Auto_CustomPanel 可以快速添加IO。也可以添加自定义的UserControl
            Form_Auto.SetCustomPanel(new object[] { DI.总气压源 }, new object[] { DO.CCD2下AOI检测拍照, DO.CCD3上AOI检测拍照, DO.CCD4下AOI检测拍照 });
            //Form_Auto.SetCustomPanel(new UserControl());
        }

        /// <summary>
        ///添加站位
        /// </summary>
        public static void AddStation()
        {
            //StationMgr.AddStation(new StationRecheck("复检站"), 1);
            //StationMgr.AddStation(new StationUnload("下料站") , 1, true, new Form_Test(), new Form_Test());
            StationMgr.AddStation(new StationLoading("上料仓站"), 1);11
            StationMgr.AddStation(new StationLoadingPickEx("上料取放站"), 1);
            StationMgr.AddStation(new StationFeedTrans("入料接驳站"), 1);
            StationMgr.AddStation(new StationLaserDisp("激光位移检测站"), 1);
            StationMgr.AddStation(new StationAOIExDown1("下相机AOI1站"), 1);
            StationMgr.AddStation(new StationAOIExDown1("上相机AOI站"), 1);
            StationMgr.AddStation(new StationAOIExDown1("下相机AOI2站"), 1);
            StationMgr.AddStation(new StationOutputTrans("出料接驳站"), 1);
            StationMgr.AddStation(new StationTransferPick("中转取放站"), 1);

            StationMgr.AddStation(new StationUnloadGantryEx("下料取放站"), 2);

            StationMgr.AddStation(new StationUnloadBinEx("下料仓A站"), 2);
            StationMgr.AddStation(new StationUnloadBinEx("下料仓B站"), 2);
            StationMgr.AddStation(new StationUnloadBinEx("下料仓C站"), 2);
        }

        /// <summary>
        /// 轴运动安全判定
        /// </summary>
        /// <param name="nAxis">轴号</param>
        /// <param name="dbTargetPos">目标位置，回原、Jog会传null</param>
        /// <param name="strMsg">危险操作信息</param>
        /// <returns></returns>
        public static bool IsAxisSafe(int nAxis, double? dbTargetPos, ref string strMsg)
        {
            switch ((AXIS)nAxis)
            {            
                case AXIS.料盘移载Y轴:
                    double Pos1 = StationLoading.POINT.安全位.GetPos(AXIS.料盘供料Z轴, "上料仓站");

                    if (!(AXIS.料盘供料Z轴.GetPos() - Pos1 <= 5 && AXIS.料盘供料Z轴.GetPos() - Pos1 >= -5))
                    {
                        strMsg = "供料Z轴危险";
                        return false;
                    }
                    break;

                case AXIS.下料仓移载X1轴:
                    double Pos2 = StationUnloadBinEx.POINT.顶升安全位.GetPos(AXIS.下料仓顶升Z1轴, "下料仓A站");

                    if (!(AXIS.下料仓顶升Z1轴.GetPos() - Pos2 <= 5 && AXIS.下料仓顶升Z1轴.GetPos() - Pos2 >= -5))
                    {
                        strMsg = "下料仓顶升Z1轴危险";
                        return false;
                    }
                    if (!DO.下料仓A上料前解锁伸出.Get() || !DO.下料仓A上料后解锁伸出.Get() || !DO.下料仓A下料前解锁伸出.Get() || !DO.下料仓A下料后解锁伸出.Get())
                    {
                        strMsg = "下料仓A解锁气缸必须要伸出".txtTrans(TransType.Trans);
                        return false;
                    }

                    Pos2 = StationTransferPick.POINT.下料仓A移载放料位.GetPos(AXIS.中转取放Z轴, "中转取放站");
                    double PosY = StationTransferPick.POINT.下料仓A移载放料位.GetPos(AXIS.中转取放Y轴, "中转取放站");
                    if ((Pos2 - AXIS.中转取放Z轴.GetPos() <= 10) && Math.Abs(PosY - AXIS.中转取放Y轴.GetPos()) <= 10)
                    {
                        strMsg = "中转Z轴危险";
                        return false;
                    }
                    break;
                case AXIS.下料仓移载X2轴:
                    double Pos3 = StationUnloadBinEx.POINT.顶升安全位.GetPos(AXIS.下料仓顶升Z2轴, "下料仓B站");

                    if (!(AXIS.下料仓顶升Z2轴.GetPos() - Pos3 <= 5 && AXIS.下料仓顶升Z2轴.GetPos() - Pos3 >= -5))
                    {
                        strMsg = "下料仓顶升Z2轴危险";
                        return false;
                    }
                    if (!DO.下料仓B上料前解锁伸出.Get() || !DO.下料仓B上料后解锁伸出.Get() || !DO.下料仓B下料前解锁伸出.Get() || !DO.下料仓B下料后解锁伸出.Get())
                    {
                        strMsg = "下料仓B解锁气缸必须要伸出".txtTrans(TransType.Trans);
                        return false;
                    }

                    Pos3 = StationTransferPick.POINT.下料仓B移载放料位.GetPos(AXIS.中转取放Z轴, "中转取放站");
                    PosY = StationTransferPick.POINT.下料仓B移载放料位.GetPos(AXIS.中转取放Y轴, "中转取放站");
                    if (Pos3 - AXIS.中转取放Z轴.GetPos() <= 10 && Math.Abs(PosY - AXIS.中转取放Y轴.GetPos()) <= 10)
                    {
                        strMsg = "中转Z轴危险";
                        return false;
                    }
                    break;

                case AXIS.下料仓移载X3轴:
                    double Pos4 = StationUnloadBinEx.POINT.顶升安全位.GetPos(AXIS.下料仓顶升Z3轴, "下料仓C站");
                    if (!(AXIS.下料仓顶升Z3轴.GetPos() - Pos4 <= 5 && AXIS.下料仓顶升Z3轴.GetPos() - Pos4 >= -5))
                    {
                        strMsg = "下料仓顶升Z3轴危险";
                        return false;
                    }
                    if (!DO.下料仓C上料前解锁伸出.Get() || !DO.下料仓C上料后解锁伸出.Get() || !DO.下料仓C下料前解锁伸出.Get() || !DO.下料仓C下料后解锁伸出.Get())
                    {
                        strMsg = "下料仓C解锁气缸必须要伸出".txtTrans(TransType.Trans);
                        return false;
                    }

                    Pos4 = StationTransferPick.POINT.下料仓C移载放料位.GetPos(AXIS.中转取放Z轴, "中转取放站");
                    PosY = StationTransferPick.POINT.下料仓C移载放料位.GetPos(AXIS.中转取放Y轴, "中转取放站");
                    if (Pos4 - AXIS.中转取放Z轴.GetPos() <= 10 && Math.Abs(PosY - AXIS.中转取放Y轴.GetPos()) <= 10)
                    {
                        strMsg = "中转Z轴危险";
                        return false;
                    }
                    break;

                case AXIS.下料取放X轴:
                    double SafePosZ1 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴1Z轴, "下料取放站");
                    double SafePosZ2 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴2Z轴, "下料取放站");
                    double SafePosZ3 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴3Z轴, "下料取放站");
                    double SafePosZ4 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴4Z轴, "下料取放站");

                    bool bIsZ1Safe = AXIS.下料取放吸嘴1Z轴.GetPos() - SafePosZ1 <= 5 && AXIS.下料取放吸嘴1Z轴.GetPos() - SafePosZ1 >= -5;
                    bool bIsZ2Safe = AXIS.下料取放吸嘴2Z轴.GetPos() - SafePosZ2 <= 5 && AXIS.下料取放吸嘴2Z轴.GetPos() - SafePosZ2 >= -5;
                    bool bIsZ3Safe = AXIS.下料取放吸嘴3Z轴.GetPos() - SafePosZ3 <= 5 && AXIS.下料取放吸嘴3Z轴.GetPos() - SafePosZ3 >= -5;
                    bool bIsZ4Safe = AXIS.下料取放吸嘴4Z轴.GetPos() - SafePosZ4 <= 5 && AXIS.下料取放吸嘴4Z轴.GetPos() - SafePosZ4 >= -5;

                    if (!bIsZ1Safe)
                    {
                        strMsg = "下料取放吸嘴1Z轴危险";
                        return false;
                    }
                    if (!bIsZ2Safe)
                    {
                        strMsg = "下料取放吸嘴2Z轴危险";
                        return false;
                    }
                    if (!bIsZ3Safe)
                    {
                        strMsg = "下料取放吸嘴3Z轴危险";
                        return false;
                    }
                    if (!bIsZ4Safe)
                    {
                        strMsg = "下料取放吸嘴4Z轴危险";
                        return false;
                    }
                    break;

                case AXIS.下料取放Y轴:
                    SafePosZ1 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴1Z轴, "下料取放站");
                    SafePosZ2 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴2Z轴, "下料取放站");
                    SafePosZ3 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴3Z轴, "下料取放站");
                    SafePosZ4 = StationUnloadGantryEx.POINT.安全位.GetPos(AXIS.下料取放吸嘴4Z轴, "下料取放站");

                    bIsZ1Safe = AXIS.下料取放吸嘴1Z轴.GetPos() - SafePosZ1 <= 5 && AXIS.下料取放吸嘴1Z轴.GetPos() - SafePosZ1 >= -5;
                    bIsZ2Safe = AXIS.下料取放吸嘴2Z轴.GetPos() - SafePosZ2 <= 5 && AXIS.下料取放吸嘴2Z轴.GetPos() - SafePosZ2 >= -5;
                    bIsZ3Safe = AXIS.下料取放吸嘴3Z轴.GetPos() - SafePosZ3 <= 5 && AXIS.下料取放吸嘴3Z轴.GetPos() - SafePosZ3 >= -5;
                    bIsZ4Safe = AXIS.下料取放吸嘴4Z轴.GetPos() - SafePosZ4 <= 5 && AXIS.下料取放吸嘴4Z轴.GetPos() - SafePosZ4 >= -5;

                    if (!bIsZ1Safe)
                    {
                        strMsg = "下料取放吸嘴1Z轴危险";
                        return false;
                    }
                    if (!bIsZ2Safe)
                    {
                        strMsg = "下料取放吸嘴2Z轴危险";
                        return false;
                    }
                    if (!bIsZ3Safe)
                    {
                        strMsg = "下料取放吸嘴3Z轴危险";
                        return false;
                    }
                    if (!bIsZ4Safe)
                    {
                        strMsg = "下料取放吸嘴4Z轴危险";
                        return false;
                    }
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// DO操作安全判定
        /// </summary>
        /// <param name="nIndex">DO序号</param>
        /// <param name="strMsg">危险操作信息</param>
        /// <returns></returns>
        public static bool IsDoSafe(int nIndex, ref string strMsg)
        {
            switch ((DO)nIndex)
            {
                //case DO.复检阻挡上:
                //case DO.复检阻挡下:
                //    break;
                //case DO.复检顶升上:
                //case DO.复检顶升下:
                //    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// 设置生产配方，能快按生产场景切参数
        /// </summary>
        public static void InitRecipe()
        {
            Parameter.AddRecipe("标准生产", () =>
            {
                //Parameter.SetBit("VisionEnable", true);
            });
            Parameter.AddRecipe("场景1", () =>
            {
            });
            Parameter.AddRecipe("场景2", () =>
            {
            });
        }

        /// <summary>
        /// 自定义回原模式
        /// </summary>
        public static void InitHomeMode()
        {
            //CardMgr.AddHomeMode("InoEcat", -1, "111-222", "音圈电机回原");
            CardMgr.AddHomeMode("InoEcat2Ex", 19, "YQ", "音圈电机回原");
        }

        /// <summary>
        /// PDCA初始化
        /// </summary>
        public static void InitPDCA()
        {
            //添加PDCA数据项
            PDCA.AddItem(new PdcaItem_attr("GlueCode"));
            PDCA.AddItem(new PdcaItem_attr("Version"));
            PDCA.AddItem(new PdcaItem_log_file("Administrator", "1"));
            PDCA.AddItem(new PdcaItem_dut_pos("A6"));
            PDCA.AddItem(new PdcaItem_attr("Station"));
            PDCA.AddItem(new PdcaItem_pdata("W1"));
            PDCA.AddItem(new PdcaItem_pdata("W2"));
            PDCA.AddItem(new PdcaItem_pdata("X1", "0.7", "5", "HZ"));
            PDCA.AddItem(new PdcaItem_pdata("X2", "0.7", "5", "HZ"));

            PDCA.Init(TCP_Client.PDCA.GetTcp(), "SEC_v1.0_CoilDispense");
        }

      
    }
}
