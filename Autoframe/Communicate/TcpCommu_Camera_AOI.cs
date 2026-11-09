using AF.Communicate;
using AF.Station;
using Autoframe;
using System;
using System.Collections.Generic;
using AF.Common;
using AF.Data;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace Autoframe
{
    public class TcpCommu_Camera_AOI : TcpCommunicate
    {
        //通讯变量
        public double fX;
        public double fY;
        public double fU;

        /// <summary>
        /// 结果
        /// </summary>

        public int CCD2Cavity;
        public int CCD3Cavity;
        public int CCD4Cavity;

        public bool CCD2result;
        public bool CCD3result;
        public bool CCD4result;

        public bool CCD234ResetRet;

        /// <summary>
        /// 数据
        /// </summary>

        public string[] ccd2Data = new string[Global.slotTotalCount];
        public string[] ccd3Data = new string[Global.slotTotalCount];
        public string[] ccd4Data = new string[Global.slotTotalCount];


        public string NgReason;

        // 命令枚举
        public enum Command
        {

            CCD2下AOI检测拍照,
            CCD3上AOI检测拍照,
            CCD4下AOI检测拍照,

            CCD2Reset,
            CCD3Reset,
            CCD4Reset,

            //动静态
            CCD2静态,
            CCD3静态,
            CCD4静态,

            CCD2动态,
            CCD3动态,
            CCD4动态,

            CCD2获取拍照结果,
            CCD3获取拍照结果,
            CCD4获取拍照结果,
        

        }



        public TcpClient TcpLink { set { this.tcplink = value; } }
        
        public TcpCommu_Camera_AOI() : base()
        {
            //绑定网口
            //this.tcplink = TcpClientMgr.GetClient(1);
        }
        /// <summary>
        /// 触发相机拍照
        /// </summary>
        /// <param name="nCmdIdx">命令索引</param>
        /// <param name="param">命令参数</param>
        /// <param name="bEnable">打开关闭</param>
        /// <returns></returns> b

        public void Camtrigger(object nCmdIdx, bool bEnable)
        {
            if(bEnable)
            {
                switch ((Command)nCmdIdx)
                {

                    case Command.CCD2下AOI检测拍照:

                        DO.CCD2下AOI检测拍照.Set(true);

                        break;
                    case Command.CCD3上AOI检测拍照:

                        DO.CCD3上AOI检测拍照.Set(true);
                        break;
                    case Command.CCD4下AOI检测拍照:

                        DO.CCD4下AOI检测拍照.Set(true);
                        break;

                }

                Delay(20);//避免相机接收不到
            }
            else
            {
                switch ((Command)nCmdIdx)
                {

                    case Command.CCD2下AOI检测拍照:
                        DO.CCD2下AOI检测拍照.Set(false);
                       

                        break;
                    case Command.CCD3上AOI检测拍照:
                        DO.CCD3上AOI检测拍照.Set(false);
                       
                        break;
                    case Command.CCD4下AOI检测拍照:
                        DO.CCD4下AOI检测拍照.Set(false);
                  
                        break;

                }
                Delay(10);

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

                case Command.CCD2下AOI检测拍照:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"T2,{param[0]},{param[1]},{param[2]}"; //AOI，CCD2，ProcessID,ProductID,Barcode
                    else
                        this.CmdSend = "T2,n,SN";
                    break;

                case Command.CCD3上AOI检测拍照:
                    if (param != null && param.Length >= 1)

                        this.CmdSend = $"T3,{param[0]},{param[1]},{param[2]}"; //AOI，CCD3，ProcessID,ProductID,Barcode;
                    else
                        this.CmdSend = "T3,n,SN";
                    break;

                case Command.CCD4下AOI检测拍照:
                    if (param != null && param.Length >= 2)

                        this.CmdSend = $"T4,{param[0]},{param[1]},{param[2]}";
                    else
                        this.CmdSend = "T4,n,SN";
                    break;
                case Command.CCD2Reset:
                case Command.CCD3Reset:
                case Command.CCD4Reset:
                    if (param != null)

                        this.CmdSend = $"RST";
                    else
                        this.CmdSend = "RST";
                    break;
                case Command.CCD2获取拍照结果:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"SR,{param[0]}"; //SR,id
                    else
                        this.CmdSend = "SR,id";
                    break;
                case Command.CCD3获取拍照结果:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"SR,{param[0]}"; //SR,id
                    else
                        this.CmdSend = "SR,id";
                    break;
                case Command.CCD4获取拍照结果:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"SR,{param[0]}"; //SR,id
                    else
                        this.CmdSend = "SR,id";
                    break;
                
                #region 相机动静态指令
                case Command.CCD2静态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Static,CCD2"; //
                    else
                        this.CmdSend = "Static,CCD2";
                    break;
                case Command.CCD3静态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Static,CCD3"; //
                    else
                        this.CmdSend = "Static,CCD3";
                    break;
                case Command.CCD4静态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Static,CCD4"; //
                    else
                        this.CmdSend = "Static,CCD4";
                    break;
                
                case Command.CCD2动态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Dynamic,CCD2"; //
                    else
                        this.CmdSend = "Dynamic,CCD2";
                    break;
                case Command.CCD3动态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Dynamic,CCD3"; //
                    else
                        this.CmdSend = "Dynamic,CCD3";
                    break;
                case Command.CCD4动态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Dynamic,CCD4"; //
                    else
                        this.CmdSend = "Dynamic,CCD4";
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

                case Command.CCD2下AOI检测拍照:
                    CCD2result = GetRcvString(3) == "OK";
                   // CCD2Cavity = GetRcvInt(2);
                    if (!CCD2result)
                    {
                        NgReason =  GetRcvString(4);
                    }
                    break;
                case Command.CCD2获取拍照结果:
                    CCD2result = GetRcvString(1) == "1";
                    break;
                case Command.CCD3上AOI检测拍照:
                    CCD3result = GetRcvString(3) == "OK";
                   // CCD3Cavity = GetRcvInt(2);
                    if (!CCD3result)
                    {
                        NgReason = GetRcvString(4);
                    }
                    break;
                case Command.CCD3获取拍照结果:
                    CCD3result = GetRcvString(1) == "1";
                    break;
                case Command.CCD4下AOI检测拍照:
                    CCD4result = GetRcvString(3) == "OK";
                   // CCD4Cavity = GetRcvInt(2);
                    if (!CCD4result)
                    {
                        NgReason = GetRcvString(4);
                    }
                    break;
                case Command.CCD2Reset:
                case Command.CCD3Reset:
                case Command.CCD4Reset:
                    if (GetRcvString(1) == "OK")
                        return true;
                    else
                        return false;
                case Command.CCD4获取拍照结果:
                    CCD3result = GetRcvString(1) == "1";
                    break;

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

       public bool WaitReply1(object nCmdIdx, string startWith)
        {
            string strRet = "";
            while (true)
            {            
                tcplink.ReadLine(out strRet);

                if (startWith.Length > 0 && strRet.StartsWith(startWith))
                {
                    break;
                }
            }

            strRcv = strRet.Split(',');

            return AnalyzeRcvString(nCmdIdx);
        }

     
    }
}