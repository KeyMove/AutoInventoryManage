namespace AutoInvtoryManage
{
    partial class LoadExecl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadExecl));
            this.LoadBar = new System.Windows.Forms.ProgressBar();
            this.LoadInfo = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LoadBar
            // 
            this.LoadBar.Location = new System.Drawing.Point(33, 12);
            this.LoadBar.Name = "LoadBar";
            this.LoadBar.Size = new System.Drawing.Size(552, 47);
            this.LoadBar.TabIndex = 0;
            // 
            // LoadInfo
            // 
            this.LoadInfo.AutoSize = true;
            this.LoadInfo.Location = new System.Drawing.Point(285, 71);
            this.LoadInfo.Name = "LoadInfo";
            this.LoadInfo.Size = new System.Drawing.Size(59, 12);
            this.LoadInfo.TabIndex = 1;
            this.LoadInfo.Text = "导入中...";
            // 
            // LoadExecl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(617, 89);
            this.Controls.Add(this.LoadInfo);
            this.Controls.Add(this.LoadBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoadExecl";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "导入Execl表格";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar LoadBar;
        private System.Windows.Forms.Label LoadInfo;
    }
}