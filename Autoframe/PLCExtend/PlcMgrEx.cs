using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using AF.Common;
using AF.Communicate;
using AutoFrame;

using System.IO;
using Foundation;


namespace AutoFrame.PLCExtend
{
    public  class PlcMgrEx:Singleton<PlcMgrEx>
    {
        //---------------------- PLC  连接地址 ----------------------//
        /// <summary>
        /// 连接地址
        /// </summary>
        private static string[] m_strNETItem = { "序号", "名称", "通讯方式", "IP地址", "端口", "超时时间" };
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> m_dicNET_SN = new Dictionary<string, string>();
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, string> m_dicNET_Name = new Dictionary<string, string>();

        public Dictionary<string, string> m_dicNETStyle = new Dictionary<string, string>();

        public Dictionary<string, string> m_dicNETIP = new Dictionary<string, string>();

        public Dictionary<string, string> m_dicNETPort = new Dictionary<string, string>();

        public Dictionary<string, string> m_dicNETOverTime = new Dictionary<string, string>();

        //---------------------- PLC  交互寄存器类型 ----------------------//
        /// <summary>
        /// 交互寄存器属性
        /// </summary>
        private static string[] m_strRegItem = { "序号", "控制类型", "元件类型", "起始地址", "元件名称", "名称翻译" };
        /// <summary>
        /// 交互寄存器 元件类型
        /// </summary>
        public Dictionary<string, string> m_dicReg_SN = new Dictionary<string, string>();
        /// <summary>
        /// 交互寄存器 元件类型
        /// </summary>
        public Dictionary<string, string> m_dicReg_ControlType = new Dictionary<string, string>();
        /// <summary>
        /// 交互寄存器 元件类型
        /// </summary>
        public Dictionary<string, string> m_dicReg_UintType = new Dictionary<string, string>();
        /// <summary>
        /// 交互寄存器 起始地址
        /// </summary>
        public Dictionary<string, string> m_dicReg_StartAdress = new Dictionary<string, string>();
        /// <summary>
        /// 交互寄存器 名称翻译
        /// </summary>
        public Dictionary<string, string> m_dicReg_Translate = new Dictionary<string, string>();

    


        public string[] m_strArrayNET;
        /// <summary>
        /// 交互寄存器名称向量
        /// </summary>
        public string[] m_strArrayReg;
       

        /// <summary>
        /// 类构造函数
        /// </summary>
        public PlcMgrEx()
        {

        }


        /// <summary>
        /// 读取系统配置
        /// </summary>
        /// <param name="strFile">配置文件 </param>
        /// <returns></returns>
        public bool LoadCfgFile(string strFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Application.StartupPath + "\\" + "Config" +"\\"+ strFile);
            if (doc.HasChildNodes)
            {
                ReadNetCfgFromXml(doc);
                ReadRegCfgFromXml(doc);
                return true;
            }
            return false;
        }

        private void ReadNetCfgFromXml(XmlDocument doc)
        {
            m_dicNET_SN.Clear();
            m_dicNET_Name.Clear();
            m_dicNETStyle.Clear();
            m_dicNETIP.Clear();
            m_dicNETPort.Clear();
            m_dicNETOverTime.Clear();
            m_strArrayNET = new string[3];
            int index = 0;
            XmlNodeList xnl = doc.SelectNodes("/SystemCfg/Plc/汇川PLC");
            if (xnl.Count > 0)
            {
                xnl = xnl.Item(0).ChildNodes;
                if (xnl.Count > 0)
                {
                    foreach (XmlNode xn in xnl)
                    {
                        XmlElement xe = (XmlElement)xn;
                        string strNETSN = xe.GetAttribute(m_strNETItem[0]).Trim();
                        string strNETName = xe.GetAttribute(m_strNETItem[1]).Trim();
                        string strNETStyle = xe.GetAttribute(m_strNETItem[2]).Trim();
                        string strNETIP = xe.GetAttribute(m_strNETItem[3]).Trim();
                        string strNETPort = xe.GetAttribute(m_strNETItem[4]).Trim();
                        string strNETOverTime = xe.GetAttribute(m_strNETItem[5]).Trim();

                        if (strNETName != string.Empty)
                        {
                            try
                            {
                                m_dicNETIP.Add(strNETSN, strNETIP);
                                m_dicNETPort.Add(strNETSN, strNETPort);
                                m_dicNETOverTime.Add(strNETSN, strNETOverTime);
                                m_dicNET_Name.Add(strNETSN, strNETName);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString(), strNETName);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        ///读取系统配置文件里PLC 寄存器单元配置 
        /// </summary>
        /// <param name="doc"></param>
        private void ReadRegCfgFromXml(XmlDocument doc)
        {
            m_dicReg_SN.Clear();
            m_dicReg_ControlType.Clear();
            m_dicReg_UintType.Clear();
            m_dicReg_StartAdress.Clear();
            m_dicReg_Translate.Clear();
            m_strArrayReg = new string[1000];
            int index = 0;
            XmlNodeList xnl = doc.SelectNodes($"/SystemCfg/Plc/Register");
            if (xnl.Count > 0)
            {
                xnl = xnl.Item(0).ChildNodes;
                if (xnl.Count > 0)
                {
                    foreach (XmlNode xn in xnl)
                    {
                        XmlElement xe = (XmlElement)xn;
                        string strSN = xe.GetAttribute(m_strRegItem[0]).Trim();
                        string strControlType = xe.GetAttribute(m_strRegItem[1]).Trim();
                        string strUintType = xe.GetAttribute(m_strRegItem[2]).Trim();
                        string strStartAdress = xe.GetAttribute(m_strRegItem[3]).Trim();
                        string strName = xe.GetAttribute(m_strRegItem[4]).Trim();
                        string strTranslate = xe.GetAttribute(m_strRegItem[5]).Trim();
                        if (string.IsNullOrEmpty(strTranslate))
                        {
                            strTranslate = strName;
                        }

                        if (strName != string.Empty)
                        {
                            try
                            {
                                m_strArrayReg[index++] = strName;
                                m_dicReg_SN.Add(strName, strSN);
                                m_dicReg_ControlType.Add(strName, strControlType);
                                m_dicReg_UintType.Add(strName, strUintType);
                                m_dicReg_StartAdress.Add(strName, strStartAdress);
                                m_dicReg_Translate.Add(strName, strTranslate);
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString(), strName);
                            }
                        }
                    }
                }
            }
        }       
    }
}
