using AF.Card;
using AF.Common;
using AF.Data;
using AF.Station;
using Autoframe;
using Microsoft.VisualBasic.Logging;
using OxyPlot.Axes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using AF.CardEx;

namespace Autoframe
{
    /// <summary>
    /// 上料仓站
    /// </summary>
    public class StationLoading : StationEx
    {
        int index;
        int IOport;
        string msg = "";
        int dryRunCount=0;
        int dryRunBin = 0;

        AxisHandle axisHandle = CardMgr.GetAxis(((int)AXIS.料盘供料Z轴));
         
        /// <summary>
        /// 构造函数，需要设置站位当前的IO输入，IO输出，轴方向及轴名称，以显示在手动页面方便操作
        /// </summary>
        /// <param name="strName"></param>
        public StationLoading(string strName) : base(strName)
        {
            HardwareConfig();
            CheckPoint<POINT>();
            LoadContainer<CONTAINER, ProductData>();
        }

        void HardwareConfig()
        {
            //配置站位界面显示输入
            arrayDI = new object[] { DI.上料仓启动, DI.上料仓安全光栅, DI.上料站供料料盘有无, DI.上料站料仓1料盘到位,DI.上料站料仓1料盘有无,DI.上料站料仓2料盘到位,DI.上料站料仓2料盘有无};
            //配置站位界面显示输出
            arrayDO = new object[] { DO.上料仓启动灯 };
            //配置站位界面显示气缸
            arrayCylinder = new object[] {  };
            //配置站位轴
            arrayAxis = new object[] { AXIS.料盘移载Y轴, AXIS.料盘供料Z轴 };
        }

        ProductData productData = new ProductData();

        TcpCommu_Camera camera = new TcpCommu_Camera();
        /// <summary>
        /// 站位内的点位枚举
        /// </summary>
        public enum POINT
        {
            移载上料位,
            料仓1供料位,
            料仓2供料位,
            顶升位,
            安全位
        }

        /// <summary>
        /// 产品穴位
        /// </summary>
        public enum CONTAINER
        {
            穴位1,
            穴位2,
        }

        public override void OnStop()
        {
            //base.OnStop() 不能删，且需要放在第一行。
            base.OnStop();
            AXIS.料盘移载Y轴.ServoOn(false);
            AXIS.料盘供料Z轴.ServoOn(false);
        }
        public override void OnPause()
        {
            //base.OnPause() 不能删，且需要放在第一行。
            base.OnPause();
        }
        public override void OnResume()
        {
            //base.OnResume() 不能删，且需要放在第一行。
            // return base.OnResume();
        }

        /// <summary>
        /// 初始化之前调用
        /// 1.设置初始化流程和自动流程的初始步
        /// 2.复位寄存器
        /// </summary>
        public override void Preinit()

        /// <summary>
        /// 为不同的流程设置起始步
        /// </summary>

        {
            switch (Process)
            {
                case AutoProcess.正常生产:
                case AutoProcess.空跑:
                case AutoProcess.相机标定:
                    //默认流程入口 是 StepEntry
                    break;
                default:
                    this.InitEntry = StepExit;
                    this.AutoEntry = StepEmpty;
                    break;
            }

            RegBit.上料仓站初始化完成.Set(false);
            RegBit.上料取放站初始化完成.Set(false);
            RegBit.上料仓站初始化可放料.Set(false);
            RegBit.上料取放站初始化安全.Set(false);
            RegBit.上料取放站初始化放料完成.Set(false);

            RegBit.上料取放站可取料.Set(false);
            RegBit.上料取放站取料完成.Set(false);

            DO.启动灯.Set(false);
        }

        protected override string InitRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    
                    return "轴上使能";

                case "轴上使能":
                    AXIS.料盘移载Y轴.ServoOn(true);
                    AXIS.料盘供料Z轴.ServoOn(true);
                    Delay(1000);
                    return "等待上料取放站初始化安全";


                case "等待上料取放站初始化安全":
                    if (RegBit.上料取放站初始化安全.Get())
                    {
                        RegBit.上料取放站初始化安全.Set(false);
                        return "供料Z轴回安全位";
                    }
                    else
                    {
                        return "等待上料取放站初始化安全";
                    }
                   

                case "供料Z轴回安全位":
                    if (DI.上料站供料料盘有无.Get())
                    {
                        AxisSingleTo(AXIS.料盘供料Z轴, POINT.安全位, 20);
                    }
                    else
                    {
                        AxisSingleTo(AXIS.料盘供料Z轴, POINT.安全位);
                    }
                   
                    WaitAxis(AXIS.料盘供料Z轴);
                    return "获取料仓1是否有料盘";

                case "获取料仓1是否有料盘":
                    if (!IsDryRun)
                    {
                        //夹爪上有料
                        if (!DI.上料站供料料盘有无.Get() && DI.取料站夹料盘夹紧位.Get())
                        {
                            ShowMessage("料仓1没有料盘，放个料盘?", "获取料仓1是否有料盘", new string[] { "已处理" });
                        }
                    }
                    return "移载到供料位";

                case "移载到供料位":
                    AxisSingleTo(AXIS.料盘移载Y轴,POINT.料仓1供料位);
                    WaitAxis(AXIS.料盘移载Y轴);
                    index = 1;
                    IOport = CardMgr.DI[(int)(DI.上料站料仓1料盘到位)].nIndex;
                    //CardMgr.GetDiHandle(DI.上料站料仓1料盘到位).
                    return "供料Z轴快速到顶升位";

                case "供料Z轴快速到顶升位":

                    ((Card_InoEcat2Ex)CardMgr.Card[0]).DestroyEventDiMotion(index);//清除事件

                    ((Card_InoEcat2Ex)CardMgr.Card[0]).SetDiMotionEvent(index, IOport, ((int)AXIS.料盘供料Z轴), 0, ((double)POINT.顶升位) * axisHandle.config.dbGearRatio, axisHandle.config.dbSpeedMax * AxisHandle.SpeedPercent / 100, axisHandle.config.dbAcc, axisHandle.config.dbDec);//设置事件

                    AxisSingleTo(AXIS.料盘供料Z轴,POINT.顶升位 ,30);//用事件中断停止

                    return "等待供料Z轴停止";


                case "等待供料Z轴停止":

                    DI sensor = index == 1 ? DI.上料站料仓1料盘到位 : DI.上料站料仓2料盘到位;

                    DI sensor2 =  DI.上料站供料料盘有无;
                    //上料站供料料盘有无,没有信号的话，，上料站料仓1料盘到位或DI.上料站料仓2料盘到位 感应不到
                    if (sensor2.Get())
                    {
                        if (AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop && (sensor.Get() || IsDryRun))
                        {
                            return "供料Z轴下降固定距离";
                        }
                        else
                        {
                            return "等待供料Z轴停止";
                        }
                    }
                    else
                    {
                        if (AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop)
                        {
                            return "供料Z轴下降固定距离";
                        }
                        else
                        {
                            return "等待供料Z轴停止";
                        }
                    }


                case "供料Z轴下降固定距离":

                    double dis = Parameter.GetDouble("FeedZ_MoveDownFixedDistance");
                    AxisSingleTo(AXIS.料盘供料Z轴, AXIS.料盘供料Z轴.GetPos() - dis);
                    WaitAxis(AXIS.料盘供料Z轴);

                    return "供料Z轴慢速到顶升位";

                case "供料Z轴慢速到顶升位":
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).DestroyEventDiMotion(index);//清除事件
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).SetDiMotionEvent(index, IOport, ((int)AXIS.料盘供料Z轴), 0, ((double)POINT.顶升位) * axisHandle.config.dbGearRatio, axisHandle.config.dbSpeedMax * AxisHandle.SpeedPercent / 100, axisHandle.config.dbAcc, axisHandle.config.dbDec);//设置事件

                    //AxisSingleTo(POINT.顶升位, AXIS.料盘供料Z轴, 10);//用事件中断停止

                    AXIS.料盘供料Z轴.AbsMove(POINT.顶升位.GetPos(AXIS.料盘供料Z轴, this), Parameter.GetDouble("FeedZ_LowSpeed"));

                    return "等待Z轴停止";

                case "等待Z轴停止":

                    DI sensor1 = index == 1 ? DI.上料站料仓1料盘到位 : DI.上料站料仓2料盘到位;
                    DI sensor3 = DI.上料站供料料盘有无;
                    //上料站供料料盘有无,没有信号的话，，上料站料仓1料盘到位或DI.上料站料仓2料盘到位 感应不到
                    if (sensor3.Get())
                    {
                        if (AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop && (sensor1.Get() || IsDryRun))
                        {
                            return "通知上料取放站可放料";
                        }
                        else
                        {
                            return "等待Z轴停止";
                        }
                    }
                    else
                    {
                        if (AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop)
                        {
                            return "通知上料取放站可放料";
                        }
                        else
                        {
                            return "等待Z轴停止";
                        }
                    }
             

                case "通知上料取放站可放料":
                     
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).DestroyEventDiMotion(index);//清除事件
                    RegBit.上料仓站初始化可放料.Set(true);
                    return "等待上料取放站初始化放料完成";

                case "等待上料取放站初始化放料完成":
                    if (RegBit.上料取放站初始化放料完成.Get())
                    {
                        RegBit.上料取放站初始化放料完成.Set(false);
                        return "Z轴到安全位";
                    }
                    else
                    {
                        return "等待上料取放站初始化放料完成";
                    }
                case "Z轴到安全位":
                    if (DI.上料站供料料盘有无.Get())
                    {
                        AxisSingleTo(AXIS.料盘供料Z轴, POINT.安全位, 20);
                    }
                    else
                    {
                        AxisSingleTo(AXIS.料盘供料Z轴, POINT.安全位);
                    }
                       
                    WaitAxis(AXIS.料盘供料Z轴);
                    return "移载到OP上料位";

                case "移载到OP上料位":
                    AxisSingleTo(AXIS.料盘移载Y轴,POINT.移载上料位 );
                    WaitAxis(AXIS.料盘移载Y轴);
                    return "料仓启动灯关";

                case "料仓启动灯关":
                    DO.上料仓启动灯.Set(false);
                    return "初始化完成";

                case "初始化完成":
                    RegBit.上料仓站初始化完成.Set(true);
                    //WaitAllStaReady();
                    //结束初始化
                    dryRunBin = 0;
                    return StepExit;
                default:
                    return null;
            }
        }

        protected override string ProduceRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
					return "料仓启动灯开";

                case "料仓启动灯开":
                    DO.上料仓启动灯.Set(true);
                    return "提示OP上料";

                case "提示OP上料":
                    Logger.Process(Name, "上料仓请上料后按料仓启动按钮");
                    return "等待料仓启动";

                case "等待料仓启动":
                    if (!IsDryRun)//正常跑
                    {
                        WaitDI(DI.上料仓启动, true, -1);
                    } 
                    return "料仓启动灯关";
                case "料仓启动灯关":
                    DO.上料仓启动灯.Set(false);

                    return "判断料仓是否有料";

                case "判断料仓是否有料":
                    if (!IsDryRun)//正常跑
                    {
                        if (DI.上料站料仓1料盘有无.Get()||DI.上料站料仓2料盘有无.Get())
                        {
                            if (DI.上料站料仓1料盘有无.Get() && DI.上料站料仓1料盘到位.Get())
                            {
                                ShowMessage("料仓1料盘数量太多，请减少到位感应器之下");
                                return "等待料仓启动";
                            }
                            else if (DI.上料站料仓2料盘有无.Get() && DI.上料站料仓2料盘到位.Get())
                            {
                                ShowMessage("料仓2料盘数量太多，请减少到位感应器之下");
                                return "等待料仓启动";
                            }
                            return DI.上料站料仓1料盘有无.Get() ? "料仓1移动至供料位" : "料仓2移动至供料位";
                        }
                        else
                        {
                            return "料仓启动灯开";
                        }
                    }
                    else
                    {
                        if (DI.上料站料仓1料盘有无.Get() && DI.上料站料仓1料盘到位.Get())
                        {
                            ShowMessage("料仓1料盘数量太多，请减少到位感应器之下");
                        }
                        else if (DI.上料站料仓2料盘有无.Get() && DI.上料站料仓2料盘到位.Get())
                        {
                            ShowMessage("料仓2料盘数量太多，请减少到位感应器之下");
                        }
                        if (dryRunBin == 0)
                        {
                            return "料仓1移动至供料位";
                        }
                        else
                        {
                            return "料仓2移动至供料位";
                        }
                    }

                case "料仓1移动至供料位":
                    index = 1;
                    IOport = CardMgr.DI[(int)(DI.上料站料仓1料盘到位)].nIndex;
                    //if (AXIS.料盘供料Z轴.GetPos() - ((double)POINT.安全位) <= 5 && AXIS.料盘供料Z轴.GetPos() - ((double)POINT.安全位) >= -5)
                    if (SystemInit.IsAxisSafe(((int)AXIS.料盘移载Y轴), null, ref msg))
                    {
                        AxisSingleTo(AXIS.料盘移载Y轴,POINT.料仓1供料位 );
                        WaitAxis(AXIS.料盘移载Y轴);
                        return "供料Z轴快速到顶升位";
                    }
                    {
                        //MessageBox.Show("供料Z轴危险");
                        ShowMessage($"{msg}", null, new string[] { "再次动作" });
                        return "料仓1移动至供料位";
                    }


                case "料仓2移动至供料位":
                    index = 2;
                    IOport = CardMgr.DI[(int)(DI.上料站料仓2料盘到位)].nIndex;
                    //if (AXIS.料盘供料Z轴.GetPos() - ((double)POINT.安全位) <= 5 && AXIS.料盘供料Z轴.GetPos() - ((double)POINT.安全位) >= -5)
                    if (SystemInit.IsAxisSafe(((int)AXIS.料盘移载Y轴), null, ref msg))
                    {
                        AxisSingleTo(AXIS.料盘移载Y轴,POINT.料仓2供料位 );
                        WaitAxis(AXIS.料盘移载Y轴);
                        return "供料Z轴快速到顶升位";
                    }
                    {
                        //MessageBox.Show("供料Z轴危险");
                        ShowMessage($"{msg}", null, new string[] { "再次动作" });
                        return "料仓2移动至供料位";
                    }
                    

                case "供料Z轴快速到顶升位":
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).DestroyEventDiMotion(index);//清除事件
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).SetDiMotionEvent(index, IOport,((int)AXIS.料盘供料Z轴),0, ((double)POINT.顶升位)* axisHandle.config.dbGearRatio, axisHandle.config.dbSpeedMax  * AxisHandle.SpeedPercent / 100, axisHandle.config.dbAcc, axisHandle.config.dbDec);//设置事件

                    AxisSingleTo(AXIS.料盘供料Z轴, POINT.顶升位, 30);//用事件中断停止
                    return "等待供料Z轴停止";


                case "等待供料Z轴停止":

                    DI sensor = index == 1 ? DI.上料站料仓1料盘到位 : DI.上料站料仓2料盘到位;
                    bool axisIsStop = AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop;
                    bool sensorGet = sensor.Get();//sensor 接常开啊
                    if (AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop && (sensorGet || IsDryRun))
                    {
                        return "供料Z轴下降固定距离";
                    }
                    else
                    {
                        return "等待供料Z轴停止";
                    }


                case "供料Z轴下降固定距离":
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).DestroyEventDiMotion(index);//清除事件
                    AxisSingleTo(AXIS.料盘供料Z轴, AXIS.料盘供料Z轴.GetPos() - Parameter.GetDouble("FeedZ_MoveDownFixedDistance"),30);
                    WaitAxis(AXIS.料盘供料Z轴);

                    return "供料Z轴慢速到顶升位";

                case "供料Z轴慢速到顶升位":
                   
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).SetDiMotionEvent(index, IOport, ((int)AXIS.料盘供料Z轴), 0, ((double)POINT.顶升位) * axisHandle.config.dbGearRatio, axisHandle.config.dbSpeedMax * AxisHandle.SpeedPercent / 500, axisHandle.config.dbAcc, axisHandle.config.dbDec);//设置事件

                    //AxisSingleTo(POINT.顶升位, AXIS.料盘供料Z轴,10.0);//用事件中断停止
     
                     AXIS.料盘供料Z轴.AbsMove(POINT.顶升位.GetPos(AXIS.料盘供料Z轴, this), Parameter.GetDouble("FeedZ_LowSpeed"));//

                    return "等待Z轴停止";

                case "等待Z轴停止":

                    DI sensor1 = index == 1 ? DI.上料站料仓1料盘到位 : DI.上料站料仓2料盘到位;
                    bool axisIsStop1 = AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop;
                    bool sensorGet1 = sensor1.Get();//sensor 接常开啊
                    if (AXIS.料盘供料Z轴.IsStop() == AxisHandle.StopType.NormalStop && (sensorGet1 || IsDryRun))
                    {
                       double axisZPosition = AXIS.料盘供料Z轴.GetPos();
                        Logger.Process(this.Name, $"Z轴位置{axisZPosition}");
                        return "通知上料取放站可取料";
                    }
                    else
                    {
                        return "等待Z轴停止";
                    }

                case "通知上料取放站可取料":
                    ((Card_InoEcat2Ex)CardMgr.Card[0]).DestroyEventDiMotion(index);//清除事件
                    RegBit.上料取放站可取料.Set(true);
                    return "等待上料取放站取料完成";

                case "等待上料取放站取料完成":
                    if (RegBit.上料取放站取料完成.Get())
                    {
                        RegBit.上料取放站取料完成.Set(false);
                        return "供料料盘有无判断";
                    }
                    else
                    {
                        return "等待上料取放站取料完成";
                    }
                case "供料料盘有无判断":
                    if (!IsDryRun)
                    {
                        if (DI.上料站供料料盘有无.Get() )
                        {
                            return "供料Z轴快速到顶升位";
                        }
                        else
                        {
                            return "供料Z到安全位";
                        }
                    }
                    else
                    {
                        if (dryRunCount<2)
                        {
                            dryRunCount++;
                            return "供料Z轴快速到顶升位";
                        }
                        else
                        {
                            if (dryRunBin == 0)
                            {
                                dryRunBin = 1;
                            }
                            else
                            {
                                dryRunBin = 0;
                            }
                            dryRunCount = 0;
                            return "供料Z到安全位";
                        }
                    }
            
   
                case "供料Z到安全位":
                    AxisSingleTo(AXIS.料盘供料Z轴,POINT.安全位 );
                    WaitAxis(AXIS.料盘供料Z轴);
                    return "料仓有料检测";
                case "料仓有料检测":
                    if (!IsDryRun)//正常跑
                    {
                        if (DI.上料站料仓1料盘有无.Get() || DI.上料站料仓2料盘有无.Get())
                        {
                            return DI.上料站料仓1料盘有无.Get() ? "料仓1移动至供料位" : "料仓2移动至供料位";
                        }
                        else
                        {
                            return "料仓移动到移载上料位";
                        }
                    }
                    else
                    {
                        if (dryRunBin == 0)
                        {
                            return "料仓1移动至供料位";
                        }
                        else
                        {
                            return "料仓2移动至供料位";
                        }

                    }


                case "料仓移动到移载上料位":

                    //if (AXIS.料盘供料Z轴.GetPos() - ((double)POINT.安全位) <= 5 && AXIS.料盘供料Z轴.GetPos() - ((double)POINT.安全位) >= -5)
                    if (SystemInit.IsAxisSafe(((int)AXIS.料盘移载Y轴),null,ref msg))
                    {
                        AxisSingleTo(AXIS.料盘移载Y轴,POINT.移载上料位);
                        WaitAxis(AXIS.料盘移载Y轴);
                        return "生产流程结束";
                    }
                    else
                    {
                        //MessageBox.Show("供料Z轴危险");
                        ShowMessage($"{msg}", null, new string[] { "再次动作" });
                        return "料仓移动到移载上料位";
                    }

                case "生产流程结束":
                    return StepEntry;
                default:
                    return null;
            }

        }

        protected override string CalibRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    Delay(10);
                    return "标定";
                default:
                    return null;
            }
        }

        protected override string GrrRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    return "GRR";
                default:
                    return null;
            }
        }

        protected override string OtherRun(string strStep)
        {
            switch (strStep)
            {
                case StepEntry:
                    return "Other";

                default:
                    return null;
            }
        }
    }
    public static partial class HandleEx
    {
        public static double GetPos(this StationLoading.POINT pt, AXIS axis, StationBase station)
        {
            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }

        public static double GetPos(this StationLoading.POINT pt, AXIS axis, string strStation)
        {
            StationBase station = StationMgr.GetStation(strStation);
            if (station == null)
                return 0;

            return station.dictPoint[(int)pt][$"a{(int)axis}"];
        }
    }
}
