using AF.Common;
using AF.Communicate;
using AF.Data;
using AF.Station;
using Autoframe;
using System;
using System.Collections.Generic;

namespace Autoframe
{
    public class TcpCommu_Robot : TcpCommunicate
    {
        //通讯变量
        public double fX;
        public double fY;
        public double fZ;
        public double fU;

        /// <summary>
        /// 结果
        /// </summary>
        public bool result;


        // 命令枚举
        public enum Command
        {
            初始化,
            取料,
            放料,
        }

        public TcpCommu_Robot() : base()
        {
            //绑定网口
            this.tcplink = TcpClientMgr.GetClient(1);
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
                case Command.初始化:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"Init,{param[0]}";
                    else
                        this.CmdSend = "Init,速度";
                    break;

                case Command.取料:
                    if (param != null && param.Length >= 1)
                        this.CmdSend = $"Pick,{param[0]}";
                    else
                        this.CmdSend = "Pick,穴位";
                    break;

                case Command.放料:
                    if (param != null && param.Length >= 3)
                        this.CmdSend = $"Place,{param[0]},{param[1]},{param[2]}";
                    else
                        this.CmdSend = "Place,X,Y,U";
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
            switch ((Command)nCmdIdx)
            {
                case Command.初始化:
                    result = GetRcvInt(1) == 1;
                    break;
                case Command.取料:
                    result = (GetRcvInt(1) == 1);
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
    }
}