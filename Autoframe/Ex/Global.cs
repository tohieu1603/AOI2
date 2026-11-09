using AutoFrame.DBExtend;
using AutoFrame.PLCExtend;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autoframe
{
    static class Global
    {
        /// <summary>
        /// 料盘行数
        /// </summary>
        public static int trayTotalRow = 8;
        /// <summary>
        /// 料盘列数
        /// </summary>
        public static int trayTotalCol = 8;

        // 穴位个数
        public static int slotTotalCount => trayTotalRow * trayTotalCol;
        /// <summary>
        /// 料盘行间距
        /// </summary>
        public static double trayRowGap = 20;
        /// <summary>
        /// 料盘列间距
        /// </summary>
        public static double trayColGap = 20;

        public static int TrayID = 1;


        public static string trayTypeName = string.Empty;
        /// <summary>
        /// 流道模式开关
        /// </summary>
        public static bool IsPickProduct = false;
        /// <summary>
        /// 清料模式开关
        /// </summary>
        public static bool IsClearAllProduct = false;
        /// <summary>
        ///PLC
        /// </summary>
        /// 

        public static PlcHelper m_plcHelper = null;
        /// <summary>
        /// 料盘动子号
        /// </summary>
        public static string moveidnum;

        public static string[] arr_moveID = new string[6] { "0", "0", "0", "0", "0", "0" };


        public static bool b_IsMaintenanceMode = false;

        public static string strMaintenanceID = "";

        public static double d_CT = 0;

        public static string m_RESPMonitorMover = "MonitorMover_Data";
        public static string[] ListNameStation = { "入料接驳站", "Wait1", "激光位移检测站", "Wait2", "下相机AOI1站", "Wait3", "上相机AOI站", "Wait4", "下相机AOI2站", "Wait5", "出料接驳站", "中转取放站" };


        public static bool RedisInit()
        {
            RedisHelperEx.Instance.AddConnection();
            ConnectionMultiplexer connectionMultiplexer;
            bool isConnectOK = RedisHelperEx.Instance.IsConnected("0", out connectionMultiplexer);

            if (!isConnectOK)
            {
                //MessageBox.Show("数据库连接失败");
                return false;
            }
            else
            {
                // MessageBox.Show("数据库连接成功");
                return true;

            }
        }
    }
}
