namespace AutoInvtoryManage
{
    partial class InfoConfirm
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
            this.InfoText = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.A1 = new System.Windows.Forms.TextBox();
            this.A2 = new System.Windows.Forms.TextBox();
            this.A3 = new System.Windows.Forms.TextBox();
            this.B2 = new System.Windows.Forms.ComboBox();
            this.B1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // InfoText
            // 
            this.InfoText.AutoSize = true;
            this.InfoText.Font = new System.Drawing.Font("宋体", 10F);
            this.InfoText.Location = new System.Drawing.Point(22, 28);
            this.InfoText.Name = "InfoText";
            this.InfoText.Size = new System.Drawing.Size(35, 14);
            this.InfoText.TabIndex = 0;
            this.InfoText.Text = "Info";
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(72, 298);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(83, 37);
            this.button1.TabIndex = 1;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(256, 298);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(83, 37);
            this.button2.TabIndex = 1;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 10F);
            this.label1.Location = new System.Drawing.Point(25, 267);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "备注3:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 10F);
            this.label2.Location = new System.Drawing.Point(25, 244);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(49, 14);
            this.label2.TabIndex = 0;
            this.label2.Text = "备注2:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 10F);
            this.label3.Location = new System.Drawing.Point(25, 221);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 14);
            this.label3.TabIndex = 0;
            this.label3.Text = "备注1:";
            // 
            // A1
            // 
            this.A1.Location = new System.Drawing.Point(72, 219);
            this.A1.Name = "A1";
            this.A1.Size = new System.Drawing.Size(309, 21);
            this.A1.TabIndex = 2;
            // 
            // A2
            // 
            this.A2.Location = new System.Drawing.Point(72, 242);
            this.A2.Name = "A2";
            this.A2.Size = new System.Drawing.Size(309, 21);
            this.A2.TabIndex = 2;
            // 
            // A3
            // 
            this.A3.Location = new System.Drawing.Point(72, 265);
            this.A3.Name = "A3";
            this.A3.Size = new System.Drawing.Size(309, 21);
            this.A3.TabIndex = 2;
            // 
            // B2
            // 
            this.B2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.B2.FormattingEnabled = true;
            this.B2.Location = new System.Drawing.Point(72, 197);
            this.B2.Name = "B2";
            this.B2.Size = new System.Drawing.Size(309, 20);
            this.B2.TabIndex = 3;
            // 
            // B1
            // 
            this.B1.AutoSize = true;
            this.B1.Font = new System.Drawing.Font("宋体", 10F);
            this.B1.Location = new System.Drawing.Point(32, 199);
            this.B1.Name = "B1";
            this.B1.Size = new System.Drawing.Size(42, 14);
            this.B1.TabIndex = 0;
            this.B1.Text = "储位:";
            // 
            // InfoConfirm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(415, 350);
            this.Controls.Add(this.B2);
            this.Controls.Add(this.A3);
            this.Controls.Add(this.A2);
            this.Controls.Add(this.A1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.B1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.InfoText);
            this.Name = "InfoConfirm";
            this.Text = "出入库确认";
            this.Load += new System.EventHandler(this.InfoConfirm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label InfoText;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox A1;
        private System.Windows.Forms.TextBox A2;
        private System.Windows.Forms.TextBox A3;
        private System.Windows.Forms.ComboBox B2;
        private System.Windows.Forms.Label B1;
    }
}