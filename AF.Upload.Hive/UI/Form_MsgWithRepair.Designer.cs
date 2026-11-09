namespace AF.Upload.Hive
{
    partial class Form_MsgWithRepair
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
            this.TableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lineControl1 = new AF.Upload.Hive.LineControl();
            this.Label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.Button_Pause = new AF.Upload.Hive.MultiLineButton();
            this.Button_Stop = new AF.Upload.Hive.MultiLineButton();
            this.TableLayoutPanel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // TableLayoutPanel1
            // 
            this.TableLayoutPanel1.ColumnCount = 1;
            this.TableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.TableLayoutPanel1.Controls.Add(this.panel2, 0, 0);
            this.TableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.TableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TableLayoutPanel1.Location = new System.Drawing.Point(1, 26);
            this.TableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.TableLayoutPanel1.Name = "TableLayoutPanel1";
            this.TableLayoutPanel1.RowCount = 3;
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55.55556F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22.22222F));
            this.TableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.TableLayoutPanel1.Size = new System.Drawing.Size(898, 383);
            this.TableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Location = new System.Drawing.Point(0, 85);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(898, 212);
            this.panel1.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.lineControl1);
            this.panel2.Controls.Add(this.Label2);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(898, 85);
            this.panel2.TabIndex = 5;
            // 
            // lineControl1
            // 
            this.lineControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lineControl1.LineLength = 868;
            this.lineControl1.LineThickness = 3;
            this.lineControl1.Location = new System.Drawing.Point(0, 50);
            this.lineControl1.Margin = new System.Windows.Forms.Padding(0);
            this.lineControl1.Name = "lineControl1";
            this.lineControl1.Padding = new System.Windows.Forms.Padding(15);
            this.lineControl1.Size = new System.Drawing.Size(898, 35);
            this.lineControl1.TabIndex = 2;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Label2.Location = new System.Drawing.Point(292, 19);
            this.Label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(340, 31);
            this.Label2.TabIndex = 1;
            this.Label2.Text = "设备已停机 Machine stopped";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.Button_Pause, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.Button_Stop, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 300);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(892, 80);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // Button_Pause
            // 
            this.Button_Pause.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Button_Pause.BackColor = System.Drawing.Color.DarkGray;
            this.Button_Pause.FlatAppearance.BorderSize = 0;
            this.Button_Pause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Pause.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Button_Pause.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Button_Pause.ImageIndex = 1;
            this.Button_Pause.ImagePadding = 5;
            this.Button_Pause.LeftImage = global::AF.Upload.Hive.Properties.Resources.Pause;
            this.Button_Pause.Line1 = "暂停 / Pause ";
            this.Button_Pause.Line1Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.Button_Pause.Line2Font = new System.Drawing.Font("微软雅黑", 1F, System.Drawing.FontStyle.Bold);
            this.Button_Pause.Location = new System.Drawing.Point(75, 4);
            this.Button_Pause.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Button_Pause.Name = "Button_Pause";
            this.Button_Pause.Size = new System.Drawing.Size(296, 72);
            this.Button_Pause.TabIndex = 5;
            this.Button_Pause.Text = "暂停设备，继续维修\r\nPause machine, continue to repair\r\n";
            this.Button_Pause.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Button_Pause.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.Button_Pause.TextPadding = 1;
            this.Button_Pause.UseVisualStyleBackColor = false;
            this.Button_Pause.Click += new System.EventHandler(this.Button_Pause_Click);
            // 
            // Button_Stop
            // 
            this.Button_Stop.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.Button_Stop.BackColor = System.Drawing.Color.DarkGray;
            this.Button_Stop.FlatAppearance.BorderSize = 0;
            this.Button_Stop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Button_Stop.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Button_Stop.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Button_Stop.ImageIndex = 2;
            this.Button_Stop.ImagePadding = 5;
            this.Button_Stop.LeftImage = global::AF.Upload.Hive.Properties.Resources.Stop;
            this.Button_Stop.Line1 = "停止 / Stop";
            this.Button_Stop.Line1Font = new System.Drawing.Font("微软雅黑", 15F, System.Drawing.FontStyle.Bold);
            this.Button_Stop.Line2Font = new System.Drawing.Font("微软雅黑", 1F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Button_Stop.Location = new System.Drawing.Point(517, 4);
            this.Button_Stop.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Button_Stop.Name = "Button_Stop";
            this.Button_Stop.Size = new System.Drawing.Size(304, 72);
            this.Button_Stop.TabIndex = 4;
            this.Button_Stop.Text = "停止设备，继续维修\r\nStop machine, continue to repair\r\n";
            this.Button_Stop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.Button_Stop.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            this.Button_Stop.TextPadding = 1;
            this.Button_Stop.UseVisualStyleBackColor = false;
            this.Button_Stop.Click += new System.EventHandler(this.Button_Stop_Click);
            // 
            // Form_MsgWithRepair
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 410);
            this.ControlBox = false;
            this.Controls.Add(this.TableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.Name = "Form_MsgWithRepair";
            this.ShowCloseButton = false;
            this.Text = "维修/Troubleshooting SOP";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_MsgWithRepair_FormClosed);
            this.Load += new System.EventHandler(this.Form_MsgWithRepair_Load);
            this.Controls.SetChildIndex(this.TableLayoutPanel1, 0);
            this.TableLayoutPanel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        internal System.Windows.Forms.TableLayoutPanel TableLayoutPanel1;
        internal System.Windows.Forms.Label Label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private LineControl lineControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        internal MultiLineButton Button_Stop;
        internal MultiLineButton Button_Pause;
    }
}