namespace TransformData
{
    partial class frmSectionFile
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnElevateNumber = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(522, 20);
            this.textBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(540, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(37, 21);
            this.button1.TabIndex = 1;
            this.button1.Text = "..";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnElevateNumber
            // 
            this.btnElevateNumber.Location = new System.Drawing.Point(12, 97);
            this.btnElevateNumber.Name = "btnElevateNumber";
            this.btnElevateNumber.Size = new System.Drawing.Size(153, 23);
            this.btnElevateNumber.TabIndex = 2;
            this.btnElevateNumber.Text = "Seksjoner på 1.1";
            this.btnElevateNumber.UseVisualStyleBackColor = true;
            this.btnElevateNumber.Click += new System.EventHandler(this.btnElevateNumber_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 55);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(153, 23);
            this.button2.TabIndex = 3;
            this.button2.Text = "Strip MS Word";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // frmSectionFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 445);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnElevateNumber);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Name = "frmSectionFile";
            this.Text = "Seksjoner Html-fil";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnElevateNumber;
        private System.Windows.Forms.Button button2;
    }
}