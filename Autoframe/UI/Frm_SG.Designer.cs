namespace AutoFrame.UI
{
    partial class Frm_SG
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.btn_Open = new System.Windows.Forms.Button();
            this.btn_Close = new System.Windows.Forms.Button();
            this.txt_Ip = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btn_startcallbcak = new System.Windows.Forms.Button();
            this.btn_setcallback = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.cmb_HZ = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_SetHZ = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btn_setStorgePort = new System.Windows.Forms.Button();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.cmb_DataOutNo = new System.Windows.Forms.ComboBox();
            this.txt_GetDataNum = new System.Windows.Forms.TextBox();
            this.txt_SetStorageNum = new System.Windows.Forms.TextBox();
            this.btn_ClearStorage = new System.Windows.Forms.Button();
            this.btn_StartStorage = new System.Windows.Forms.Button();
            this.btn_GetStorageData2 = new System.Windows.Forms.Button();
            this.btn_StopStorage = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lab_OutNum4 = new System.Windows.Forms.Label();
            this.btn_SetStorageNum = new System.Windows.Forms.Button();
            this.btn_MonitorStorage = new System.Windows.Forms.Button();
            this.lab_OutNum3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lab_State = new System.Windows.Forms.Label();
            this.lab_OutNum2 = new System.Windows.Forms.Label();
            this.lab_OutNum1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cmb_OutNo = new System.Windows.Forms.ComboBox();
            this.btn_GetSingleOut = new System.Windows.Forms.Button();
            this.btn_GetAllOut = new System.Windows.Forms.Button();
            this.lab_RealVal4 = new System.Windows.Forms.Label();
            this.lab_RealVal3 = new System.Windows.Forms.Label();
            this.lab_RealVal2 = new System.Windows.Forms.Label();
            this.lab_RealVal = new System.Windows.Forms.Label();
            this.lab_RealVal1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btn_cont = new System.Windows.Forms.Button();
            this.btn_Time = new System.Windows.Forms.Button();
            this.btn_Tir = new System.Windows.Forms.Button();
            this.gb_Mode = new System.Windows.Forms.GroupBox();
            this.btn_ToCommunication = new System.Windows.Forms.Button();
            this.btn_ToNormal = new System.Windows.Forms.Button();
            this.gb_DevCon = new System.Windows.Forms.GroupBox();
            this.txt_Log = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.gb_Mode.SuspendLayout();
            this.gb_DevCon.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_Open
            // 
            this.btn_Open.Location = new System.Drawing.Point(274, 19);
            this.btn_Open.Name = "btn_Open";
            this.btn_Open.Size = new System.Drawing.Size(59, 23);
            this.btn_Open.TabIndex = 1;
            this.btn_Open.Text = "连接";
            this.btn_Open.UseVisualStyleBackColor = true;
            this.btn_Open.Click += new System.EventHandler(this.btn_Open_Click);
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(198, 19);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(59, 23);
            this.btn_Close.TabIndex = 1;
            this.btn_Close.Text = "断开";
            this.btn_Close.UseVisualStyleBackColor = true;
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // txt_Ip
            // 
            this.txt_Ip.Location = new System.Drawing.Point(57, 20);
            this.txt_Ip.Name = "txt_Ip";
            this.txt_Ip.Size = new System.Drawing.Size(135, 21);
            this.txt_Ip.TabIndex = 2;
            this.txt_Ip.Text = "192.168.0.10";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "IP地址:";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 500F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txt_Log, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1131, 622);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox5);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.gb_Mode);
            this.panel1.Controls.Add(this.gb_DevCon);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(631, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(500, 622);
            this.panel1.TabIndex = 0;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.button1);
            this.groupBox5.Controls.Add(this.btn_startcallbcak);
            this.groupBox5.Controls.Add(this.btn_setcallback);
            this.groupBox5.Location = new System.Drawing.Point(28, 493);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(460, 100);
            this.groupBox5.TabIndex = 14;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "回调";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(295, 31);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 10;
            this.button1.Text = "Test";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btn_startcallbcak
            // 
            this.btn_startcallbcak.Location = new System.Drawing.Point(94, 31);
            this.btn_startcallbcak.Name = "btn_startcallbcak";
            this.btn_startcallbcak.Size = new System.Drawing.Size(75, 23);
            this.btn_startcallbcak.TabIndex = 5;
            this.btn_startcallbcak.Text = "开始采集";
            this.btn_startcallbcak.UseVisualStyleBackColor = true;
            this.btn_startcallbcak.Click += new System.EventHandler(this.btn_startcallbcak_Click);
            // 
            // btn_setcallback
            // 
            this.btn_setcallback.Location = new System.Drawing.Point(14, 31);
            this.btn_setcallback.Name = "btn_setcallback";
            this.btn_setcallback.Size = new System.Drawing.Size(68, 23);
            this.btn_setcallback.TabIndex = 4;
            this.btn_setcallback.Text = "设置回调";
            this.btn_setcallback.UseVisualStyleBackColor = true;
            this.btn_setcallback.Click += new System.EventHandler(this.btn_setcallback_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.cmb_HZ);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.btn_SetHZ);
            this.groupBox4.Location = new System.Drawing.Point(28, 407);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(435, 66);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "采集参数";
            // 
            // cmb_HZ
            // 
            this.cmb_HZ.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_HZ.FormattingEnabled = true;
            this.cmb_HZ.Location = new System.Drawing.Point(88, 30);
            this.cmb_HZ.Name = "cmb_HZ";
            this.cmb_HZ.Size = new System.Drawing.Size(73, 20);
            this.cmb_HZ.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(23, 34);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "采样频率：";
            // 
            // btn_SetHZ
            // 
            this.btn_SetHZ.Location = new System.Drawing.Point(175, 30);
            this.btn_SetHZ.Name = "btn_SetHZ";
            this.btn_SetHZ.Size = new System.Drawing.Size(75, 23);
            this.btn_SetHZ.TabIndex = 9;
            this.btn_SetHZ.Text = "设置";
            this.btn_SetHZ.UseVisualStyleBackColor = true;
            this.btn_SetHZ.Click += new System.EventHandler(this.btn_SetHZ_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btn_setStorgePort);
            this.groupBox3.Controls.Add(this.checkBox4);
            this.groupBox3.Controls.Add(this.checkBox3);
            this.groupBox3.Controls.Add(this.checkBox2);
            this.groupBox3.Controls.Add(this.checkBox1);
            this.groupBox3.Controls.Add(this.cmb_DataOutNo);
            this.groupBox3.Controls.Add(this.txt_GetDataNum);
            this.groupBox3.Controls.Add(this.txt_SetStorageNum);
            this.groupBox3.Controls.Add(this.btn_ClearStorage);
            this.groupBox3.Controls.Add(this.btn_StartStorage);
            this.groupBox3.Controls.Add(this.btn_GetStorageData2);
            this.groupBox3.Controls.Add(this.btn_StopStorage);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.lab_OutNum4);
            this.groupBox3.Controls.Add(this.btn_SetStorageNum);
            this.groupBox3.Controls.Add(this.btn_MonitorStorage);
            this.groupBox3.Controls.Add(this.lab_OutNum3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.lab_State);
            this.groupBox3.Controls.Add(this.lab_OutNum2);
            this.groupBox3.Controls.Add(this.lab_OutNum1);
            this.groupBox3.Location = new System.Drawing.Point(28, 238);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(460, 163);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "数据存储器";
            // 
            // btn_setStorgePort
            // 
            this.btn_setStorgePort.Location = new System.Drawing.Point(326, 99);
            this.btn_setStorgePort.Name = "btn_setStorgePort";
            this.btn_setStorgePort.Size = new System.Drawing.Size(44, 23);
            this.btn_setStorgePort.TabIndex = 14;
            this.btn_setStorgePort.Text = "设置";
            this.btn_setStorgePort.UseVisualStyleBackColor = true;
            this.btn_setStorgePort.Click += new System.EventHandler(this.btn_setStorgePort_Click);
            // 
            // checkBox4
            // 
            this.checkBox4.Location = new System.Drawing.Point(254, 103);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(78, 16);
            this.checkBox4.TabIndex = 13;
            this.checkBox4.Text = "OUT4存储";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.Location = new System.Drawing.Point(173, 103);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(78, 16);
            this.checkBox3.TabIndex = 12;
            this.checkBox3.Text = "OUT3存储";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.Location = new System.Drawing.Point(94, 103);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(78, 16);
            this.checkBox2.TabIndex = 0;
            this.checkBox2.Text = "OUT2存储";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(13, 103);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(72, 16);
            this.checkBox1.TabIndex = 11;
            this.checkBox1.Text = "OUT1存储";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // cmb_DataOutNo
            // 
            this.cmb_DataOutNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_DataOutNo.FormattingEnabled = true;
            this.cmb_DataOutNo.Items.AddRange(new object[] {
            "OUT1",
            "OUT2",
            "OUT3",
            "OUT4"});
            this.cmb_DataOutNo.Location = new System.Drawing.Point(397, 38);
            this.cmb_DataOutNo.Name = "cmb_DataOutNo";
            this.cmb_DataOutNo.Size = new System.Drawing.Size(56, 20);
            this.cmb_DataOutNo.TabIndex = 10;
            // 
            // txt_GetDataNum
            // 
            this.txt_GetDataNum.Location = new System.Drawing.Point(397, 67);
            this.txt_GetDataNum.Name = "txt_GetDataNum";
            this.txt_GetDataNum.Size = new System.Drawing.Size(56, 21);
            this.txt_GetDataNum.TabIndex = 2;
            this.txt_GetDataNum.Text = "5000";
            // 
            // txt_SetStorageNum
            // 
            this.txt_SetStorageNum.Location = new System.Drawing.Point(56, 136);
            this.txt_SetStorageNum.Name = "txt_SetStorageNum";
            this.txt_SetStorageNum.Size = new System.Drawing.Size(85, 21);
            this.txt_SetStorageNum.TabIndex = 2;
            this.txt_SetStorageNum.Text = "5000";
            // 
            // btn_ClearStorage
            // 
            this.btn_ClearStorage.Location = new System.Drawing.Point(173, 38);
            this.btn_ClearStorage.Name = "btn_ClearStorage";
            this.btn_ClearStorage.Size = new System.Drawing.Size(75, 23);
            this.btn_ClearStorage.TabIndex = 9;
            this.btn_ClearStorage.Text = "清除";
            this.btn_ClearStorage.UseVisualStyleBackColor = true;
            this.btn_ClearStorage.Click += new System.EventHandler(this.btn_ClearStorage_Click);
            // 
            // btn_StartStorage
            // 
            this.btn_StartStorage.Location = new System.Drawing.Point(13, 38);
            this.btn_StartStorage.Name = "btn_StartStorage";
            this.btn_StartStorage.Size = new System.Drawing.Size(75, 23);
            this.btn_StartStorage.TabIndex = 9;
            this.btn_StartStorage.Text = "开始";
            this.btn_StartStorage.UseVisualStyleBackColor = true;
            this.btn_StartStorage.Click += new System.EventHandler(this.btn_StartStorage_Click);
            // 
            // btn_GetStorageData2
            // 
            this.btn_GetStorageData2.Location = new System.Drawing.Point(380, 100);
            this.btn_GetStorageData2.Name = "btn_GetStorageData2";
            this.btn_GetStorageData2.Size = new System.Drawing.Size(75, 50);
            this.btn_GetStorageData2.TabIndex = 9;
            this.btn_GetStorageData2.Text = "数据取出";
            this.btn_GetStorageData2.UseVisualStyleBackColor = true;
            this.btn_GetStorageData2.Click += new System.EventHandler(this.btn_GetStorageData2_Click);
            // 
            // btn_StopStorage
            // 
            this.btn_StopStorage.Location = new System.Drawing.Point(94, 38);
            this.btn_StopStorage.Name = "btn_StopStorage";
            this.btn_StopStorage.Size = new System.Drawing.Size(75, 23);
            this.btn_StopStorage.TabIndex = 9;
            this.btn_StopStorage.Text = "停止";
            this.btn_StopStorage.UseVisualStyleBackColor = true;
            this.btn_StopStorage.Click += new System.EventHandler(this.btn_StopStorage_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(363, 42);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 3;
            this.label5.Text = "端口";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(363, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 12);
            this.label4.TabIndex = 3;
            this.label4.Text = "数量";
            // 
            // lab_OutNum4
            // 
            this.lab_OutNum4.AutoSize = true;
            this.lab_OutNum4.Location = new System.Drawing.Point(279, 76);
            this.lab_OutNum4.Name = "lab_OutNum4";
            this.lab_OutNum4.Size = new System.Drawing.Size(23, 12);
            this.lab_OutNum4.TabIndex = 3;
            this.lab_OutNum4.Text = "NAN";
            // 
            // btn_SetStorageNum
            // 
            this.btn_SetStorageNum.Location = new System.Drawing.Point(153, 134);
            this.btn_SetStorageNum.Name = "btn_SetStorageNum";
            this.btn_SetStorageNum.Size = new System.Drawing.Size(75, 23);
            this.btn_SetStorageNum.TabIndex = 9;
            this.btn_SetStorageNum.Text = "设置";
            this.btn_SetStorageNum.UseVisualStyleBackColor = true;
            this.btn_SetStorageNum.Click += new System.EventHandler(this.btn_SetStorageNum_Click);
            // 
            // btn_MonitorStorage
            // 
            this.btn_MonitorStorage.Location = new System.Drawing.Point(254, 38);
            this.btn_MonitorStorage.Name = "btn_MonitorStorage";
            this.btn_MonitorStorage.Size = new System.Drawing.Size(75, 23);
            this.btn_MonitorStorage.TabIndex = 9;
            this.btn_MonitorStorage.Text = "监控";
            this.btn_MonitorStorage.UseVisualStyleBackColor = true;
            this.btn_MonitorStorage.Click += new System.EventHandler(this.btn_MonitorStorage_Click);
            // 
            // lab_OutNum3
            // 
            this.lab_OutNum3.AutoSize = true;
            this.lab_OutNum3.Location = new System.Drawing.Point(226, 76);
            this.lab_OutNum3.Name = "lab_OutNum3";
            this.lab_OutNum3.Size = new System.Drawing.Size(23, 12);
            this.lab_OutNum3.TabIndex = 3;
            this.lab_OutNum3.Text = "NAN";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "数量：";
            // 
            // lab_State
            // 
            this.lab_State.AutoSize = true;
            this.lab_State.Location = new System.Drawing.Point(39, 76);
            this.lab_State.Name = "lab_State";
            this.lab_State.Size = new System.Drawing.Size(23, 12);
            this.lab_State.TabIndex = 3;
            this.lab_State.Text = "NAN";
            // 
            // lab_OutNum2
            // 
            this.lab_OutNum2.AutoSize = true;
            this.lab_OutNum2.Location = new System.Drawing.Point(173, 76);
            this.lab_OutNum2.Name = "lab_OutNum2";
            this.lab_OutNum2.Size = new System.Drawing.Size(23, 12);
            this.lab_OutNum2.TabIndex = 3;
            this.lab_OutNum2.Text = "NAN";
            // 
            // lab_OutNum1
            // 
            this.lab_OutNum1.AutoSize = true;
            this.lab_OutNum1.Location = new System.Drawing.Point(120, 76);
            this.lab_OutNum1.Name = "lab_OutNum1";
            this.lab_OutNum1.Size = new System.Drawing.Size(23, 12);
            this.lab_OutNum1.TabIndex = 3;
            this.lab_OutNum1.Text = "NAN";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cmb_OutNo);
            this.groupBox2.Controls.Add(this.btn_GetSingleOut);
            this.groupBox2.Controls.Add(this.btn_GetAllOut);
            this.groupBox2.Controls.Add(this.lab_RealVal4);
            this.groupBox2.Controls.Add(this.lab_RealVal3);
            this.groupBox2.Controls.Add(this.lab_RealVal2);
            this.groupBox2.Controls.Add(this.lab_RealVal);
            this.groupBox2.Controls.Add(this.lab_RealVal1);
            this.groupBox2.Location = new System.Drawing.Point(28, 146);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(460, 86);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "实时数据";
            // 
            // cmb_OutNo
            // 
            this.cmb_OutNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_OutNo.FormattingEnabled = true;
            this.cmb_OutNo.Items.AddRange(new object[] {
            "OUT1",
            "OUT2",
            "OUT3",
            "OUT4"});
            this.cmb_OutNo.Location = new System.Drawing.Point(247, 51);
            this.cmb_OutNo.Name = "cmb_OutNo";
            this.cmb_OutNo.Size = new System.Drawing.Size(56, 20);
            this.cmb_OutNo.TabIndex = 10;
            // 
            // btn_GetSingleOut
            // 
            this.btn_GetSingleOut.Location = new System.Drawing.Point(320, 51);
            this.btn_GetSingleOut.Name = "btn_GetSingleOut";
            this.btn_GetSingleOut.Size = new System.Drawing.Size(75, 23);
            this.btn_GetSingleOut.TabIndex = 9;
            this.btn_GetSingleOut.Text = "单一获取";
            this.btn_GetSingleOut.UseVisualStyleBackColor = true;
            this.btn_GetSingleOut.Click += new System.EventHandler(this.btn_GetSingleOut_Click);
            // 
            // btn_GetAllOut
            // 
            this.btn_GetAllOut.Location = new System.Drawing.Point(320, 20);
            this.btn_GetAllOut.Name = "btn_GetAllOut";
            this.btn_GetAllOut.Size = new System.Drawing.Size(75, 23);
            this.btn_GetAllOut.TabIndex = 9;
            this.btn_GetAllOut.Text = "全部获取";
            this.btn_GetAllOut.UseVisualStyleBackColor = true;
            this.btn_GetAllOut.Click += new System.EventHandler(this.btn_GetAllOut_Click);
            // 
            // lab_RealVal4
            // 
            this.lab_RealVal4.AutoSize = true;
            this.lab_RealVal4.Location = new System.Drawing.Point(266, 25);
            this.lab_RealVal4.Name = "lab_RealVal4";
            this.lab_RealVal4.Size = new System.Drawing.Size(23, 12);
            this.lab_RealVal4.TabIndex = 3;
            this.lab_RealVal4.Text = "NAN";
            // 
            // lab_RealVal3
            // 
            this.lab_RealVal3.AutoSize = true;
            this.lab_RealVal3.Location = new System.Drawing.Point(202, 25);
            this.lab_RealVal3.Name = "lab_RealVal3";
            this.lab_RealVal3.Size = new System.Drawing.Size(23, 12);
            this.lab_RealVal3.TabIndex = 3;
            this.lab_RealVal3.Text = "NAN";
            // 
            // lab_RealVal2
            // 
            this.lab_RealVal2.AutoSize = true;
            this.lab_RealVal2.Location = new System.Drawing.Point(138, 25);
            this.lab_RealVal2.Name = "lab_RealVal2";
            this.lab_RealVal2.Size = new System.Drawing.Size(23, 12);
            this.lab_RealVal2.TabIndex = 3;
            this.lab_RealVal2.Text = "NAN";
            // 
            // lab_RealVal
            // 
            this.lab_RealVal.AutoSize = true;
            this.lab_RealVal.Location = new System.Drawing.Point(138, 55);
            this.lab_RealVal.Name = "lab_RealVal";
            this.lab_RealVal.Size = new System.Drawing.Size(23, 12);
            this.lab_RealVal.TabIndex = 3;
            this.lab_RealVal.Text = "NAN";
            // 
            // lab_RealVal1
            // 
            this.lab_RealVal1.AutoSize = true;
            this.lab_RealVal1.Location = new System.Drawing.Point(74, 25);
            this.lab_RealVal1.Name = "lab_RealVal1";
            this.lab_RealVal1.Size = new System.Drawing.Size(23, 12);
            this.lab_RealVal1.TabIndex = 3;
            this.lab_RealVal1.Text = "NAN";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btn_cont);
            this.groupBox1.Controls.Add(this.btn_Time);
            this.groupBox1.Controls.Add(this.btn_Tir);
            this.groupBox1.Location = new System.Drawing.Point(238, 73);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(225, 54);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "触发模式";
            // 
            // btn_cont
            // 
            this.btn_cont.Location = new System.Drawing.Point(52, 20);
            this.btn_cont.Name = "btn_cont";
            this.btn_cont.Size = new System.Drawing.Size(43, 23);
            this.btn_cont.TabIndex = 5;
            this.btn_cont.Text = "OFF";
            this.btn_cont.UseVisualStyleBackColor = true;
            this.btn_cont.Click += new System.EventHandler(this.btn_cont_Click);
            // 
            // btn_Time
            // 
            this.btn_Time.Location = new System.Drawing.Point(100, 20);
            this.btn_Time.Name = "btn_Time";
            this.btn_Time.Size = new System.Drawing.Size(43, 23);
            this.btn_Time.TabIndex = 5;
            this.btn_Time.Text = "计时";
            this.btn_Time.UseVisualStyleBackColor = true;
            this.btn_Time.Click += new System.EventHandler(this.btn_Time_Click);
            // 
            // btn_Tir
            // 
            this.btn_Tir.Location = new System.Drawing.Point(148, 20);
            this.btn_Tir.Name = "btn_Tir";
            this.btn_Tir.Size = new System.Drawing.Size(43, 23);
            this.btn_Tir.TabIndex = 5;
            this.btn_Tir.Text = "触发";
            this.btn_Tir.UseVisualStyleBackColor = true;
            this.btn_Tir.Click += new System.EventHandler(this.btn_Tir_Click);
            // 
            // gb_Mode
            // 
            this.gb_Mode.Controls.Add(this.btn_ToCommunication);
            this.gb_Mode.Controls.Add(this.btn_ToNormal);
            this.gb_Mode.Location = new System.Drawing.Point(28, 73);
            this.gb_Mode.Name = "gb_Mode";
            this.gb_Mode.Size = new System.Drawing.Size(202, 54);
            this.gb_Mode.TabIndex = 7;
            this.gb_Mode.TabStop = false;
            this.gb_Mode.Text = "模式切换";
            // 
            // btn_ToCommunication
            // 
            this.btn_ToCommunication.Location = new System.Drawing.Point(128, 20);
            this.btn_ToCommunication.Name = "btn_ToCommunication";
            this.btn_ToCommunication.Size = new System.Drawing.Size(64, 23);
            this.btn_ToCommunication.TabIndex = 4;
            this.btn_ToCommunication.Text = "通信模式";
            this.btn_ToCommunication.UseVisualStyleBackColor = true;
            this.btn_ToCommunication.Click += new System.EventHandler(this.btn_ToCommunication_Click);
            // 
            // btn_ToNormal
            // 
            this.btn_ToNormal.Location = new System.Drawing.Point(52, 20);
            this.btn_ToNormal.Name = "btn_ToNormal";
            this.btn_ToNormal.Size = new System.Drawing.Size(64, 23);
            this.btn_ToNormal.TabIndex = 4;
            this.btn_ToNormal.Text = "一般模式";
            this.btn_ToNormal.UseVisualStyleBackColor = true;
            this.btn_ToNormal.Click += new System.EventHandler(this.btn_ToNormal_Click);
            // 
            // gb_DevCon
            // 
            this.gb_DevCon.Controls.Add(this.txt_Ip);
            this.gb_DevCon.Controls.Add(this.btn_Open);
            this.gb_DevCon.Controls.Add(this.label1);
            this.gb_DevCon.Controls.Add(this.btn_Close);
            this.gb_DevCon.Location = new System.Drawing.Point(28, 12);
            this.gb_DevCon.Name = "gb_DevCon";
            this.gb_DevCon.Size = new System.Drawing.Size(435, 55);
            this.gb_DevCon.TabIndex = 6;
            this.gb_DevCon.TabStop = false;
            this.gb_DevCon.Text = "设备连接";
            // 
            // txt_Log
            // 
            this.txt_Log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txt_Log.Location = new System.Drawing.Point(3, 3);
            this.txt_Log.Multiline = true;
            this.txt_Log.Name = "txt_Log";
            this.txt_Log.ReadOnly = true;
            this.txt_Log.Size = new System.Drawing.Size(625, 616);
            this.txt_Log.TabIndex = 1;
            // 
            // Frm_SG
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1131, 622);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Frm_SG";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "深视点激光";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.gb_Mode.ResumeLayout(false);
            this.gb_DevCon.ResumeLayout(false);
            this.gb_DevCon.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_Open;
        private System.Windows.Forms.Button btn_Close;
        private System.Windows.Forms.TextBox txt_Ip;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btn_ToNormal;
        private System.Windows.Forms.Button btn_ToCommunication;
        private System.Windows.Forms.Button btn_Tir;
        private System.Windows.Forms.Button btn_Time;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btn_cont;
        private System.Windows.Forms.GroupBox gb_Mode;
        private System.Windows.Forms.GroupBox gb_DevCon;
        private System.Windows.Forms.Button btn_GetAllOut;
        private System.Windows.Forms.Button btn_GetStorageData2;
        private System.Windows.Forms.Button btn_MonitorStorage;
        private System.Windows.Forms.Button btn_StartStorage;
        private System.Windows.Forms.Button btn_StopStorage;
        private System.Windows.Forms.Button btn_ClearStorage;
        private System.Windows.Forms.Label lab_RealVal1;
        private System.Windows.Forms.Label lab_OutNum4;
        private System.Windows.Forms.Label lab_OutNum3;
        private System.Windows.Forms.Label lab_OutNum2;
        private System.Windows.Forms.Label lab_OutNum1;
        private System.Windows.Forms.Label lab_State;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lab_RealVal4;
        private System.Windows.Forms.Label lab_RealVal3;
        private System.Windows.Forms.Label lab_RealVal2;
        private System.Windows.Forms.ComboBox cmb_OutNo;
        private System.Windows.Forms.Button btn_GetSingleOut;
        private System.Windows.Forms.Label lab_RealVal;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btn_SetStorageNum;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmb_HZ;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_SetHZ;
        private System.Windows.Forms.TextBox txt_SetStorageNum;
        private System.Windows.Forms.ComboBox cmb_DataOutNo;
        private System.Windows.Forms.TextBox txt_GetDataNum;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btn_setcallback;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btn_startcallbcak;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button btn_setStorgePort;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txt_Log;
    }
}

