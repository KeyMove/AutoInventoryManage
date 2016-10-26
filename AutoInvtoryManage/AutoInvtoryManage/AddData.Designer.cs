namespace AutoInvtoryManage
{
    partial class AddData
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
            this.T1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CSID = new System.Windows.Forms.CheckBox();
            this.T2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.T3 = new System.Windows.Forms.TextBox();
            this.T4 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.T6 = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.T7 = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.T5 = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // T1
            // 
            this.T1.Location = new System.Drawing.Point(77, 6);
            this.T1.Name = "T1";
            this.T1.ReadOnly = true;
            this.T1.Size = new System.Drawing.Size(185, 21);
            this.T1.TabIndex = 0;
            this.T1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.IntInput);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "物料编号:";
            // 
            // CSID
            // 
            this.CSID.AutoSize = true;
            this.CSID.Location = new System.Drawing.Point(275, 9);
            this.CSID.Name = "CSID";
            this.CSID.Size = new System.Drawing.Size(108, 16);
            this.CSID.TabIndex = 1;
            this.CSID.Text = "自定义物料编号";
            this.CSID.UseVisualStyleBackColor = true;
            this.CSID.CheckedChanged += new System.EventHandler(this.CSID_CheckedChanged);
            // 
            // T2
            // 
            this.T2.Location = new System.Drawing.Point(77, 33);
            this.T2.Name = "T2";
            this.T2.Size = new System.Drawing.Size(185, 21);
            this.T2.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "物料代码:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "物料名称:";
            // 
            // T3
            // 
            this.T3.Location = new System.Drawing.Point(77, 60);
            this.T3.Name = "T3";
            this.T3.Size = new System.Drawing.Size(185, 21);
            this.T3.TabIndex = 3;
            // 
            // T4
            // 
            this.T4.Location = new System.Drawing.Point(77, 87);
            this.T4.Multiline = true;
            this.T4.Name = "T4";
            this.T4.Size = new System.Drawing.Size(185, 108);
            this.T4.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "规格型号:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(36, 204);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "单位:";
            // 
            // T6
            // 
            this.T6.Location = new System.Drawing.Point(77, 228);
            this.T6.Name = "T6";
            this.T6.Size = new System.Drawing.Size(185, 21);
            this.T6.TabIndex = 6;
            this.T6.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.IntInput);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 232);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(59, 12);
            this.label6.TabIndex = 4;
            this.label6.Text = "库存数量:";
            // 
            // T7
            // 
            this.T7.Location = new System.Drawing.Point(77, 255);
            this.T7.Name = "T7";
            this.T7.Size = new System.Drawing.Size(185, 21);
            this.T7.TabIndex = 7;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 258);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 12);
            this.label7.TabIndex = 4;
            this.label7.Text = "储存位置:";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(275, 292);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(88, 34);
            this.button1.TabIndex = 8;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // T5
            // 
            this.T5.FormattingEnabled = true;
            this.T5.Items.AddRange(new object[] {
            "PCS"});
            this.T5.Location = new System.Drawing.Point(77, 202);
            this.T5.Name = "T5";
            this.T5.Size = new System.Drawing.Size(185, 20);
            this.T5.TabIndex = 5;
            this.T5.Text = "PCS";
            // 
            // AddData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(382, 344);
            this.Controls.Add(this.T5);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.T4);
            this.Controls.Add(this.T7);
            this.Controls.Add(this.T6);
            this.Controls.Add(this.T3);
            this.Controls.Add(this.CSID);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.T2);
            this.Controls.Add(this.T1);
            this.Name = "AddData";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "添加物料";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox T1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox CSID;
        private System.Windows.Forms.TextBox T2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox T3;
        private System.Windows.Forms.TextBox T4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox T6;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox T7;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox T5;
    }
}