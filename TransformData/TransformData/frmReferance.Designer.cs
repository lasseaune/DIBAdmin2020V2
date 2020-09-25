namespace TransformData
{
    partial class frmReferance
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
            this.btnOpen = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnReadRef = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.gR = new System.Windows.Forms.GroupBox();
            this.rb2 = new System.Windows.Forms.RadioButton();
            this.rb1 = new System.Windows.Forms.RadioButton();
            this.gReg = new System.Windows.Forms.GroupBox();
            this.reg2 = new System.Windows.Forms.RadioButton();
            this.reg1 = new System.Windows.Forms.RadioButton();
            this.btnTestRegexp = new System.Windows.Forms.Button();
            this.btnValidate = new System.Windows.Forms.Button();
            this.cbListOfRegExp = new System.Windows.Forms.ComboBox();
            this.gR.SuspendLayout();
            this.gReg.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpen
            // 
            this.btnOpen.Location = new System.Drawing.Point(428, 12);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(33, 20);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = "...";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(410, 20);
            this.textBox1.TabIndex = 1;
            // 
            // btnReadRef
            // 
            this.btnReadRef.Location = new System.Drawing.Point(395, 280);
            this.btnReadRef.Name = "btnReadRef";
            this.btnReadRef.Size = new System.Drawing.Size(75, 23);
            this.btnReadRef.TabIndex = 2;
            this.btnReadRef.Text = "Les";
            this.btnReadRef.UseVisualStyleBackColor = true;
            this.btnReadRef.Click += new System.EventHandler(this.btnReadRef_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(12, 56);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(410, 20);
            this.textBox2.TabIndex = 3;
            this.textBox2.Text = "C:\\Documents and Settings\\Lasse Aune\\Mine dokumenter\\Visual Studio 2010\\Projects\\" +
    "TransformData\\TransformData\\xml\\InTextLinksRegexp.xml";
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(12, 97);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(410, 20);
            this.textBox3.TabIndex = 4;
            this.textBox3.Text = "C:\\Documents and Settings\\Lasse Aune\\mine dokumenter\\visual studio 2010\\Projects\\" +
    "TransformData\\TransformData\\xml\\InTextLinksActions.xml";
            this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "RegEx XML";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Actions XML";
            // 
            // gR
            // 
            this.gR.Controls.Add(this.rb2);
            this.gR.Controls.Add(this.rb1);
            this.gR.Location = new System.Drawing.Point(15, 123);
            this.gR.Name = "gR";
            this.gR.Size = new System.Drawing.Size(114, 73);
            this.gR.TabIndex = 7;
            this.gR.TabStop = false;
            this.gR.Text = "Actionfiler";
            // 
            // rb2
            // 
            this.rb2.AutoSize = true;
            this.rb2.Checked = true;
            this.rb2.Location = new System.Drawing.Point(6, 42);
            this.rb2.Name = "rb2";
            this.rb2.Size = new System.Drawing.Size(76, 17);
            this.rb2.TabIndex = 1;
            this.rb2.TabStop = true;
            this.rb2.Text = "Lokale filer";
            this.rb2.UseVisualStyleBackColor = true;
            // 
            // rb1
            // 
            this.rb1.AutoSize = true;
            this.rb1.Location = new System.Drawing.Point(6, 19);
            this.rb1.Name = "rb1";
            this.rb1.Size = new System.Drawing.Size(80, 17);
            this.rb1.TabIndex = 0;
            this.rb1.TabStop = true;
            this.rb1.Text = "Globale filer";
            this.rb1.UseVisualStyleBackColor = true;
            // 
            // gReg
            // 
            this.gReg.Controls.Add(this.reg2);
            this.gReg.Controls.Add(this.reg1);
            this.gReg.Location = new System.Drawing.Point(180, 123);
            this.gReg.Name = "gReg";
            this.gReg.Size = new System.Drawing.Size(125, 73);
            this.gReg.TabIndex = 8;
            this.gReg.TabStop = false;
            this.gReg.Text = "RegExp filer";
            // 
            // reg2
            // 
            this.reg2.AutoSize = true;
            this.reg2.Checked = true;
            this.reg2.Location = new System.Drawing.Point(6, 42);
            this.reg2.Name = "reg2";
            this.reg2.Size = new System.Drawing.Size(76, 17);
            this.reg2.TabIndex = 1;
            this.reg2.TabStop = true;
            this.reg2.Text = "Lokale filer";
            this.reg2.UseVisualStyleBackColor = true;
            // 
            // reg1
            // 
            this.reg1.AutoSize = true;
            this.reg1.Location = new System.Drawing.Point(6, 19);
            this.reg1.Name = "reg1";
            this.reg1.Size = new System.Drawing.Size(80, 17);
            this.reg1.TabIndex = 0;
            this.reg1.TabStop = true;
            this.reg1.Text = "Globale filer";
            this.reg1.UseVisualStyleBackColor = true;
            // 
            // btnTestRegexp
            // 
            this.btnTestRegexp.Location = new System.Drawing.Point(314, 280);
            this.btnTestRegexp.Name = "btnTestRegexp";
            this.btnTestRegexp.Size = new System.Drawing.Size(75, 23);
            this.btnTestRegexp.TabIndex = 9;
            this.btnTestRegexp.Text = "Test Regexp";
            this.btnTestRegexp.UseVisualStyleBackColor = true;
            this.btnTestRegexp.Click += new System.EventHandler(this.btnTestRegexp_Click);
            // 
            // btnValidate
            // 
            this.btnValidate.Location = new System.Drawing.Point(311, 173);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(136, 23);
            this.btnValidate.TabIndex = 10;
            this.btnValidate.Text = "Konsolider RegEx";
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // cbListOfRegExp
            // 
            this.cbListOfRegExp.FormattingEnabled = true;
            this.cbListOfRegExp.Location = new System.Drawing.Point(314, 138);
            this.cbListOfRegExp.Name = "cbListOfRegExp";
            this.cbListOfRegExp.Size = new System.Drawing.Size(133, 21);
            this.cbListOfRegExp.TabIndex = 11;
            this.cbListOfRegExp.SelectedIndexChanged += new System.EventHandler(this.cbListOfRegExp_SelectedIndexChanged);
            // 
            // frmReferance
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(485, 315);
            this.Controls.Add(this.cbListOfRegExp);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.btnTestRegexp);
            this.Controls.Add(this.gReg);
            this.Controls.Add(this.gR);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.btnReadRef);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnOpen);
            this.Name = "frmReferance";
            this.Text = "frmReferance";
            this.gR.ResumeLayout(false);
            this.gR.PerformLayout();
            this.gReg.ResumeLayout(false);
            this.gReg.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnReadRef;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox gR;
        private System.Windows.Forms.RadioButton rb2;
        private System.Windows.Forms.RadioButton rb1;
        private System.Windows.Forms.GroupBox gReg;
        private System.Windows.Forms.RadioButton reg2;
        private System.Windows.Forms.RadioButton reg1;
        private System.Windows.Forms.Button btnTestRegexp;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.ComboBox cbListOfRegExp;
    }
}