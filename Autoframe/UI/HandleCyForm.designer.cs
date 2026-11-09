namespace AutoFrame.UI
{
    partial class HandleCyForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.switch1 = new AntdUI.Switch();
            this.switch2 = new AntdUI.Switch();
            this.switch3 = new AntdUI.Switch();
            this.switch4 = new AntdUI.Switch();
            this.switch5 = new AntdUI.Switch();
            this.switch6 = new AntdUI.Switch();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.switch6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.switch5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.switch4, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.switch3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.switch2, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.switch1, 0, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(36, 79);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(225, 244);
            this.tableLayoutPanel1.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(36, 28);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(225, 45);
            this.panel1.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(225, 45);
            this.label1.TabIndex = 0;
            this.label1.Text = "下料气缸调试";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // switch1
            // 
            this.switch1.CheckedText = "A上料解锁一起伸出";
            this.switch1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.switch1.Location = new System.Drawing.Point(3, 3);
            this.switch1.Name = "switch1";
            this.switch1.Size = new System.Drawing.Size(219, 34);
            this.switch1.TabIndex = 0;
            this.switch1.Text = "switch1";
            this.switch1.UnCheckedText = "A上料解锁一起缩回";
            this.switch1.CheckedChanged += new AntdUI.BoolEventHandler(this.switch1_CheckedChanged);
            // 
            // switch2
            // 
            this.switch2.CheckedText = "B上料解锁一起伸出";
            this.switch2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.switch2.Location = new System.Drawing.Point(3, 43);
            this.switch2.Name = "switch2";
            this.switch2.Size = new System.Drawing.Size(219, 34);
            this.switch2.TabIndex = 1;
            this.switch2.Text = "switch2";
            this.switch2.UnCheckedText = "B上料解锁一起缩回";
            this.switch2.CheckedChanged += new AntdUI.BoolEventHandler(this.switch2_CheckedChanged);
            // 
            // switch3
            // 
            this.switch3.CheckedText = "C上料解锁一起伸出";
            this.switch3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.switch3.Location = new System.Drawing.Point(3, 83);
            this.switch3.Name = "switch3";
            this.switch3.Size = new System.Drawing.Size(219, 34);
            this.switch3.TabIndex = 2;
            this.switch3.Text = "switch3";
            this.switch3.UnCheckedText = "C上料解锁一起缩回";
            this.switch3.CheckedChanged += new AntdUI.BoolEventHandler(this.switch3_CheckedChanged);
            // 
            // switch4
            // 
            this.switch4.CheckedText = "A下料解锁一起伸出";
            this.switch4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.switch4.Location = new System.Drawing.Point(3, 123);
            this.switch4.Name = "switch4";
            this.switch4.Size = new System.Drawing.Size(219, 34);
            this.switch4.TabIndex = 3;
            this.switch4.Text = "switch4";
            this.switch4.UnCheckedText = "A下料解锁一起缩回";
            this.switch4.CheckedChanged += new AntdUI.BoolEventHandler(this.switch4_CheckedChanged);
            // 
            // switch5
            // 
            this.switch5.CheckedText = "B下料解锁一起伸出";
            this.switch5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.switch5.Location = new System.Drawing.Point(3, 163);
            this.switch5.Name = "switch5";
            this.switch5.Size = new System.Drawing.Size(219, 34);
            this.switch5.TabIndex = 4;
            this.switch5.Text = "switch5";
            this.switch5.UnCheckedText = "B下料解锁一起缩回";
            this.switch5.CheckedChanged += new AntdUI.BoolEventHandler(this.switch5_CheckedChanged);
            // 
            // switch6
            // 
            this.switch6.CheckedText = "C下料解锁一起伸出";
            this.switch6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.switch6.Location = new System.Drawing.Point(3, 203);
            this.switch6.Name = "switch6";
            this.switch6.Size = new System.Drawing.Size(219, 38);
            this.switch6.TabIndex = 5;
            this.switch6.Text = "switch6";
            this.switch6.UnCheckedText = "C下料解锁一起缩回";
            this.switch6.CheckedChanged += new AntdUI.BoolEventHandler(this.switch6_CheckedChanged);
            // 
            // HandleCyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "HandleCyForm";
            this.Text = "HandleCyForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private AntdUI.Switch switch3;
        private AntdUI.Switch switch2;
        private AntdUI.Switch switch1;
        private System.Windows.Forms.Label label1;
        private AntdUI.Switch switch6;
        private AntdUI.Switch switch5;
        private AntdUI.Switch switch4;
    }
}