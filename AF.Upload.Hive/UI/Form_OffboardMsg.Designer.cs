namespace AF.Upload.Hive
{
    partial class Form_OffboardMsg
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
            this.components = new System.ComponentModel.Container();
            this.Button1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dualLineLabel1 = new AF.Upload.Hive.DualLineLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Button1
            // 
            this.Button1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Button1.BackColor = System.Drawing.Color.DarkGray;
            this.Button1.FlatAppearance.BorderSize = 0;
            this.Button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button1.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.Button1.Location = new System.Drawing.Point(74, 130);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(330, 46);
            this.Button1.TabIndex = 4;
            this.Button1.Text = "发送最新信息 / Send most recent message";
            this.Button1.UseVisualStyleBackColor = false;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.SlateBlue;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Controls.Add(this.dualLineLabel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Button1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(478, 204);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // dualLineLabel1
            // 
            this.dualLineLabel1.AutoSize = true;
            this.dualLineLabel1.BackColor = System.Drawing.Color.Transparent;
            this.dualLineLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dualLineLabel1.Line1Color = System.Drawing.Color.White;
            this.dualLineLabel1.Line1Text = "请检查Hive网络连接或者重新注册";
            this.dualLineLabel1.Line2Color = System.Drawing.Color.White;
            this.dualLineLabel1.Line2Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.dualLineLabel1.Line2Text = "Pls check the Hive network connection or re-onboard";
            this.dualLineLabel1.Location = new System.Drawing.Point(2, 0);
            this.dualLineLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.dualLineLabel1.Name = "dualLineLabel1";
            this.dualLineLabel1.Size = new System.Drawing.Size(474, 102);
            this.dualLineLabel1.TabIndex = 5;
            this.dualLineLabel1.Text = "请检查Hive网络连接或者重新注册";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form_OffboardMsg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 204);
            this.ControlBox = false;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form_OffboardMsg";
            this.Text = " ";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_OperationMsg_FormClosed);
            this.Load += new System.EventHandler(this.Form_OperationMsg_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.Button Button1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DualLineLabel dualLineLabel1;
        private System.Windows.Forms.Timer timer1;
    }
}