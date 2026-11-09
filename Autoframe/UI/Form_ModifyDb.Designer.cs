namespace Autoframe
{
    partial class Form_ModifyDb
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
            this.label1 = new System.Windows.Forms.Label();
            this.cb_DbName = new System.Windows.Forms.ComboBox();
            this.cb_SlotNo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cb_SlotState = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btn_Modify = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(35, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Db";
            // 
            // cb_DbName
            // 
            this.cb_DbName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_DbName.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cb_DbName.FormattingEnabled = true;
            this.cb_DbName.Location = new System.Drawing.Point(143, 51);
            this.cb_DbName.Name = "cb_DbName";
            this.cb_DbName.Size = new System.Drawing.Size(233, 28);
            this.cb_DbName.TabIndex = 1;
            this.cb_DbName.SelectedIndexChanged += new System.EventHandler(this.cb_DbName_SelectedIndexChanged);
            // 
            // cb_SlotNo
            // 
            this.cb_SlotNo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_SlotNo.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cb_SlotNo.FormattingEnabled = true;
            this.cb_SlotNo.Location = new System.Drawing.Point(143, 103);
            this.cb_SlotNo.Name = "cb_SlotNo";
            this.cb_SlotNo.Size = new System.Drawing.Size(233, 28);
            this.cb_SlotNo.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(35, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select Slot";
            // 
            // cb_SlotState
            // 
            this.cb_SlotState.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_SlotState.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cb_SlotState.FormattingEnabled = true;
            this.cb_SlotState.Location = new System.Drawing.Point(143, 155);
            this.cb_SlotState.Name = "cb_SlotState";
            this.cb_SlotState.Size = new System.Drawing.Size(233, 28);
            this.cb_SlotState.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(35, 158);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 20);
            this.label3.TabIndex = 4;
            this.label3.Text = "Select State";
            // 
            // btn_Modify
            // 
            this.btn_Modify.Font = new System.Drawing.Font("Microsoft YaHei UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btn_Modify.Location = new System.Drawing.Point(425, 92);
            this.btn_Modify.Name = "btn_Modify";
            this.btn_Modify.Size = new System.Drawing.Size(164, 51);
            this.btn_Modify.TabIndex = 6;
            this.btn_Modify.Text = "Modify";
            this.btn_Modify.UseVisualStyleBackColor = true;
            this.btn_Modify.Click += new System.EventHandler(this.btn_Modify_Click);
            // 
            // Form_ModifyDb
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SeaShell;
            this.ClientSize = new System.Drawing.Size(675, 226);
            this.Controls.Add(this.btn_Modify);
            this.Controls.Add(this.cb_SlotState);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cb_SlotNo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cb_DbName);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form_ModifyDb";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_ModifyDb";
            this.Load += new System.EventHandler(this.Form_ModifyDb_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cb_DbName;
        private System.Windows.Forms.ComboBox cb_SlotNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cb_SlotState;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btn_Modify;
    }
}