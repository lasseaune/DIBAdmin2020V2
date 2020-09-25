using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TransformData.Global;
using System.Xml;
using System.Xml.Linq;
using System.IO;
namespace TransformData
{
    public partial class frmProject : Form
    {
        
        public TextBox[] tb;
        public Label[] lb;
        private string _HOST = "";
        
        public void SetTextBoxValue(string value)
        {
            TextBox c = tb.Where(p => p.BorderStyle == BorderStyle.FixedSingle).First();
            this.SetVisibleCore(true);
            c.Text = c.Text + value;
            
        }
        public frmProject(string path, string host, string name, string title, string topic_id)
        {
            InitializeComponent();
            _HOST = host;
            tb = new TextBox[5];
            lb = new Label[5];
            int y = 6;
            for (int i = 0, len = tb.Count(); i < len; i++)
            {
                lb[i] = new Label();
                lb[i].Name = "lb" + i;
                lb[i].Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
                switch (i)
                {
                    case 0: lb[i].Text = "Adresse"; break;
                    case 1: lb[i].Text = "Kilde"; break;
                    case 2:
                        lb[i].Text = "Tittel"; break;
                        
                    case 3: lb[i].Text = "Undertittel"; break;
                    case 4: lb[i].Text = "Topic Id:"; break;
                }
                 
                lb[i].Location = new Point(11, y);
                splitContainer1.Panel1.Controls.Add(lb[i]);
                y = y + 15;
                tb[i] = new TextBox();
                tb[i].Name = "textBox" + i;
                
                switch (i)
                {
                    case 0:
                        tb[i].Text = path;
                        tb[i].Enabled = false;
                        break;
                    case 1:
                        tb[i].Text = host;
                        tb[i].Enabled = false;
                        break;
                    case 2:
                        tb[i].TextChanged += new EventHandler(title_textChanged);
                        tb[i].Text = name;
                        break;
                    case 3:
                        tb[i].Text = title;
                        break;
                    case 4:
                        tb[i].Text = topic_id;
                        break;

                    default:
                        break;
                }

                tb[i].Location = new Point(12, y);
                tb[i].BringToFront();
                tb[i].MouseDown += new MouseEventHandler(tb_MouseDown);
                tb[i].Width = splitContainer1.Width - (tb[i].Left*2);
                splitContainer1.Panel1.Controls.Add(tb[i]);
                tb[i].Anchor = (AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top);
                lb[i].SendToBack();
                tb[i].BringToFront();
                y = y + 24;
            }

            
        }
        private void title_textChanged(object sender, EventArgs e)
        {
           TextBox t = sender as TextBox;
            if (t != null)
            {
                string test = t.Text;
                if (global._dbArea == "")
                {
                    global._dbArea = _HOST + @"\";
                }

                if (File.Exists(global._dbPath + global._dbArea + "import.xml"))
                {
                    XDocument xmlDoc = XDocument.Load(global._dbPath + global._dbArea + "import.xml");

                    if ((xmlDoc.Descendants("document").Count() == 0 ? 0 : xmlDoc.Descendants("document").Where(p => (p.Element("name") == null ? "" : p.Element("name").Value).ToLower() == t.Text.ToLower()).Count()) != 0)
                    {
                        MessageBox.Show(t.Text + " er allerede importert!");
                        this.DialogResult = DialogResult.Abort;
                        this.Close();
                    }
                }
            }
        }
        private void tb_MouseDown(object sender, MouseEventArgs e )
        {
            foreach (TextBox t in tb)
            {
                if (t == sender)
                {
                    t.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                    t.BorderStyle = BorderStyle.Fixed3D;

            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }
    }
}
