namespace AF.Upload.Hive
{
    partial class Form_SafetyStatusMonitor
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.Panel errorContainer;
        private System.Windows.Forms.TableLayoutPanel errorTable;
        private System.Windows.Forms.Label lblCode;
        private System.Windows.Forms.Label lblMsg;
        private System.Windows.Forms.Panel scrollPanel;
        private System.Windows.Forms.Panel buttonPanel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelMain = new System.Windows.Forms.Panel();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.errorContainer = new System.Windows.Forms.Panel();
            this.errorTable = new System.Windows.Forms.TableLayoutPanel();
            this.TextBox_Errormessage = new AF.Upload.Hive.BlackBorderTextBox();
            this.TextBox_Errorcode = new AF.Upload.Hive.BlackBorderTextBox();
            this.lblCode = new System.Windows.Forms.Label();
            this.lblMsg = new System.Windows.Forms.Label();
            this.scrollPanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.button_OK = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            this.mainLayout.SuspendLayout();
            this.errorContainer.SuspendLayout();
            this.errorTable.SuspendLayout();
            this.scrollPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.AutoScroll = true;
            this.panelMain.BackColor = System.Drawing.Color.LightCoral;
            this.panelMain.Controls.Add(this.mainLayout);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.panelMain.Size = new System.Drawing.Size(514, 355);
            this.panelMain.TabIndex = 0;
            // 
            // mainLayout
            // 
            this.mainLayout.ColumnCount = 1;
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.Controls.Add(this.errorContainer, 0, 0);
            this.mainLayout.Controls.Add(this.scrollPanel, 0, 1);
            this.mainLayout.Controls.Add(this.buttonPanel, 0, 2);
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.Location = new System.Drawing.Point(8, 8);
            this.mainLayout.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.RowCount = 3;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 67F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainLayout.Size = new System.Drawing.Size(498, 339);
            this.mainLayout.TabIndex = 0;
            // 
            // errorContainer
            // 
            this.errorContainer.BackColor = System.Drawing.Color.Transparent;
            this.errorContainer.Controls.Add(this.errorTable);
            this.errorContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorContainer.Location = new System.Drawing.Point(0, 0);
            this.errorContainer.Margin = new System.Windows.Forms.Padding(0);
            this.errorContainer.Name = "errorContainer";
            this.errorContainer.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.errorContainer.Size = new System.Drawing.Size(498, 67);
            this.errorContainer.TabIndex = 0;
            // 
            // errorTable
            // 
            this.errorTable.BackColor = System.Drawing.Color.Transparent;
            this.errorTable.ColumnCount = 2;
            this.errorTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.errorTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.errorTable.Controls.Add(this.TextBox_Errormessage, 1, 1);
            this.errorTable.Controls.Add(this.TextBox_Errorcode, 1, 0);
            this.errorTable.Controls.Add(this.lblCode, 0, 0);
            this.errorTable.Controls.Add(this.lblMsg, 0, 1);
            this.errorTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorTable.Location = new System.Drawing.Point(4, 4);
            this.errorTable.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.errorTable.Name = "errorTable";
            this.errorTable.RowCount = 2;
            this.errorTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.errorTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.errorTable.Size = new System.Drawing.Size(490, 59);
            this.errorTable.TabIndex = 0;
            // 
            // TextBox_Errormessage
            // 
            this.TextBox_Errormessage.BackColor = System.Drawing.Color.White;
            this.TextBox_Errormessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBox_Errormessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBox_Errormessage.Font = new System.Drawing.Font("微软雅黑", 7F);
            this.TextBox_Errormessage.Location = new System.Drawing.Point(190, 29);
            this.TextBox_Errormessage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextBox_Errormessage.Name = "TextBox_Errormessage";
            this.TextBox_Errormessage.ReadOnly = true;
            this.TextBox_Errormessage.Size = new System.Drawing.Size(298, 23);
            this.TextBox_Errormessage.TabIndex = 6;
            // 
            // TextBox_Errorcode
            // 
            this.TextBox_Errorcode.BackColor = System.Drawing.Color.White;
            this.TextBox_Errorcode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TextBox_Errorcode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TextBox_Errorcode.Font = new System.Drawing.Font("微软雅黑", 7F);
            this.TextBox_Errorcode.Location = new System.Drawing.Point(190, 2);
            this.TextBox_Errorcode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.TextBox_Errorcode.Name = "TextBox_Errorcode";
            this.TextBox_Errorcode.ReadOnly = true;
            this.TextBox_Errorcode.Size = new System.Drawing.Size(298, 23);
            this.TextBox_Errorcode.TabIndex = 5;
            // 
            // lblCode
            // 
            this.lblCode.AutoSize = true;
            this.lblCode.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lblCode.ForeColor = System.Drawing.Color.Black;
            this.lblCode.Location = new System.Drawing.Point(2, 0);
            this.lblCode.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCode.Name = "lblCode";
            this.lblCode.Size = new System.Drawing.Size(155, 19);
            this.lblCode.TabIndex = 0;
            this.lblCode.Text = "故障代码 Error code:";
            this.lblCode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMsg
            // 
            this.lblMsg.AutoSize = true;
            this.lblMsg.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold);
            this.lblMsg.ForeColor = System.Drawing.Color.Black;
            this.lblMsg.Location = new System.Drawing.Point(2, 27);
            this.lblMsg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMsg.Name = "lblMsg";
            this.lblMsg.Size = new System.Drawing.Size(184, 19);
            this.lblMsg.TabIndex = 2;
            this.lblMsg.Text = "故障信息 Error message:";
            this.lblMsg.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // scrollPanel
            // 
            this.scrollPanel.AutoScroll = true;
            this.scrollPanel.BackColor = System.Drawing.Color.Transparent;
            this.scrollPanel.Controls.Add(this.tableLayoutPanel1);
            this.scrollPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollPanel.Location = new System.Drawing.Point(0, 67);
            this.scrollPanel.Margin = new System.Windows.Forms.Padding(0);
            this.scrollPanel.Name = "scrollPanel";
            this.scrollPanel.Size = new System.Drawing.Size(498, 222);
            this.scrollPanel.TabIndex = 1;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Size = new System.Drawing.Size(498, 0);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // buttonPanel
            // 
            this.buttonPanel.BackColor = System.Drawing.Color.Transparent;
            this.buttonPanel.Controls.Add(this.button_OK);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.Location = new System.Drawing.Point(0, 289);
            this.buttonPanel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(498, 50);
            this.buttonPanel.TabIndex = 2;
            // 
            // button_OK
            // 
            this.button_OK.BackColor = System.Drawing.Color.LightGray;
            this.button_OK.FlatAppearance.BorderSize = 0;
            this.button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OK.Location = new System.Drawing.Point(171, 9);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(178, 38);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确认 / OK";
            this.button_OK.UseVisualStyleBackColor = false;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // Form_SafetyStatusMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 355);
            this.ControlBox = false;
            this.Controls.Add(this.panelMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(85, 91);
            this.Name = "Form_SafetyStatusMonitor";
            this.ShowIcon = false;
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_SafetyStatusMonitor_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_SafetyStatusMonitor_FormClosed);
            this.Load += new System.EventHandler(this.Form_SafetyStatusMonitor_Load);
            this.panelMain.ResumeLayout(false);
            this.mainLayout.ResumeLayout(false);
            this.errorContainer.ResumeLayout(false);
            this.errorTable.ResumeLayout(false);
            this.errorTable.PerformLayout();
            this.scrollPanel.ResumeLayout(false);
            this.scrollPanel.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        internal BlackBorderTextBox TextBox_Errorcode;
        internal BlackBorderTextBox TextBox_Errormessage;
    }
}