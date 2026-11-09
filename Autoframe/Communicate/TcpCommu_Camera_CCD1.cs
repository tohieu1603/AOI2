using AF.Communicate;
using AF.Station;
using Autoframe;
using System;
using System.Collections.Generic;
using AF.Common;
using AF.Data;
using System.Diagnostics;
using System.Threading; 

namespace Autoframe
{
    public class TcpCommu_Camera_CCD1 : TcpCommunicate
    {
        //通讯变量
        public double fX;
        public double fY;
        public double fU;

        /// <summary>
        /// 结果
        /// </summary>
        public bool result;

        /// <summary>
        /// 料盘检测拍照是否成功
        /// </summary>
        public bool resultHaveProductTakePhotoIsSuccess;

        public bool InitializeResult = false;
        public string trayTypeFromVision = string.Empty;

        /// <summary>
        /// 拍照料盘有无结果   0:无料，1有料
        /// </summary>
        public bool resultHaveProduct;
        public string[] ccdData = new string[Global.slotTotalCount];//视觉反馈数据


        // 命令枚举
        public enum Command
        {
            CCD1上料定位拍照,
            CCD1料盘有无,
           // 料盘型号,


            //GRR指令
            CCD1静态,
            CCD1动态,

        }



        public TcpClient TcpLink { set { this.tcplink = value; } }
 
        public TcpCommu_Camera_CCD1() : base()
        {
            //绑定网口
            this.tcplink = TcpClientMgr.GetClient(0);

        }
        /// <summary>
        /// 触发相机拍照
        /// </summary>
        /// <param name="nCmdIdx">命令索引</param>
        /// <param name="param">命令参数</param>
        /// <returns></returns>

        public void Camtrigger(object nCmdIdx, bool bEnable)
        {
            switch ((Command)nCmdIdx)
            {
     
                case Command.CCD1上料定位拍照:
                    DO.CCD1上料定位拍照.Set(bEnable);
                    break;
                case Command.CCD1静态:
                    DO.CCD1上料定位拍照.Set(bEnable);
                    break;
                case Command.CCD1动态:
                    DO.CCD1上料定位拍照.Set(bEnable);
                    break;
                    ;
            }
            if (bEnable)
            {
                Delay(100);//避免相机接收不到
                DO.CCD1上料定位拍照.Set(false);
                Delay(50);//避免相机接收不到
            }


        }
        /// <summary>
        /// 获取发送指令文本
        /// </summary>
        /// <param name="nCmdIdx">命令索引</param>
        /// <param name="param">命令参数</param>
        /// <returns></returns>
        public override string GetSendString(object nCmdIdx, params object[] param)
        {
            if (!(nCmdIdx is Command))
            {
                EventMgr.Error("参数类型错误".txtTrans(TransType.Mark));
                return "";
            }

            switch ((Command)nCmdIdx)
            {
                case Command.CCD1上料定位拍照:
                    if (param != null && param.Length >= 3)
                        //LCR，CCD1，ProcessID，X,Y,R，UserString;
                        this.CmdSend = $"LCR,CCD1,1,{param[0]},{param[1]},{param[2]},userString";
                    else
                        this.CmdSend = "LCR,CCD1,1,X,Y,U,";
                    break;
                case Command.CCD1料盘有无:
                    if (param != null && param.Length >= 3)
                        //LCR，CCD1，ProcessID，X,Y,R，UserString;
                        this.CmdSend = $"LCR,CCD1,10,userString";
                    else
                        this.CmdSend = $"LCR,CCD1,10,userString";
                    break;
                //case Command.料盘型号:
                //    if (param != null && param.Length >= 3)
                //        //LCR，CCD1，ProcessID，X,Y,R，UserString;
                //        this.CmdSend = $"Initialize,{param[0]}";
                //    else
                //        this.CmdSend = $"Initialize,{param[0]}";
                //    break;



                #region 相机动静态指令
                case Command.CCD1静态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Static,CCD1"; //
                    else
                        this.CmdSend = "Static,CCD1";
                    break;

                case Command.CCD1动态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Dynamic,CCD1"; //
                    else
                        this.CmdSend = "Dynamic,CCD1";
                    break;

                #endregion

                default:
                    this.CmdSend = "";
                    break;
            }

            return base.GetSendString(nCmdIdx, param); ;
        }

        
        /// <summary>
        /// 接受数据解析
        /// </summary>
        /// <param name="nCmdIdx">命令索引</param>
        public override bool AnalyzeRcvString(object nCmdIdx)
        {
            if (!(nCmdIdx is Command))
            {
                EventMgr.Error("参数类型错误".txtTrans(TransType.Mark));
                return false;
            }

            switch ((Command)nCmdIdx)
            {
                case Command.CCD1上料定位拍照:
                    result = GetRcvInt(2) == 1;
                    if (result)
                    {
                        if (!string.IsNullOrWhiteSpace(GetRcvString(3)))
                        {
                            string[] visionSlotResult = GetRcvString(3).Split('_');
                            if (visionSlotResult != null)
                            {
                                ccdData = visionSlotResult;
                            }
                        }
                    }
                   
                    break;
                case Command.CCD1静态:
                case Command.CCD1动态:
                    result = GetRcvString(1) == "1";

                    //for (int i = 0; i < Global.slotTotalCount; i++)
                    //{
                    //    ushort index = ((ushort)(3 + i));
                    //    ccdData[i] = (GetRcvInt(index) == 1 ? "OK" : "NA");  
                    //}
                    break;
                case Command.CCD1料盘有无:
                    resultHaveProductTakePhotoIsSuccess = GetRcvString(2) == "1";
                    result= GetRcvString(2) == "1";
                    if (resultHaveProductTakePhotoIsSuccess)
                    {
                        resultHaveProduct = GetRcvString(3) == "1";
                    }
                    else
                    {
                        resultHaveProduct = false;
                    }
                    break;
                //case Command.料盘型号:
                //    InitializeResult = GetRcvString(1) == "OK";
                //    if (InitializeResult)
                //    {
                //        trayTypeFromVision = GetRcvString(2);
                //    }
                //    else
                //    {
                //        trayTypeFromVision = string.Empty;
                //    }
                //    break;
            }
            return true;
        }

        //public override List<string[]> GetCommandList()
        //{
        //    List<string[]> ls = new List<string[]>();
        //    string strName, strCmd;
        //    foreach (var item in Enum.GetValues(typeof(Command)))
        //    {
        //        strName = item.ToString();
        //        strCmd = GetSendString(item);
        //        ls.Add(new string[] { strName, strCmd });
        //    }
        //    return ls;
        //}
    }
}