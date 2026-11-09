using AF.Common;
using AF.Communicate;
using AF.File;
using AF.Upload.Hive;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;

namespace Autoframe
{
    public static class SFCQuery
    {
        private static object objLock = new object();
        private static TcpClient link = TcpClientMgr.GetClient(0);//根据自己项目进行修改索引号

        public enum SFCMode
        {
            GetStationID,
            GetCardTagid,

            GetProduct,
        }
        // ========== 基础设施 ==========
        public static bool Connect()
        {
            if (link != null)
            {
                link.Close();
                Thread.Sleep(50);
                return link.Open();
            }
            return false;
        }

        public static void Disconnect()
        {
            link?.Close();
        }

        public static void SendMessage( string msg)
        {
            link?.WriteString(msg);
        }

        public static bool IsSuccess(string reply)
        {
            if (string.IsNullOrEmpty(reply)) return false;

            foreach (string str in reply.Split('\n'))
            {
                string[] parts = str.Split('@');
                if (parts.Length > 0)
                {
                    string head = parts[0].ToLower();
                    if (head.Contains("bad") || head.Contains("err"))
                        return false;
                }
            }
            return true;
        }

        private static string RegexExtract(string strMsg, string regular)
        {
            Match m = Regex.Match(strMsg, regular);
            return m.Success ? m.Groups[1].Value : "";
        }

        // ========== 私有核心：只负责通信 + 返回原始收发 ==========
        private static bool SendAndReceive(SFCMode mode, string cardId,
                                           out string strSend,
                                           out string strReveive)
        {
            strSend = "";
            strReveive = "";

            if (cardId != null)
                cardId = cardId.Replace("+", "%2B");

            switch (mode)
            {
                case SFCMode.GetStationID:
                    strSend = "ghi_station\n";
                    break;
                case SFCMode.GetProduct://获取专案名称
                    strSend = "ghi_product\n";
                    break;
                case SFCMode.GetCardTagid:
                    string machineSN = Ini.GetStringValue(HiveCommandProcessor.m_strVendorStationInfoIniFile, "machine_info", "machine_sn", "");
                    strSend = $"sfc_post@c=QUERY_4_SFC&subcmd=query_skills_info&isAutomation=1&cardtagid={cardId}&machinesn={machineSN}\n";
                    break;
            }

            lock (objLock)
            {
                int failCount = 0;
                while (true)
                {
                    failCount++;
                    try
                    {
                        Connect();
                        SendMessage(strSend);
                        Thread.Sleep(100);
                        link.ReadLine(out strReveive, "}@\n");
                        break;
                    }
                    catch
                    {
                        Disconnect();
                        Thread.Sleep(200);
                    }

                    if (failCount >= 3)
                    {
                        failCount = 0;
                        MessageMgr.Show($"SFC- {mode} 失败，请检查后重试", "SFC");
                        Disconnect();
                        return false;
                    }
                }
            }

            strSend = strSend.Trim('\n');
            strReveive = (strReveive ?? "").Trim('\n');
            return true;
        }

        // ========== 公开方法 1：只针对 GetStationID / HiveStationInfo ==========
        public static bool M_UploadSFC( ref HiveStationInfo hiveStationInfo)
        {
            string strSend1, strReveive1, strSend2, strReveive2;
            SendAndReceive(SFCMode.GetStationID, "",  out strSend1, out strReveive1);
            SendAndReceive(SFCMode.GetProduct, "", out strSend2, out strReveive2);

            bool flag = true;

            if (!IsSuccess(strReveive1) || !IsSuccess(strReveive2))
            {
                flag = false;
                if (string.IsNullOrEmpty(strReveive1) || string.IsNullOrEmpty(strReveive1))
                    hiveStationInfo.stationID = "CommError";   // 或你想放的标志
            }
            else
            {
                string[] StrStationID = RegexExtract(strReveive1, @"\{([^}]*)\}").Split('_');//FXGL_C04-4FB-03_3_AE-72
                hiveStationInfo.site = StrStationID[0];
                hiveStationInfo.building = StrStationID[1].Split('-')[0];
                hiveStationInfo.station_type = StrStationID[3];
                hiveStationInfo.line = StrStationID[1];
                hiveStationInfo.instance =Convert.ToInt32( StrStationID[2]);
                hiveStationInfo.line_type = "default";
                hiveStationInfo.station_name = "default";
                hiveStationInfo.product = RegexExtract(strReveive2, @"\{([^}]*)\}");

                flag = !string.IsNullOrEmpty(hiveStationInfo.stationID) && hiveStationInfo.stationID.Length > 5 && (hiveStationInfo.product.Length > 1);
            }

            HiveMgr.SaveLog(strSend1, true, strReveive1, DataStyle.Other_sfc);
            HiveMgr.SaveLog(strSend2, true, strReveive2, DataStyle.Other_sfc);
            return flag;
        }

        // ========== 公开方法 2：只针对 GetCardTagid / HiveUserInfo ==========
        public static bool M_UploadSFC( ref HiveUserInfo hiveUserInfo)
        {
            string strSend, strReveive;
            SendAndReceive(SFCMode.GetCardTagid, hiveUserInfo.CardTagid, out strSend, out strReveive);

            bool flag = IsSuccess(strReveive) && strReveive.Contains("SFC_OK");

            if (flag)
            {
                string skillsInfo = RegexExtract(strReveive, @"skills_info=([^@}]*)");

                if (string.IsNullOrEmpty(skillsInfo))
                {
                    flag = false;
                }
                else if (skillsInfo.Trim() == "" || skillsInfo.Contains("err"))
                {
                    flag = false;
                }
                else
                {
                    string[] parts = skillsInfo.Split(';');
                    if (parts.Length < 3) parts = skillsInfo.Split(',');

                    if (parts.Length >= 3)
                    {
                        hiveUserInfo.Name = parts[0].Trim();
                        hiveUserInfo.Vendor = parts[1].Trim();
                        hiveUserInfo.Level = parts[2].Trim();
                    }
                    else
                    {
                        flag = false;
                    }
                }
            }
            hiveUserInfo.strSend = strSend;
            hiveUserInfo.strReceive = strReveive.Trim();
            HiveMgr.SaveLog(strSend, true, strReveive, DataStyle.Other_sfc);
            return flag;
        }
    }
}