namespace Autoframe
{
    partial class Form_TraySetting
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
            this.cbx_Style = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lbl_Row = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lbl_Column = new System.Windows.Forms.Label();
            this.btn_Save = new AntdUI.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lbl_ColumnGap = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbl_RowGap = new System.Windows.Forms.Label();
            this.tsvTrayDisplay = new TraySlotControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "料盘型号";
            // 
            // cbx_Style
            // 
            this.cbx_Style.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbx_Style.Location = new System.Drawing.Point(92, 36);
            this.cbx_Style.Name = "cbx_Style";
            this.cbx_Style.Size = new System.Drawing.Size(121, 29);
            this.cbx_Style.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(232, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "料盘行数";
            // 
            // lbl_Row
            // 
            this.lbl_Row.AutoSize = true;
            this.lbl_Row.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Row.Location = new System.Drawing.Point(325, 17);
            this.lbl_Row.Name = "lbl_Row";
            this.lbl_Row.Size = new System.Drawing.Size(19, 21);
            this.lbl_Row.TabIndex = 3;
            this.lbl_Row.Text = "8";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(376, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 21);
            this.label4.TabIndex = 4;
            this.label4.Text = "料盘列数";
            // 
            // lbl_Column
            // 
            this.lbl_Column.AutoSize = true;
            this.lbl_Column.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_Column.Location = new System.Drawing.Point(467, 17);
            this.lbl_Column.Name = "lbl_Column";
            this.lbl_Column.Size = new System.Drawing.Size(19, 21);
            this.lbl_Column.TabIndex = 5;
            this.lbl_Column.Text = "8";
            // 
            // btn_Save
            // 
            this.btn_Save.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btn_Save.Location = new System.Drawing.Point(3, 552);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(514, 44);
            this.btn_Save.TabIndex = 8;
            this.btn_Save.Text = "料盘型号已经选好";
            this.btn_Save.Type = AntdUI.TTypeMini.Success;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.btn_Save, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tsvTrayDisplay, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(520, 599);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbl_ColumnGap);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.lbl_RowGap);
            this.panel1.Controls.Add(this.cbx_Style);
            this.panel1.Controls.Add(this.lbl_Column);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lbl_Row);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 452);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(514, 94);
            this.panel1.TabIndex = 9;
            // 
            // lbl_ColumnGap
            // 
            this.lbl_ColumnGap.AutoSize = true;
            this.lbl_ColumnGap.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_ColumnGap.Location = new System.Drawing.Point(467, 58);
            this.lbl_ColumnGap.Name = "lbl_ColumnGap";
            this.lbl_ColumnGap.Size = new System.Drawing.Size(19, 21);
            this.lbl_ColumnGap.TabIndex = 9;
            this.lbl_ColumnGap.Text = "8";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(376, 58);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(90, 21);
            this.label5.TabIndex = 8;
            this.label5.Text = "料盘列间距";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(232, 58);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 21);
            this.label6.TabIndex = 6;
            this.label6.Text = "料盘行间距";
            // 
            // lbl_RowGap
            // 
            this.lbl_RowGap.AutoSize = true;
            this.lbl_RowGap.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbl_RowGap.Location = new System.Drawing.Point(325, 58);
            this.lbl_RowGap.Name = "lbl_RowGap";
            this.lbl_RowGap.Size = new System.Drawing.Size(19, 21);
            this.lbl_RowGap.TabIndex = 7;
            this.lbl_RowGap.Text = "8";
            // 
            // tsvTrayDisplay
            // 
            this.tsvTrayDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tsvTrayDisplay.Location = new System.Drawing.Point(3, 3);
            this.tsvTrayDisplay.Name = "tsvTrayDisplay";
            this.tsvTrayDisplay.NewLineDisplayText = false;
            this.tsvTrayDisplay.Size = new System.Drawing.Size(514, 443);
            this.tsvTrayDisplay.TabIndex = 10;
            this.tsvTrayDisplay.Title = "料盘型号示意图";
            this.tsvTrayDisplay.TitleColor = System.Drawing.Color.Gold;
            this.tsvTrayDisplay.TrayColCount = 8;
            this.tsvTrayDisplay.TrayRowCount = 8;
            // 
            // Form_TraySetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 599);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form_TraySetting";
            this.Text = "Form_TraySetting";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbx_Style;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbl_Row;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lbl_Column;
        private AntdUI.Button btn_Save;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private TraySlotControl tsvTrayDisplay;
        private System.Windows.Forms.Label lbl_ColumnGap;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbl_RowGap;
    }
}