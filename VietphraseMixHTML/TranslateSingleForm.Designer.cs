namespace VietphraseMixHTML
{
    partial class TranslateSingleForm
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
            this.textChooseFile = new System.Windows.Forms.TextBox();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textChooseFile
            // 
            this.textChooseFile.Location = new System.Drawing.Point(13, 43);
            this.textChooseFile.Name = "textChooseFile";
            this.textChooseFile.Size = new System.Drawing.Size(375, 20);
            this.textChooseFile.TabIndex = 0;
            // 
            // buttonSelect
            // 
            this.buttonSelect.Location = new System.Drawing.Point(395, 43);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(88, 23);
            this.buttonSelect.TabIndex = 1;
            this.buttonSelect.Text = "Chon file";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Location = new System.Drawing.Point(13, 70);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(99, 23);
            this.buttonTranslate.TabIndex = 2;
            this.buttonTranslate.Text = "Dich";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(13, 122);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(92, 13);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Chua dich file nao";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(154, 75);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(144, 17);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "Ghi de len file dang chon";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(408, 122);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Exit";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // TranslateSingleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(494, 148);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.buttonSelect);
            this.Controls.Add(this.textChooseFile);
            this.Name = "TranslateSingleForm";
            this.Text = "Dich thuan file rieng le";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textChooseFile;
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Button button1;
    }
}