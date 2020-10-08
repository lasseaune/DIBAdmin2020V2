namespace Regjeringen
{
    partial class frmDownloadRegjeringen
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
            this.btnDownload = new System.Windows.Forms.Button();
            this.txtHTTPadress = new System.Windows.Forms.TextBox();
            this.btmMakeHtml = new System.Windows.Forms.Button();
            this.txtTempFolderName = new System.Windows.Forms.TextBox();
            this.txtHtmlFileName = new System.Windows.Forms.TextBox();
            this.btnMakeSegment = new System.Windows.Forms.Button();
            this.btnXHTML = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(253, 47);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(80, 24);
            this.btnDownload.TabIndex = 0;
            this.btnDownload.Text = "Last ned";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // txtHTTPadress
            // 
            this.txtHTTPadress.Location = new System.Drawing.Point(12, 12);
            this.txtHTTPadress.Name = "txtHTTPadress";
            this.txtHTTPadress.Size = new System.Drawing.Size(321, 20);
            this.txtHTTPadress.TabIndex = 1;
            // 
            // btmMakeHtml
            // 
            this.btmMakeHtml.Location = new System.Drawing.Point(253, 124);
            this.btmMakeHtml.Name = "btmMakeHtml";
            this.btmMakeHtml.Size = new System.Drawing.Size(79, 30);
            this.btmMakeHtml.TabIndex = 2;
            this.btmMakeHtml.Text = "Lag HTML fil";
            this.btmMakeHtml.UseVisualStyleBackColor = true;
            this.btmMakeHtml.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtTempFolderName
            // 
            this.txtTempFolderName.Location = new System.Drawing.Point(12, 97);
            this.txtTempFolderName.Name = "txtTempFolderName";
            this.txtTempFolderName.Size = new System.Drawing.Size(321, 20);
            this.txtTempFolderName.TabIndex = 3;
            // 
            // txtHtmlFileName
            // 
            this.txtHtmlFileName.Location = new System.Drawing.Point(12, 169);
            this.txtHtmlFileName.Name = "txtHtmlFileName";
            this.txtHtmlFileName.Size = new System.Drawing.Size(321, 20);
            this.txtHtmlFileName.TabIndex = 4;
            // 
            // btnMakeSegment
            // 
            this.btnMakeSegment.Location = new System.Drawing.Point(254, 200);
            this.btnMakeSegment.Name = "btnMakeSegment";
            this.btnMakeSegment.Size = new System.Drawing.Size(77, 22);
            this.btnMakeSegment.TabIndex = 5;
            this.btnMakeSegment.Text = "Lag reg dok";
            this.btnMakeSegment.UseVisualStyleBackColor = true;
            this.btnMakeSegment.Click += new System.EventHandler(this.btnMakeSegment_Click);
            // 
            // btnXHTML
            // 
            this.btnXHTML.Location = new System.Drawing.Point(253, 228);
            this.btnXHTML.Name = "btnXHTML";
            this.btnXHTML.Size = new System.Drawing.Size(75, 23);
            this.btnXHTML.TabIndex = 6;
            this.btnXHTML.Text = "XHTML";
            this.btnXHTML.UseVisualStyleBackColor = true;
            this.btnXHTML.Click += new System.EventHandler(this.btnXHTML_Click);
            // 
            // frmDownloadRegjeringen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(345, 344);
            this.Controls.Add(this.btnXHTML);
            this.Controls.Add(this.btnMakeSegment);
            this.Controls.Add(this.txtHtmlFileName);
            this.Controls.Add(this.txtTempFolderName);
            this.Controls.Add(this.btmMakeHtml);
            this.Controls.Add(this.txtHTTPadress);
            this.Controls.Add(this.btnDownload);
            this.Name = "frmDownloadRegjeringen";
            this.Text = "Last ned EPUB";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.TextBox txtHTTPadress;
        private System.Windows.Forms.Button btmMakeHtml;
        private System.Windows.Forms.TextBox txtTempFolderName;
        private System.Windows.Forms.TextBox txtHtmlFileName;
        private System.Windows.Forms.Button btnMakeSegment;
        private System.Windows.Forms.Button btnXHTML;
    }
}

