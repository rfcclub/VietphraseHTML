namespace VietphraseMixHTML
{
    partial class GetLinksForm
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
            this.lstLinks = new System.Windows.Forms.ListBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtUpdateSign = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rdoIncremental = new System.Windows.Forms.RadioButton();
            this.rdoStringPrefix = new System.Windows.Forms.RadioButton();
            this.rdoRegExp = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.rdoNone = new System.Windows.Forms.RadioButton();
            this.btnReload = new System.Windows.Forms.Button();
            this.chkSortable = new System.Windows.Forms.CheckBox();
            this.btnSort = new System.Windows.Forms.Button();
            this.btnSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstLinks
            // 
            this.lstLinks.FormattingEnabled = true;
            this.lstLinks.Location = new System.Drawing.Point(27, 136);
            this.lstLinks.Name = "lstLinks";
            this.lstLinks.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstLinks.Size = new System.Drawing.Size(395, 485);
            this.lstLinks.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(163, 627);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(126, 35);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "Đồng ý";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtUpdateSign
            // 
            this.txtUpdateSign.Location = new System.Drawing.Point(124, 49);
            this.txtUpdateSign.Name = "txtUpdateSign";
            this.txtUpdateSign.Size = new System.Drawing.Size(298, 20);
            this.txtUpdateSign.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Dấu hiệu update";
            // 
            // rdoIncremental
            // 
            this.rdoIncremental.AutoSize = true;
            this.rdoIncremental.Location = new System.Drawing.Point(25, 26);
            this.rdoIncremental.Name = "rdoIncremental";
            this.rdoIncremental.Size = new System.Drawing.Size(71, 17);
            this.rdoIncremental.TabIndex = 4;
            this.rdoIncremental.TabStop = true;
            this.rdoIncremental.Text = "Tăng dần";
            this.rdoIncremental.UseVisualStyleBackColor = true;
            this.rdoIncremental.CheckedChanged += new System.EventHandler(this.rdoIncremental_CheckedChanged);
            // 
            // rdoStringPrefix
            // 
            this.rdoStringPrefix.AutoSize = true;
            this.rdoStringPrefix.Location = new System.Drawing.Point(117, 26);
            this.rdoStringPrefix.Name = "rdoStringPrefix";
            this.rdoStringPrefix.Size = new System.Drawing.Size(93, 17);
            this.rdoStringPrefix.TabIndex = 5;
            this.rdoStringPrefix.TabStop = true;
            this.rdoStringPrefix.Text = "Chuỗi đầu link";
            this.rdoStringPrefix.UseVisualStyleBackColor = true;
            this.rdoStringPrefix.CheckedChanged += new System.EventHandler(this.rdoStringPrefix_CheckedChanged);
            // 
            // rdoRegExp
            // 
            this.rdoRegExp.AutoSize = true;
            this.rdoRegExp.Location = new System.Drawing.Point(216, 26);
            this.rdoRegExp.Name = "rdoRegExp";
            this.rdoRegExp.Size = new System.Drawing.Size(116, 17);
            this.rdoRegExp.TabIndex = 6;
            this.rdoRegExp.TabStop = true;
            this.rdoRegExp.Text = "Regular Expression";
            this.rdoRegExp.UseVisualStyleBackColor = true;
            this.rdoRegExp.CheckedChanged += new System.EventHandler(this.rdoRegExp_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Chuỗi nhận dạng:";
            // 
            // rdoNone
            // 
            this.rdoNone.AutoSize = true;
            this.rdoNone.Location = new System.Drawing.Point(339, 26);
            this.rdoNone.Name = "rdoNone";
            this.rdoNone.Size = new System.Drawing.Size(83, 17);
            this.rdoNone.TabIndex = 8;
            this.rdoNone.TabStop = true;
            this.rdoNone.Text = "Không dùng";
            this.rdoNone.UseVisualStyleBackColor = true;
            this.rdoNone.CheckedChanged += new System.EventHandler(this.rdoNone_CheckedChanged);
            // 
            // btnReload
            // 
            this.btnReload.Location = new System.Drawing.Point(313, 75);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(109, 23);
            this.btnReload.TabIndex = 10;
            this.btnReload.Text = "Load lai";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // chkSortable
            // 
            this.chkSortable.AutoSize = true;
            this.chkSortable.Location = new System.Drawing.Point(30, 74);
            this.chkSortable.Name = "chkSortable";
            this.chkSortable.Size = new System.Drawing.Size(138, 17);
            this.chkSortable.TabIndex = 11;
            this.chkSortable.Text = "Sap xep truoc khi down";
            this.chkSortable.UseVisualStyleBackColor = true;
            this.chkSortable.CheckedChanged += new System.EventHandler(this.chkSortable_CheckedChanged);
            // 
            // btnSort
            // 
            this.btnSort.Location = new System.Drawing.Point(313, 104);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(109, 23);
            this.btnSort.TabIndex = 12;
            this.btnSort.Text = "Sap xep lai link";
            this.btnSort.UseVisualStyleBackColor = true;
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // btnSelect
            // 
            this.btnSelect.Location = new System.Drawing.Point(30, 107);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(162, 23);
            this.btnSelect.TabIndex = 13;
            this.btnSelect.Text = "Chon theo dau hieu update";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // GetLinksForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(453, 669);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnSort);
            this.Controls.Add(this.chkSortable);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.rdoNone);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.rdoRegExp);
            this.Controls.Add(this.rdoStringPrefix);
            this.Controls.Add(this.rdoIncremental);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtUpdateSign);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lstLinks);
            this.Name = "GetLinksForm";
            this.Text = "GetLinksForm";
            this.Load += new System.EventHandler(this.GetLinksForm_Load);
            this.Shown += new System.EventHandler(this.GetLinksForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstLinks;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtUpdateSign;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton rdoIncremental;
        private System.Windows.Forms.RadioButton rdoStringPrefix;
        private System.Windows.Forms.RadioButton rdoRegExp;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton rdoNone;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.CheckBox chkSortable;
        private System.Windows.Forms.Button btnSort;
        private System.Windows.Forms.Button btnSelect;
    }
}