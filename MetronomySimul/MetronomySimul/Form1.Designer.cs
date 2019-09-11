namespace MetronomySimul
{
    partial class Form1
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.activeConnections = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.Log = new System.Windows.Forms.RichTextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(954, 606);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.textBox1);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.progressBar2);
            this.tabPage1.Controls.Add(this.progressBar1);
            this.tabPage1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(946, 580);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Parametry symulacji";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(398, 240);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(169, 20);
            this.textBox1.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(434, 223);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Wartość wychylenia";
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(338, 197);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.progressBar2.RightToLeftLayout = true;
            this.progressBar2.Size = new System.Drawing.Size(145, 23);
            this.progressBar2.TabIndex = 1;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(482, 197);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(145, 23);
            this.progressBar1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.activeConnections);
            this.tabPage2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(946, 580);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Aktywne połączenia";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // activeConnections
            // 
            this.activeConnections.Location = new System.Drawing.Point(144, 108);
            this.activeConnections.Multiline = true;
            this.activeConnections.Name = "activeConnections";
            this.activeConnections.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.activeConnections.Size = new System.Drawing.Size(630, 269);
            this.activeConnections.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.Log);
            this.tabPage3.Cursor = System.Windows.Forms.Cursors.No;
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(946, 580);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Log";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(8, 630);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // Log
            // 
            this.Log.BackColor = System.Drawing.SystemColors.HotTrack;
            this.Log.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Log.ForeColor = System.Drawing.SystemColors.MenuBar;
            this.Log.Location = new System.Drawing.Point(6, 6);
            this.Log.Name = "Log";
            this.Log.ReadOnly = true;
            this.Log.Size = new System.Drawing.Size(934, 568);
            this.Log.TabIndex = 0;
            this.Log.Text = "Metronomy log";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 630);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Drgające metronomy";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Splitter splitter1;
        public System.Windows.Forms.ProgressBar progressBar2;
        public System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox activeConnections;
        public System.Windows.Forms.RichTextBox Log;
    }
}
