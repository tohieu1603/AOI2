using AF.Common;
using AF.Communicate;
using AF.Data;
using Autoframe;
using EasyModbus;
using NModbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.Linq;
using static Foundation.LogBase;

namespace AutoFrame.PLCExtend
{
    // PLC客户端信息类，存储每个PLC的连接和状态信息
    public class PlcClient
    {
        public int SequenceNumber { get; set; } // PLC序号（1=3D工位，2=下料工位）
        public ModbusClient ModbusClient { get; set; } = new ModbusClient();
        public string IpAddress { get; set; } = "";
        public int Port { get; set; } = 0;
        public Thread ReadThread { get; set; }
        public int[] RegisterValues { get; set; } = new int[123]; // 缓存读取的寄存器值
        public string StartAddress { get; set; } = "50"; // 起始地址（从配置读取）
        public bool IsConnected { get; set; } // 可读写属性

        // 添加线程终止标志
        public bool ShouldStop { get; set; }
    }

    public class PlcHelper 
    {
        // 存储PLC客户端信息，key为序号（1=3D工位，2=下料工位）
        private Dictionary<int, PlcClient> _plcClients = new Dictionary<int, PlcClient>();
        IEEE754Hleper IEEE754 = new IEEE754Hleper();
        private int MaxIndex = 100;
        private readonly object _plcLock = new object(); // 线程安全锁

        /// <summary>
        /// ShowLog委托
        /// </summary>
        public delegate void ShowLogHandler1(string strLog);

        public PlcHelper()
        {
            InitPlcClients();
        }

        /// <summary>
        /// 初始化PLC客户端列表
        /// </summary>
        private void InitPlcClients()
        {
            _plcClients.Clear();
            // 从配置中获取所有PLC序号
            var plcNumbers = PlcMgrEx.GetInstance().m_dicNETIP.Keys
                .Where(key => int.TryParse(key, out _))
                .Select(key => int.Parse(key))
                .ToList();

            foreach (var seq in plcNumbers)
            {
                _plcClients[seq] = new PlcClient
                {
                    SequenceNumber = seq,
                    StartAddress = GetStartAddress($"{seq}_PLC寄存器起始地址")
                };
            }
        }

        // 获取指定PLC的寄存器起始地址
        private string GetStartAddress(string registerName)
        {
            PlcMgrEx.GetInstance().m_dicReg_StartAdress.TryGetValue(registerName, out string address);
            return address;
        }

        /// <summary>
        /// 检查指定序号的PLC是否连接
        /// </summary>
        public bool IsConnected(int plcSequence)
        {
            return _plcClients.TryGetValue(plcSequence, out var client) && client.IsConnected;
        }

        /// <summary>
        /// 连接指定序号的PLC（1=3D，2=下料）
        /// </summary>
        public bool ConnectPLC(int plcSequence)
        {
            try
            {
                if (!_plcClients.ContainsKey(plcSequence))
                {
                    //MessageBox.Show($"PLC序号{plcSequence}未配置");
                    return false;
                }
                // 先断开已有连接
                DisConnectPLC(plcSequence);
                // 从配置字典中获取IP和端口
                string plcIdStr = plcSequence.ToString();
                string ipAddress;
                string portStr;
                if (!PlcMgrEx.GetInstance().m_dicNETIP.TryGetValue(plcIdStr, out ipAddress) ||
                    !PlcMgrEx.GetInstance().m_dicNETPort.TryGetValue(plcIdStr, out portStr) ||
                    !int.TryParse(portStr, out int port))
                {
                    MessageBox.Show($"PLC序号{plcSequence}的IP/端口配置缺失");
                    return false;
                }
                var client = _plcClients[plcSequence];
                // 建立新连接
                client.ModbusClient = new ModbusClient(ipAddress, port);
                client.ModbusClient.Connect();
                client.IsConnected = client.ModbusClient.Connected; // 连接后同步状态
                client.IpAddress = ipAddress;
                client.Port = port;
                if (client.IsConnected)
                {
                    client.ShouldStop = false;
                    client.ReadThread = new Thread(() => 一次性读取寄存器数据(plcSequence))
                    {
                        IsBackground = true
                    };
                    client.ReadThread.Start();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接序号为{plcSequence}的PLC失败: {ex.Message}"); 
                return false;
            }
        }

        /// <summary>
        /// 断开指定序号的PLC连接
        /// </summary>
        public void DisConnectPLC(int plcSequence)
        {
            if (_plcClients.TryGetValue(plcSequence, out var client))
            {
                client.ShouldStop = true;
                // 等待线程安全退出
                if (client.ReadThread != null && client.ReadThread.IsAlive)
                {
                    client.ReadThread.Join(1000);
                    if (client.ReadThread.IsAlive)
                    {

                    }
                }

                // 断开连接
                if (client.ModbusClient != null && client.IsConnected)
                {
                    try
                    {
                        client.ModbusClient.Disconnect();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"断开PLC连接时出错: {ex.Message}");
                    }
                }
                client.ReadThread = null;
            }
        }


        /// <summary>
        /// 持续读取指定序号PLC的寄存器数据
        /// </summary>
        private void 一次性读取寄存器数据(int plcSequence)
        {
            if (!_plcClients.TryGetValue(plcSequence, out var client))
                return;
            while (!client.ShouldStop)
            {
                try
                {
                    if (client.IsConnected)
                    {
                        int startAddr = Convert.ToInt16(client.StartAddress);
                        bool b_result = ReadData_ALLString(plcSequence, new int[1] { startAddr }, 123, out int[] values);
                        if (b_result)
                        {
                            lock (_plcLock)
                            {
                                client.RegisterValues = values;
                            }
                        }
                    }
                    else if (!client.ShouldStop)
                    {
                        try
                        {
                            client.ModbusClient.Connect();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"PLC {plcSequence} 重新连接失败: {ex.Message}");
                            Thread.Sleep(1000);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"读取序号为{plcSequence}的PLC寄存器失败: {ex.ToString()}");
                    Thread.Sleep(1000);
                }
                Thread.Sleep(2);
            }

        }

        /// <summary>
        /// 一次性读取指定序号PLC的寄存器
        /// </summary>
        public bool ReadData_ALLString(int plcSequence, int[] regAddress, int strLength, out int[] value, ShowLogHandler1 ShowLog = null)
        {
            value = new int[1] { -1 };
            lock (_plcLock)
            {
                try
                {
                    if (!_plcClients.TryGetValue(plcSequence, out var client) || !client.IsConnected)
                    {
                        //ShowLog?.Invoke($"PLC {plcSequence} 未连接", LogLevel.Error);
                        Logger.Log("plc", $"PLC {plcSequence} 未连接");
                        return false;
                    }
                    try
                    {
                        client.ModbusClient.ReadHoldingRegisters(regAddress[0], 1); // 微型读取
                    }
                    catch
                    {
                        // 连1个寄存器都读不了，说明真断了
                        // ShowLog?.Invoke($"PLC {plcSequence} 已断开（实际检测）", LogLevel.Error);
                        Logger.Log("plc", $"PLC {plcSequence} 已断开（实际检测）");
                        client.IsConnected = false; // 更新状态
                        return false;
                    }
                    value = client.ModbusClient.ReadHoldingRegisters(regAddress[0], strLength);
                    return true;
                }
                catch (Exception ex)
                {
                    string errorMsg = $"序号为{plcSequence}的PLC读取寄存器线程失败！请检查！: {ex.Message}";
                    //ShowLog?.Invoke(errorMsg, LogLevel.Error);
                    Logger.Log("plc", errorMsg);
                    MessageBox.Show(errorMsg);
                    return false;
                }
            }
        }

        #region //-------------------------------- 半解析型读写 --------------------------------//
        /// <summary>
        /// PLC寄存器地址转 Modbus地址
        /// </summary>
        public int[] ConvertToIntAddress(string regAddress)
        {
            //地址规格  IX4.0    QX4.0    MX1200.0    MB610   
            Regex rg = new Regex(@"[0-9]+");
            Match m = rg.Match(regAddress);

            if (m.Length == 0)
                return null;

            int[] iAddress = { 0, 0 };
            try
            {
                switch (regAddress.Substring(0, 2))
                {
                    case "IX":
                        iAddress[0] = Convert.ToInt32(m.Value) * 8;
                        iAddress[1] = Convert.ToInt32(m.NextMatch().Value);
                        break;

                    case "QX":
                        iAddress[0] = Convert.ToInt32(m.Value) * 8;
                        iAddress[1] = Convert.ToInt32(m.NextMatch().Value);
                        break;

                    case "MX":
                        iAddress[0] = Convert.ToInt32(m.Value) / 2;//实际地址除以2比如%MX1000,对应的地址是500
                        iAddress[1] = Convert.ToInt32(m.NextMatch().Value);
                        if (Convert.ToInt32(m.Value) % 2 == 1)
                        {
                            iAddress[1] = iAddress[1] + 8;
                        }
                        break;

                    case "MB":
                        iAddress[0] = Convert.ToInt32(m.Value) / 2;//实际地址除以2比如%MB610,对应的地址是305
                        iAddress[1] = 0;
                        break;
                    case "MW":
                        iAddress[0] = Convert.ToInt32(m.Value);
                        iAddress[1] = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                // WarningMgr.GetInstance().Error("PLC ConvertToIntAddress Alarm", "");
                Logger.Log("plc", "PLC ConvertToIntAddress Alarm");
            }
            return iAddress;
        }




        /// <summary>
        /// 向指定序号PLC写入字符串
        /// </summary>
        public void WriteData_String(int plcSequence, int[] regAddress, string value, ShowLogHandler1 ShowLog = null)
        {
            lock (_plcLock)
            {
                int _count = 0;
                while (true)
                {
                    if (_count > 10)
                    {
                        MessageBox.Show($"序号为{plcSequence}的PLC连接失败");
                        _count = 0;
                        return;
                    }
                    try
                    {
                        if (!_plcClients.TryGetValue(plcSequence, out var client) || !client.IsConnected)
                        {
                            MessageBox.Show($"序号为{plcSequence}的PLC未连接");
                            return;
                        }

                        if (!string.IsNullOrEmpty(value))
                        {
                            // 以第一个\0（空字符）为界，截取前面的有效内容
                            int nullCharIndex = value.IndexOf('\0');
                            string cleanedValue = nullCharIndex > 0 ? value.Substring(0, nullCharIndex) : value;
                            int[] registers = ModbusClient.ConvertStringToRegisters(cleanedValue);
                            client.ModbusClient.WriteMultipleRegisters(regAddress[0], registers);
                            return;
                        }
                        else
                        {
                            // 空value时填充默认值
                           string defaultvalue = "VID__FAIL";
                            int[] registers = ModbusClient.ConvertStringToRegisters(defaultvalue); 
                            client.ModbusClient.WriteMultipleRegisters(regAddress[0], registers);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _count++;
                       // ShowLog?.Invoke($"序号为{plcSequence}的PLC写入字符串失败: {ex.Message}", LogLevel.Warn);
                        Logger.Log("plc", $"序号为{plcSequence}的PLC写入字符串失败: {ex.Message}");
                        // 尝试重新连接
                        DisConnectPLC(plcSequence);
                        Thread.Sleep(100);
                        ConnectPLC(plcSequence);
                        Thread.Sleep(500);
                    }
                }
            }
        }

        /// <summary>
        /// 向指定序号PLC写入布尔型数据
        /// </summary>
        public void WriteData_Bool(int plcSequence, string strAddress, bool value, ShowLogHandler1 ShowLog = null)
        {
            int DataResult_WMX = 0;
            lock (_plcLock)
            {
                if (ShowLog != null)
                {
                    ShowLog($"WriteData_Bool PLC{plcSequence}:{strAddress}:{value}");
                }
                int _count = 0;
                while (true)
                {
                    if (_count > 10)
                    {
                        MessageBox.Show($"序号为{plcSequence}的PLC连接失败");
                        _count = 0;
                        return;
                    }
                    try
                    {
                        if (!_plcClients.TryGetValue(plcSequence, out var client) || !client.IsConnected)
                        {
                            MessageBox.Show($"序号为{plcSequence}的PLC未连接");
                            return;
                        }

                        string type;
                        string address;
                        if (!PlcMgrEx.GetInstance().m_dicReg_UintType.TryGetValue(strAddress, out type) ||
                            !PlcMgrEx.GetInstance().m_dicReg_StartAdress.TryGetValue(strAddress, out address))
                        {
                            string strInfo = $"寄存器 {strAddress} 不存在，请检查配置";
                            MessageBox.Show(strInfo, "配置错误");
                            return;
                        }

                        int[] regAddress = ConvertToIntAddress(type + address);
                        int[] registers = client.ModbusClient.ReadHoldingRegisters(regAddress[0], 1);

                        if (value)
                        {
                            DataResult_WMX = registers[0] | (1 << regAddress[1]);//写1操作
                        }
                        else
                        {
                            DataResult_WMX = registers[0] & (~(1 << regAddress[1]));//写0操作
                        }

                        client.ModbusClient.WriteMultipleRegisters(regAddress[0], new int[] { DataResult_WMX });
                        return;
                    }
                    catch (Exception ex)
                    {
                        _count++;
                        string strEnAddress = "";
                        PlcMgrEx.GetInstance().m_dicReg_Translate.TryGetValue(strAddress, out strEnAddress);

                        DisConnectPLC(plcSequence);
                        Thread.Sleep(100);
                        ConnectPLC(plcSequence);
                        Thread.Sleep(500);
                    }
                }
            }
        }

        /// <summary>
        /// 向指定序号PLC写入字符串
        /// </summary>
        public void WriteData_String(int plcSequence, string strAddress, string value, ShowLogHandler1 ShowLog = null)
        {
            lock (_plcLock)
            {
                int _count = 0;
                while (true)
                {
                    if (_count > 10)
                    {
                        MessageBox.Show($"序号为{plcSequence}的PLC连接失败");
                        _count = 0;
                        return;
                    }
                    try
                    {
                        if (!_plcClients.TryGetValue(plcSequence, out var client) || !client.IsConnected)
                        {
                            MessageBox.Show($"序号为{plcSequence}的PLC未连接");
                            return;
                        }

                        string type;
                        string address;
                        if (!PlcMgrEx.GetInstance().m_dicReg_UintType.TryGetValue(strAddress, out type) ||
                            !PlcMgrEx.GetInstance().m_dicReg_StartAdress.TryGetValue(strAddress, out address))
                        {
                            string strInfo = $"寄存器 {strAddress} 不存在，请检查配置";
                            MessageBox.Show(strInfo, "配置错误");
                            return;
                        }

                        int[] regAddress = ConvertToIntAddress(type + address);
                        int[] registers = ModbusClient.ConvertStringToRegisters(value);
                        client.ModbusClient.WriteMultipleRegisters(regAddress[0], registers);
                        return;
                    }
                    catch (Exception ex)
                    {
                        _count++;
                        DisConnectPLC(plcSequence);
                        Thread.Sleep(100);
                        ConnectPLC(plcSequence);
                        Thread.Sleep(500);
                    }
                }
            }
        }



        public void WriteData_Float(int plcSequence, string strAddress, string value, ShowLogHandler1 ShowLog = null)
        {

        }

        //public void WriteTo3DStation(int plcSequence, WritePLC3D address, int value)
        //{
        //    int actualAddress = (int)address;
        //    WriteData_Int16(plcSequence, new int[] { actualAddress }, value);
        //}

        //public void WriteToUnLoadStation(int plcSequence, WritePLCUnLoad address, int value)
        //{
        //    int actualAddress = (int)address;
        //    WriteData_Int16(plcSequence, new int[] { actualAddress }, value);
        //}

        //public void WriteToUnLoad(int plcSequence, WritePLCUnLoad address, string value)
        //{
        //    int actualAddress = (int)address;
        //    WriteData_String(plcSequence, new int[] { actualAddress }, value);
        //}















        /// <summary>
        /// 从指定序号PLC读取整型数
        /// </summary>
        public int ReadData_Int16(int plcSequence, int[] regAddress, ShowLogHandler1 ShowLog = null)
        {
            lock (_plcLock)
            {
                if (false)
                {
                    return 1;
                }
                int _count = 0;
                while (true)
                {
                    if (_count > 10)
                    {
                        MessageBox.Show($"序号为{plcSequence}的PLC连接失败");
                        _count = 0;
                        return -1;
                    }
                    try
                    {
                        if (!_plcClients.TryGetValue(plcSequence, out var client) || !client.IsConnected)
                        {
                            MessageBox.Show($"序号为{plcSequence}的PLC未连接");
                            return -1;
                        }

                        int[] registers = client.ModbusClient.ReadHoldingRegisters(regAddress[0], 1);
                        return registers[0];
                    }
                    catch (Exception ex)
                    {
                        _count++;
                        DisConnectPLC(plcSequence);
                        Thread.Sleep(100);
                        ConnectPLC(plcSequence);
                        Thread.Sleep(500);
                    }
                }
            }
        }

        /// <summary>
        /// 向指定序号PLC写入整型数
        /// </summary>
        public void WriteData_Int16(int plcSequence, int[] regAddress, int value, ShowLogHandler1 ShowLog = null)
        {
            lock (_plcLock)
            {

                if (false)
                {
                    return;
                }
                int _count = 0;
                while (true)
                {
                    
                    if (_count > 10)
                    {
                        MessageBox.Show($"序号为{plcSequence}的PLC连接失败");
                        _count = 0;
                        return;
                    }
                    try
                    {
                        if (!_plcClients.TryGetValue(plcSequence, out var client) || !client.IsConnected)
                        {
                            MessageBox.Show($"序号为{plcSequence}的PLC未连接");
                            return;
                        }

                        int[] registers = ModbusClient.ConvertIntToRegisters(value);
                        if (registers.Length > 0 && registers[registers.Length - 1] == 0)
                        {
                            registers = registers.Take(registers.Count() - 1).ToArray();
                        }
                        client.ModbusClient.WriteMultipleRegisters(regAddress[0], registers);
                        return;
                    }
                    catch (Exception ex)
                    {
                        _count++;
                        DisConnectPLC(plcSequence);
                        Thread.Sleep(100);
                        ConnectPLC(plcSequence);
                        Thread.Sleep(500);
                    }
                }
            }
        }
        #endregion



       // #region //-------------------------------- 3D工位专用读取方法 --------------------------------//
        ///// <summary>
        ///// 读取3D工位（PLC序号1）单个寄存器值
        ///// </summary>
        //public bool Get3DValue(ReadPLC3D enumType, out int value)
        //{
        //    value = 0;
        //    lock (_plcLock)
        //    {
        //        // 确认3D PLC（序号1）已连接
        //        if (!_plcClients.TryGetValue(1, out var client) || !client.IsConnected)
        //            return false;

        //        int startAddr = int.Parse(client.StartAddress);
        //        int plcActualAddress = (int)enumType;
        //        int arrayIndex = plcActualAddress - startAddr;

        //        // 校验索引有效性
        //        if (client.RegisterValues != null
        //            && arrayIndex >= 0
        //            && arrayIndex < client.RegisterValues.Length)
        //        {
        //            value = client.RegisterValues[arrayIndex];
        //            return true;
        //        }
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 读取3D工位（PLC序号1）值并判断是否等于目标值
        ///// </summary>
        //public bool Read3DAndCheck(ReadPLC3D enumType, int targetValue)
        //{
        //    if (Get3DValue(enumType, out int actualValue))
        //    {
        //        return actualValue == targetValue;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 读取3D工位（PLC序号1）心跳寄存器的值
        ///// </summary>
        //public int Read3DHeartValue(ReadPLC3D enumType)
        //{
        //    if (Get3DValue(enumType, out int actualValue))
        //    {
        //        return actualValue;
        //    }
        //    return 0;
        //}

        ///// <summary>
        ///// 读取3D工位（PLC序号1）多个寄存器值
        ///// </summary>
        //public int[] GetMulti3DValues(ReadPLC3D enumType, int readCount)
        //{
        //    lock (_plcLock)
        //    {
        //        if (readCount <= 0)
        //            return null;

        //        // 确认3D PLC（序号1）已连接
        //        if (!_plcClients.TryGetValue(1, out var client) || !client.IsConnected)
        //            return null;

        //        int startAddr = int.Parse(client.StartAddress);
        //        int startActualAddr = (int)enumType;
        //        int startIndex = startActualAddr - startAddr;

        //        // 校验索引范围
        //        if (client.RegisterValues != null && startIndex >= 0 && startIndex + readCount <= client.RegisterValues.Length)
        //        {
        //            int[] values = new int[readCount];
        //            Array.Copy(client.RegisterValues, startIndex, values, 0, readCount);
        //            return values;
        //        }
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// 读取3D工位（PLC序号1）的SN号
        ///// </summary>
        //public string Read3DSN(ReadPLC3D startEnum)
        //{
        //    try
        //    {
        //        // 读取20个寄存器（与原逻辑一致）
        //        int[] snRegisters = GetMulti3DValues(startEnum, 20);
        //        if (snRegisters == null)
        //        {
        //            return null;
        //        }

        //        var snBuilder = new StringBuilder();
        //        foreach (int regValue in snRegisters)
        //        {
        //            char highChar = (char)((regValue >> 8) & 0xFF);
        //            char lowChar = (char)(regValue & 0xFF);
        //            snBuilder.Append(lowChar).Append(highChar);
        //        }
        //        //return snBuilder.ToString().Trim('\0');
        //        return snBuilder.ToString().Replace("\0", "");
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"读取3D PLC SN异常: {ex.Message}");
        //        return null;
        //    }
        //}
        //#endregion

        //#region //-------------------------------- 下料工位专用读取方法 --------------------------------//
        ///// <summary>
        ///// 读取下料PLC（序号2）单个寄存器值
        ///// </summary>
        //public bool GetUnLoadValue(ReadPLCUnload enumType, out int value)
        //{
        //    value = 0;
        //    lock (_plcLock)
        //    {
        //        // 确认下料PLC（序号2）已连接
        //        if (!_plcClients.TryGetValue(2, out var client) || !client.IsConnected)
        //            return false;

        //        int startAddr = int.Parse(client.StartAddress);
        //        int plcActualAddress = (int)enumType;
        //        int arrayIndex = plcActualAddress - startAddr;

        //        // 校验索引有效性
        //        if (client.RegisterValues != null
        //            && arrayIndex >= 0
        //            && arrayIndex < client.RegisterValues.Length)
        //        {
        //            value = client.RegisterValues[arrayIndex];
        //            return true;
        //        }
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 读取下料PLC（序号2）值并判断是否等于目标值
        ///// </summary>
        //public bool ReadUnLoadAndCheck(ReadPLCUnload enumType, int targetValue)
        //{
        //    if (GetUnLoadValue(enumType, out int actualValue))
        //    {
        //        return actualValue == targetValue;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// 读取下料PLC（序号2）多个寄存器值
        ///// </summary>
        //public int[] GetMultiUnLoadValues(ReadPLCUnload enumType, int readCount)
        //{
        //    lock (_plcLock)
        //    {
        //        if (readCount <= 0)
        //            return null;

        //        // 确认下料PLC（序号2）已连接
        //        if (!_plcClients.TryGetValue(2, out var client) || !client.IsConnected)
        //            return null;

        //        int startAddr = int.Parse(client.StartAddress);
        //        int startActualAddr = (int)enumType;
        //        int startIndex = startActualAddr - startAddr;

        //        // 校验索引范围
        //        if (client.RegisterValues != null && startIndex >= 0 && startIndex + readCount <= client.RegisterValues.Length)
        //        {
        //            int[] values = new int[readCount];
        //            Array.Copy(client.RegisterValues, startIndex, values, 0, readCount);
        //            return values;
        //        }
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// 读取下料PLC（序号2）的SN号
        ///// </summary>
        //public string ReadUnLoadSN(ReadPLCUnload startEnum)
        //{
        //    try
        //    {
        //        // 读取20个寄存器（与原逻辑一致）
        //        int[] snRegisters = GetMultiUnLoadValues(startEnum, 20);
        //        if (snRegisters == null)
        //        {
        //            return null;
        //        }

        //        var snBuilder = new StringBuilder();
        //        foreach (int regValue in snRegisters)
        //        {
        //            char highChar = (char)((regValue >> 8) & 0xFF);
        //            char lowChar = (char)(regValue & 0xFF);
        //            snBuilder.Append(lowChar).Append(highChar);
        //        }
        //        return snBuilder.ToString().Trim('\0');
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"读取下料PLC SN异常: {ex.Message}");
        //        return null;
        //    }
        //}








        //#endregion
    }
}
