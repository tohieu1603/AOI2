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
    public class TcpCommu_Camera : TcpCommunicate
    {
        //通讯变量
        public double ccd5X;
        public double ccd5Y;
        public double ccd5Z;

        public double ccd6X;
        public double ccd6Y;
        public double ccd6Z;


        /// <summary>
        /// 结果
        /// </summary>
        public bool CCD5result;
        public bool CCD6result;
      

        public bool CCD5result1;
        public bool CCD5result2;
        public bool CCD6result1;
        public bool CCD6result2;
        public bool CCD7result;
        public string CCD7CavityRet;

        public bool CCD7CheckTrayRet;
        public bool CCD7HaveProduct;//判断穴位里是否有物料

        public bool CCD7GetResultPick;
        public bool CCD7GetResultPlaceCCD5;
        public bool CCD7GetResultPlaceCCD6;

        public bool NozzelHaveProductResult;//判断吸嘴是否有料拍照是否成功

        public bool NozzelNG1HaveProduct;//吸嘴1 是否有料
        public bool NozzelNG2HaveProduct;//吸嘴2 是否有料

        public bool NozzelOK1HaveProduct;//吸嘴3 是否有料
        public bool NozzelOK2HaveProduct;//吸嘴4 是否有料


        /// <summary>
        /// ccd5 9点标定结果
        /// </summary>
        public bool CCD5CalibResult;

        /// <summary>
        /// ccd6 9点标定结果
        /// </summary>
        public bool CCD6CalibResult;
       

        /// <summary>
        ///CCD5注册模版结果
        /// </summary>
        public bool CCD5RegisterTemplateResult;

        /// <summary>
        ///CCD6注册模版结果
        /// </summary>
        public bool CCD6RegisterTemplateResult;

        public bool CCD7RelativeToCCD5OrCCD6Result;

        public double[] dbCCD7GetPickResultNG1;
        public double[] dbCCD7GetPickResultNG2;
        public double[] dbCCD7GetPickResultOK1;
        public double[] dbCCD7GetPickResultOK2;

        public double[] dbCCD7GetPlaceResultCCD5OK1;
        public double[] dbCCD7GetPlaceResultCCD5OK2;
        public double[] dbCCD7GetPlaceResultCCD5NG1;
        public double[] dbCCD7GetPlaceResultCCD5NG2;


        public double[] dbCCD7GetPlaceResultCCD6OK1;
        public double[] dbCCD7GetPlaceResultCCD6OK2;
        public double[] dbCCD7GetPlaceResultCCD6NG1;
        public double[] dbCCD7GetPlaceResultCCD6NG2;

        public string[] strCCD7GetProductExistA;//初始化的时候判断穴位是否为空
        public string[] strCCD7GetProductExistB;
        public string[] strCCD7GetProductExistC;

        public bool TrayAIsExsit;//判断A区是否有料盘
        public bool TrayBIsExsit;//判断B区是否有料盘
        public bool TrayCIsExsit;//判断C区是否有料盘

        // 命令枚举
        public enum Command
        {
            CCD5下料定位拍照,
            CCD6下料定位拍照,
            CCD7料盘定位拍照,
            CCD7穴位内物料有无,
            CCD7CheckHaveTray,

            CCD5吸嘴物料有无,

            CCD5静态,
            CCD6静态,
            CCD7静态,

            CCD5动态,
            CCD6动态,
            CCD7动态,
            CCD7索要料盘取料结果,
            CCD7索要料盘放料结果对应CCD5,
            CCD7索要料盘放料结果对应CCD6,


            九点标定,
            吸嘴旋转标定,
            CCD5注册吸嘴模板,
            CCD6注册吸嘴模板,
            CCD7与CCD5或CCD6关联,

            NG吸嘴是否有料,
            OK吸嘴是否有料,
            CCD5拍菲林片,
            CCD6拍菲林片,
            料盘型号
        }
        public TcpClient TcpLink { set { this.tcplink = value; } }
        private object m_lock = new object();

        public bool InitializeResult = false;
        public string trayTypeFromVision = string.Empty;

        public TcpCommu_Camera() : base()
        {
            //绑定网口
            this.tcplink = TcpClientMgr.GetClient(0);
            dbCCD7GetPickResultOK1 = new double[3];
            dbCCD7GetPickResultNG1 = new double[3];
            dbCCD7GetPickResultOK2 = new double[3];
            dbCCD7GetPickResultNG2 = new double[3];

            dbCCD7GetPlaceResultCCD5OK1 = new double[3];
            dbCCD7GetPlaceResultCCD5NG1 = new double[3];
            dbCCD7GetPlaceResultCCD5OK2 = new double[3];
            dbCCD7GetPlaceResultCCD5NG2 = new double[3];

            dbCCD7GetPlaceResultCCD6OK1 = new double[3];
            dbCCD7GetPlaceResultCCD6NG1 = new double[3];
            dbCCD7GetPlaceResultCCD6OK2 = new double[3];
            dbCCD7GetPlaceResultCCD6NG2 = new double[3];

            strCCD7GetProductExistA = new string[Global.slotTotalCount];
            strCCD7GetProductExistB = new string[Global.slotTotalCount];
            strCCD7GetProductExistC = new string[Global.slotTotalCount];
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
                case Command.CCD5下料定位拍照:
                    DO.CCD5下料定位拍照.Set(bEnable);
                    break;
                case Command.CCD6下料定位拍照:
                    DO.CCD6下料定位拍照.Set(bEnable);
                    break;
                case Command.CCD7料盘定位拍照:
                    DO.CCD7料盘定位拍照.Set(bEnable);
                    break;
            }
            if (bEnable)
                Thread.Sleep(20);//避免相机接收不到
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
                case Command.CCD5下料定位拍照:
                    if (param != null && param.Length >= 2)
                        //PosID1  PosID2,x,y,z
                        this.CmdSend = $"ATP,CCD5,4,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]},{param[6]},{param[7]},userstring";
                    else
                        this.CmdSend = "ATP,CCD4,4,ProductID,Barcode,";
                    break;
                case Command.CCD6下料定位拍照:
                    if (param != null && param.Length >= 2)
                        //PosID1  PosID2,x,y,z
                        this.CmdSend = $"ATP,CCD6,5,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]},{param[6]},{param[7]},userstring";
                    else
                        this.CmdSend = "ATP,CCD6,5,ProductID,Barcode,";
                    break;


                case Command.CCD7料盘定位拍照:
                    //ATP，CCD7，ProcessID，PosID,Times,X,Y,R，UserString
                    if (param != null && param.Length >= 2)
                        //PosID,Times,X,Y
                        this.CmdSend = $"ATP,CCD7,6,{param[0]},{param[1]},{param[2]},{param[3]},0,userString";
                    else
                        this.CmdSend = "AOI,CCD4,6,ProductID,Barcode,";
                    break;
                case Command.CCD7CheckHaveTray:
                    //ATP，CCD7，ProcessID，PosID,Times,X,Y,R，UserString
                    if (param != null && param.Length >= 1)
                        //PosID,Times,X,Y
                        this.CmdSend = $"LCR,CCD7,11,{param[0]},userString";
                    else
                        this.CmdSend = "LCR,CCD7,11,ProductID,Barcode,";
                    break;
                case Command.CCD7索要料盘取料结果:
                    //ATR，CCD7,ProcessID,PosID1,PosID2，PosID3，CCID1,CCID2,UserString;
                    if (param != null && param.Length >= 2)
                        //穴位号1，穴位号2，轨道号，吸嘴号1，吸嘴号2
                        this.CmdSend = $"ATR,CCD7,7,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},userString";
                    else
                        this.CmdSend = "ATR,CCD4,7,ProductID,Barcode,";
                    break;

                case Command.CCD7索要料盘放料结果对应CCD5:
                    //ATP，CCD7，ProcessID，PosID,Times,X,Y,R，UserString
                    if (param != null && param.Length >= 2)
                        //PosID,Times,X,Y
                        this.CmdSend = $"ATR,CCD7,8,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},userString";
                    else
                        this.CmdSend = "AOI,CCD4,8,ProductID,Barcode,";
                    break;

                case Command.CCD7索要料盘放料结果对应CCD6:
                    //ATP，CCD7，ProcessID，PosID,Times,X,Y,R，UserString
                    if (param != null && param.Length >= 2)
                        //PosID,Times,X,Y
                        this.CmdSend = $"ATR,CCD7,9,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},userString";
                    else
                        this.CmdSend = "AOI,CCD4,9,ProductID,Barcode,";
                    break;
                case Command.九点标定:
                    if (param != null && param.Length >= 2)
                        //相机名称(CCD5),流程ID，点位ID，X,Y,R
                        this.CmdSend = $"CALS,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]},userString";
                    else
                        this.CmdSend = "CALS,CCD5,9,ProductID,Barcode,";
                    break;
                case Command.吸嘴旋转标定:
                    if (param != null && param.Length >= 2)
                        //相机名称(CCD5),流程ID，点位ID，X,Y,R
                        this.CmdSend = $"CCS,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]},userString";
                    else
                        this.CmdSend = "CCS,CCD5,9,ProductID,Barcode,";
                    break;

                #region 相机动静态指令
                case Command.CCD5静态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Static,CCD5"; //
                    else
                        this.CmdSend = "Static,CCD5";
                    break;
                case Command.CCD6静态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Static,CCD6"; //
                    else
                        this.CmdSend = "Static,CCD6";
                    break;
                case Command.CCD7静态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Static,CCD7"; //
                    else
                        this.CmdSend = "Static,CCD7";
                    break;
                case Command.CCD5动态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Dynamic,CCD5"; //
                    else
                        this.CmdSend = "Dynamic,CCD5";
                    break;
                case Command.CCD6动态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Dynamic,CCD6"; //
                    else
                        this.CmdSend = "Dynamic,CCD6";
                    break;
                case Command.CCD7动态:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = "Dynamic,CCD7"; //
                    else
                        this.CmdSend = "Dynamic,CCD7";
                    break;
                #endregion
                case Command.CCD5注册吸嘴模板:
                    if (param != null && param.Length >= 1)
                        //流程ID  x,y,z
                        this.CmdSend = $"TA,CCD5,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},userString"; //
                    else
                        this.CmdSend = "TA,CCD5";
                    break;

                case Command.CCD6注册吸嘴模板:
                    if (param != null && param.Length >= 1)
                        //流程ID  x,y,z
                        this.CmdSend = $"TA,CCD6,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},userString"; //
                    else
                        this.CmdSend = "TA,CCD5";
                    break;



                case Command.CCD7穴位内物料有无:
                    if (param != null && param.Length >= 2)
                        //相机名称(CCD5),流程ID，点位ID，X,Y,R
                        this.CmdSend = $"CCS,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]},{param[6]},{param[7]},userString";
                    else
                        this.CmdSend = "CCS，CCD5,9,ProductID,Barcode,";
                    break;

                case Command.CCD7与CCD5或CCD6关联:
                    if (param != null && param.Length >= 2)
                        //指令,相机名称,流程ID，X,Y,R
                        this.CmdSend = $"LKT,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},{param[5]},{param[6]},{param[7]},userString";
                    else
                        this.CmdSend = "CCS，CCD5,9,ProductID,Barcode,";
                    break;
                case Command.CCD5拍菲林片:
                    if (param != null && param.Length >= 2)
                        //指令,相机名称,流程ID，X,Y,R
                        this.CmdSend = $"LKC,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},userString";
                    else
                        this.CmdSend = "CCS，CCD5,9,ProductID,Barcode,";
                    break;
                case Command.CCD6拍菲林片:
                    if (param != null && param.Length >= 2)
                        //指令,相机名称,流程ID，X,Y,R
                        this.CmdSend = $"LKC,{param[0]},{param[1]},{param[2]},{param[3]},{param[4]},userString";
                    else
                        this.CmdSend = "CCS，CCD5,9,ProductID,Barcode,";
                    break;
                case Command.料盘型号:
                    if (param != null && param.Length >= 3)
                        //LCR，CCD1，ProcessID，X,Y,R，UserString;
                        this.CmdSend = $"Initialize,{param[0]}";
                    else
                        this.CmdSend = $"Initialize,{param[0]}";
                    break;

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
            string[] arrRecv = strRcv;
            switch ((Command)nCmdIdx)
            {
                case Command.CCD7料盘定位拍照:
                    CCD7result = GetRcvInt(2) == 1;
                    CCD7CavityRet = GetRcvString(3);
                    break;
                case Command.CCD7CheckHaveTray:
                    CCD7CheckTrayRet = GetRcvInt(2) == 1;
                    CCD7HaveProduct = GetRcvInt(3) == 1;
                    break;
                case Command.CCD5下料定位拍照:
                    CCD5result1 = GetRcvInt(2) == 1;
                    CCD5result2 = GetRcvInt(3) == 1;
                    //ccd5X = GetRcvDouble(3);
                    //ccd5Y = GetRcvDouble(4);
                    //ccd5Z = GetRcvDouble(5);
                    break;
                case Command.CCD6下料定位拍照:
                    CCD6result1 = GetRcvInt(2) == 1;
                    CCD6result2 = GetRcvInt(3) == 1;
                    //ccd6X = GetRcvDouble(3);
                    //ccd6Y = GetRcvDouble(4);
                    //ccd6Z = GetRcvDouble(5);
                    break;
                case Command.CCD7索要料盘取料结果:
                    CCD7GetResultPick = GetRcvInt(2) == 1;
                    if (GetRcvInt(9) == 3)
                    {
                        dbCCD7GetPickResultOK1[0]= GetRcvDouble(3);
                        dbCCD7GetPickResultOK1[1] = GetRcvDouble(4);
                        dbCCD7GetPickResultOK1[2] = GetRcvDouble(5);
                    }
                    else if(GetRcvInt(9) == 4)
                    {
                        dbCCD7GetPickResultOK2[0] = GetRcvDouble(3);
                        dbCCD7GetPickResultOK2[1] = GetRcvDouble(4);
                        dbCCD7GetPickResultOK2[2] = GetRcvDouble(5);
                    }
                    else if(GetRcvInt(9) == 1)
                    {
                        dbCCD7GetPickResultNG1[0] = GetRcvDouble(3);
                        dbCCD7GetPickResultNG1[1] = GetRcvDouble(4);
                        dbCCD7GetPickResultNG1[2] = GetRcvDouble(5);
                    }
                    else if(GetRcvInt(9) == 2)
                    {
                        dbCCD7GetPickResultNG2[0] = GetRcvDouble(3);
                        dbCCD7GetPickResultNG2[1] = GetRcvDouble(4);
                        dbCCD7GetPickResultNG2[2] = GetRcvDouble(5);
                    }
                    if (GetRcvInt(10) == 3)
                    {
                        dbCCD7GetPickResultOK1[0] = GetRcvDouble(6);
                        dbCCD7GetPickResultOK1[1] = GetRcvDouble(7);
                        dbCCD7GetPickResultOK1[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 4)
                    {
                        dbCCD7GetPickResultOK2[0] = GetRcvDouble(6);
                        dbCCD7GetPickResultOK2[1] = GetRcvDouble(7);
                        dbCCD7GetPickResultOK2[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 1)
                    {
                        dbCCD7GetPickResultNG1[0] = GetRcvDouble(6);
                        dbCCD7GetPickResultNG1[1] = GetRcvDouble(7);
                        dbCCD7GetPickResultNG1[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 2)
                    {
                        dbCCD7GetPickResultNG2[0] = GetRcvDouble(6);
                        dbCCD7GetPickResultNG2[1] = GetRcvDouble(7);
                        dbCCD7GetPickResultNG2[2] = GetRcvDouble(8);
                    }
                    break;
                case Command.CCD7索要料盘放料结果对应CCD5:
                    CCD7GetResultPlaceCCD5 = GetRcvInt(2) == 1;
                    if (GetRcvInt(9) == 3)
                    {
                        dbCCD7GetPlaceResultCCD5OK1[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD5OK1[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD5OK1[2] = GetRcvDouble(5);
                    }
                    else if (GetRcvInt(9) == 4)
                    {
                        dbCCD7GetPlaceResultCCD5OK2[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD5OK2[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD5OK2[2] = GetRcvDouble(5);
                    }
                    else if (GetRcvInt(9) == 1)
                    {
                        dbCCD7GetPlaceResultCCD5NG1[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD5NG1[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD5NG1[2] = GetRcvDouble(5);
                    }
                    else if (GetRcvInt(9) == 2)
                    {
                        dbCCD7GetPlaceResultCCD5NG2[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD5NG2[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD5NG2[2] = GetRcvDouble(5);
                    }
                    if (GetRcvInt(10) == 3)
                    {
                        dbCCD7GetPlaceResultCCD5OK1[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD5OK1[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD5OK1[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 4)
                    {
                        dbCCD7GetPlaceResultCCD5OK2[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD5OK2[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD5OK2[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 1)
                    {
                        dbCCD7GetPlaceResultCCD5NG1[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD5NG1[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD5NG1[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 2)
                    {
                        dbCCD7GetPlaceResultCCD5NG2[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD5NG2[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD5NG2[2] = GetRcvDouble(8);
                    }
                    break;
                case Command.CCD7索要料盘放料结果对应CCD6:
                    CCD7GetResultPlaceCCD6 = GetRcvInt(2) == 1;
                    if (GetRcvInt(9) == 3)
                    {
                        dbCCD7GetPlaceResultCCD6OK1[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD6OK1[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD6OK1[2] = GetRcvDouble(5);
                    }
                    else if (GetRcvInt(9) == 4)
                    {
                        dbCCD7GetPlaceResultCCD6OK2[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD6OK2[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD6OK2[2] = GetRcvDouble(5);
                    }
                    else if (GetRcvInt(9) == 1)
                    {
                        dbCCD7GetPlaceResultCCD6NG1[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD6NG1[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD6NG1[2] = GetRcvDouble(5);
                    }
                    else if (GetRcvInt(9) == 2)
                    {
                        dbCCD7GetPlaceResultCCD6NG2[0] = GetRcvDouble(3);
                        dbCCD7GetPlaceResultCCD6NG2[1] = GetRcvDouble(4);
                        dbCCD7GetPlaceResultCCD6NG2[2] = GetRcvDouble(5);
                    }
                    if (GetRcvInt(10) == 3)
                    {
                        dbCCD7GetPlaceResultCCD6OK1[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD6OK1[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD6OK1[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 4)
                    {
                        dbCCD7GetPlaceResultCCD6OK2[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD6OK2[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD6OK2[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 1)
                    {
                        dbCCD7GetPlaceResultCCD6NG1[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD6NG1[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD6NG1[2] = GetRcvDouble(8);
                    }
                    else if (GetRcvInt(10) == 2)
                    {
                        dbCCD7GetPlaceResultCCD6NG2[0] = GetRcvDouble(6);
                        dbCCD7GetPlaceResultCCD6NG2[1] = GetRcvDouble(7);
                        dbCCD7GetPlaceResultCCD6NG2[2] = GetRcvDouble(8);
                    }
                    break;
                case Command.九点标定:
                    if (GetRcvString(1) == "CCD5")
                    {
                        CCD5CalibResult = GetRcvInt(3) == 1;
                    }
                    else if(GetRcvString(1) == "CCD6")
                    {
                        CCD6CalibResult = GetRcvInt(3) == 1;
                    }
                        break;
                case Command.吸嘴旋转标定:
                    if (GetRcvString(1) == "CCD5")
                    {
                        CCD5CalibResult = GetRcvInt(3) == 1;
                    }
                    else if (GetRcvString(1) == "CCD6")
                    {
                        CCD6CalibResult = GetRcvInt(3) == 1;
                    }
                    break;
                case Command.CCD5注册吸嘴模板:
                    CCD5RegisterTemplateResult = GetRcvInt(3) == 1;
                    break;
                case Command.CCD6注册吸嘴模板:
                    CCD6RegisterTemplateResult = GetRcvInt(3) == 1;
                    break;
                case Command.CCD7与CCD5或CCD6关联:
                    CCD7RelativeToCCD5OrCCD6Result = GetRcvInt(2) == 1;
                    break;
                case Command.CCD5拍菲林片:
                    CCD7RelativeToCCD5OrCCD6Result = GetRcvInt(2) == 1;
                    break;
                case Command.CCD6拍菲林片:
                    CCD7RelativeToCCD5OrCCD6Result = GetRcvInt(2) == 1;
                    break;
                case Command.料盘型号:
                    InitializeResult = GetRcvString(1) == "OK";
                    if (InitializeResult)
                    {
                        trayTypeFromVision = GetRcvString(2);
                    }
                    else
                    {
                        trayTypeFromVision = string.Empty;
                    }
                    break;
            }
            return true;
        }

        public bool Communicate(object nCmdIdx, bool bSend, ref string recvStr, params object[] param)
        {
            lock (m_lock)
            {
                if (!tcplink.IsOpen)
                {
                    tcplink.Open();
                }

                tcplink.ClearBuffer();
                if (bSend)
                {
                    string sendString = GetSendString(nCmdIdx, param);
                    tcplink.WriteLine(sendString);
                    Logger.Process((station == null) ? "" : station.Name, "->[" + tcplink.m_strName + "] " + sendString);
                }              
                Delay(100);
                if (!WaitTcpEx(strCmdHead))
                {
                    return false;
                }

                recvStr = strRcvOrg;
                return AnalyzeRcvString(nCmdIdx);
            }
        }

        private bool WaitTcpEx(string strStartWith = "", int nTimeOut = 0)
        {
            bool flag = true;
            string strReceive = "";
            if (station == null)
            {
                string strData = "";
                string text = "";
                int num = 0;
                while (true)
                {
                    tcplink.ReadLine(out strData, bManualBreak: true);
                    text += strData;
                    if (strStartWith.Length > 0 && text.StartsWith(strStartWith))
                    {
                        strReceive = text;
                        break;
                    }

                    if (text.Length > 0)
                    {
                        strReceive = text;
                        break;
                    }

                    Thread.Sleep(20);
                    num += 20;
                    if (num > 15000)
                    {
                        EventMgr.Notice(ErrCommunicate.超时, tcplink.m_strName, "等待超时".txtTrans(TransType.Mark), StationName);
                        num = 0;
                    }
                }
            }
            else
            {
                flag = station.WaitTcp(tcplink, out strReceive, strStartWith, "", nTimeOut, bTimeOutReturn: false);
            }

            if (flag)
            {
                strRcvOrg = strReceive;
                strRcv = strReceive.Split(',');
                for (int i = 0; i < strRcv.Length; i++)
                {
                    strRcv[i] = strRcv[i].Trim();
                }

                return true;
            }

            strRcvOrg = "";
            strRcv = new string[0];
            return false;
        }
    }
}