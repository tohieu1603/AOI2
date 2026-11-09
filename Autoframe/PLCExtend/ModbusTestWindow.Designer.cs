namespace AutoFrame.PLCExtend
{
    partial class ModbusTestWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.txtSingleRegister = new System.Windows.Forms.TextBox();
            this.txtSingleReadRegister = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtWriteValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblReadValue = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(65, 56);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "连接PLC";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(223, 216);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(176, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "写入单个寄存器的值";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // txtSingleRegister
            // 
            this.txtSingleRegister.Location = new System.Drawing.Point(223, 177);
            this.txtSingleRegister.Name = "txtSingleRegister";
            this.txtSingleRegister.Size = new System.Drawing.Size(170, 21);
            this.txtSingleRegister.TabIndex = 2;
            // 
            // txtSingleReadRegister
            // 
            this.txtSingleReadRegister.Location = new System.Drawing.Point(223, 366);
            this.txtSingleReadRegister.Name = "txtSingleReadRegister";
            this.txtSingleReadRegister.Size = new System.Drawing.Size(170, 21);
            this.txtSingleReadRegister.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(217, 454);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(176, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "读取单个寄存器的值";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(195, 56);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(75, 23);
            this.button4.TabIndex = 5;
            this.button4.Text = "断开PLC连接";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(38, 366);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(179, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "输入要读取的寄存器的值(整数):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(38, 137);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(179, 12);
            this.label2.TabIndex = 7;
            this.label2.Text = "输入要写入的寄存的地址(整数):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(167, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "输入要写入寄存器的值(整数):";
            // 
            // txtWriteValue
            // 
            this.txtWriteValue.Location = new System.Drawing.Point(223, 134);
            this.txtWriteValue.Name = "txtWriteValue";
            this.txtWriteValue.Size = new System.Drawing.Size(170, 21);
            this.txtWriteValue.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(38, 414);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(143, 12);
            this.label4.TabIndex = 10;
            this.label4.Text = "显示读取到的寄存器的值:";
            // 
            // lblReadValue
            // 
            this.lblReadValue.AutoSize = true;
            this.lblReadValue.Location = new System.Drawing.Point(221, 414);
            this.lblReadValue.Name = "lblReadValue";
            this.lblReadValue.Size = new System.Drawing.Size(47, 12);
            this.lblReadValue.TabIndex = 11;
            this.lblReadValue.Text = "unknown";
            // 
            // ModbusTestWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1130, 572);
            this.Controls.Add(this.lblReadValue);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtWriteValue);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.txtSingleReadRegister);
            this.Controls.Add(this.txtSingleRegister);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "ModbusTestWindow";
            this.Text = "ModbusTestWindow";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox txtSingleRegister;
        private System.Windows.Forms.TextBox txtSingleReadRegister;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtWriteValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblReadValue;
    }
}