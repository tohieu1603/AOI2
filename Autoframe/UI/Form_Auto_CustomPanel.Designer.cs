
namespace Autoframe.UI
{
    partial class Form_Auto_CustomPanel
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBox_DI = new System.Windows.Forms.GroupBox();
            this.userControl_DI = new AF.Card.UI.UserControl_IO();
            this.groupBox_DO = new System.Windows.Forms.GroupBox();
            this.userControl_DO = new AF.Card.UI.UserControl_IO();
            this.btn_maintenanceMode = new System.Windows.Forms.Button();
            this.groupBox_DI.SuspendLayout();
            this.groupBox_DO.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox_DI
            // 
            this.groupBox_DI.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_DI.Controls.Add(this.userControl_DI);
            this.groupBox_DI.Location = new System.Drawing.Point(563, 9);
            this.groupBox_DI.Name = "groupBox_DI";
            this.groupBox_DI.Size = new System.Drawing.Size(281, 236);
            this.groupBox_DI.TabIndex = 0;
            this.groupBox_DI.TabStop = false;
            this.groupBox_DI.Text = "DI";
            // 
            // userControl_DI
            // 
            this.userControl_DI.BackColor = System.Drawing.SystemColors.Control;
            this.userControl_DI.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControl_DI.Location = new System.Drawing.Point(3, 18);
            this.userControl_DI.Name = "userControl_DI";
            this.userControl_DI.Size = new System.Drawing.Size(275, 215);
            this.userControl_DI.TabIndex = 0;
            // 
            // groupBox_DO
            // 
            this.groupBox_DO.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_DO.Controls.Add(this.userControl_DO);
            this.groupBox_DO.Location = new System.Drawing.Point(858, 9);
            this.groupBox_DO.Name = "groupBox_DO";
            this.groupBox_DO.Size = new System.Drawing.Size(281, 236);
            this.groupBox_DO.TabIndex = 1;
            this.groupBox_DO.TabStop = false;
            this.groupBox_DO.Text = "DO";
            // 
            // userControl_DO
            // 
            this.userControl_DO.BackColor = System.Drawing.SystemColors.Control;
            this.userControl_DO.Dock = System.Windows.Forms.DockStyle.Fill;
            this.userControl_DO.Location = new System.Drawing.Point(3, 18);
            this.userControl_DO.Name = "userControl_DO";
            this.userControl_DO.Size = new System.Drawing.Size(275, 215);
            this.userControl_DO.TabIndex = 0;
            // 
            // btn_maintenanceMode
            // 
            this.btn_maintenanceMode.BackColor = System.Drawing.Color.WhiteSmoke;
            this.btn_maintenanceMode.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_maintenanceMode.Location = new System.Drawing.Point(27, 27);
            this.btn_maintenanceMode.Name = "btn_maintenanceMode";
            this.btn_maintenanceMode.Size = new System.Drawing.Size(170, 39);
            this.btn_maintenanceMode.TabIndex = 29;
            this.btn_maintenanceMode.Text = "维修模式操作";
            this.btn_maintenanceMode.UseVisualStyleBackColor = false;
            this.btn_maintenanceMode.Click += new System.EventHandler(this.btn_maintenanceMode_Click);
            // 
            // Form_Auto_CustomPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightGray;
            this.Controls.Add(this.btn_maintenanceMode);
            this.Controls.Add(this.groupBox_DO);
            this.Controls.Add(this.groupBox_DI);
            this.Name = "Form_Auto_CustomPanel";
            this.Size = new System.Drawing.Size(1153, 260);
            this.Load += new System.EventHandler(this.Form_Auto_CustomPanel_Load);
            this.groupBox_DI.ResumeLayout(false);
            this.groupBox_DO.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox_DI;
        private AF.Card.UI.UserControl_IO userControl_DI;
        private System.Windows.Forms.GroupBox groupBox_DO;
        private AF.Card.UI.UserControl_IO userControl_DO;
        private System.Windows.Forms.Button btn_maintenanceMode;
    }
}
