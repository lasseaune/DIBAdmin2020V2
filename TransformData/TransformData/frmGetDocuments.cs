using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using TransformData.Global;
using DIB.Data;
using System.IO;

namespace TransformData
{
    public partial class frmGetDocuments : Form
    {
        public string _REGEXPQ = "";
        public bool _RETURNKEY = false;
        public XElement m_resultXML = null;
        public XElement resultXML
        {
            get { return m_resultXML; }
            set { m_resultXML = value; }
        }
        private List<string> _names = new List<string>();
        private List<string> _year = null;

        
        public frmGetDocuments(Form p)
        {
            this.Owner = p;
            InitializeComponent();
            LoadExisting();
            LoadComboBox();
        }

        public frmGetDocuments(Form p, string Key)
        {
            this.Owner = p;
            InitializeComponent();
            LoadExisting();
            LoadComboBox(Key, true);
        }


        
        private void LoadComboBox()
        {
            if (global.m_DB_TOPICS.Count() == 0) return;
            _names = new List<string>();
            _names.Add("(Alle)");

            listBox1.Items.Clear();
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.Sorted = false;

            foreach (topic t in global.m_DB_TOPICS
                        .OrderBy(p=>(p.number=="none" ? 0 : Convert.ToInt32(p.number)))
                        .OrderBy(p => p.name1)
                        .OrderByDescending(p => p.year1)
                        )
            {
                string result = _names.Find(
                    delegate(string p)
                    {
                        return p == t.name1;
                    });
                if (result == null)
                    _names.Add(t.name1);
                
                ListViewItem item = new ListViewItem(t.info);
                item.Tag = t;
                listBox1.Items.Add(item);

            }
            foreach (string s in _names)
            {
                comboBox1.Text = "(Alle)";
                comboBox1.Items.Add(s);
            }
        }

        private void LoadComboBox(string Key, bool ReturnTopicId)
        {
            _RETURNKEY = ReturnTopicId;
            if (global.m_DB_TOPICS.Where(t => (t.key).ToLower() == Key.ToLower()).Count() == 0) return;
            _names = new List<string>();
            _names.Add("(Alle)");

            listBox1.Items.Clear();
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.Sorted = false;

            foreach (topic t in global.m_DB_TOPICS.Where(t => (t.key).ToLower() == Key.ToLower()))
            {
                ListViewItem item = new ListViewItem(t.info);
                item.Tag = t;
                listBox1.Items.Add(item);

            }
        }

        private void LoadComboBox(string name1)
        {
            if (global.m_DB_TOPICS.Count() == 0) return;

            _year = new List<string>();
            listBox1.Items.Clear();
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.Sorted = false;


            foreach (topic t in global.m_DB_TOPICS.Where(p=>p.name1 == name1))
            {
                
                string result = _year.Find(
                    delegate(string p)
                    {
                        return p == t.year1;
                    });
                if (result == null)
                    _year.Add(t.year1);

                ListViewItem item = new ListViewItem(t.info);
                item.Tag = t;
                listBox1.Items.Add(item);

            }

            foreach (string s in _year.OrderBy(p=>Convert.ToInt32(p)))
            {
                comboBox2.Text = "(Alle)";
                comboBox2.Items.Add(s);
            }

        }

        private void LoadComboBox(string name1, string year)
        {
            if (global.m_DB_TOPICS.Count() == 0) return;


            listBox1.Items.Clear();
            listBox1.DrawItem += new DrawItemEventHandler(listBox1_DrawItem);
            listBox1.DrawMode = DrawMode.OwnerDrawVariable;
            listBox1.Sorted = false;


            foreach (topic t in global.m_DB_TOPICS.Where(p => p.name1 == name1 && p.year1 == year))
            {

                ListViewItem item = new ListViewItem(t.info);
                item.Tag = t;
                listBox1.Items.Add(item);

            }

                 
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
            DialogResult = DialogResult.Cancel;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null) return;
            if (this.Owner.Name == "frmHtmlImport")
            {
                frmHtmlImport f = this.Owner as frmHtmlImport;
                ListViewItem l = (ListViewItem)listBox1.SelectedItem;

                topic t = (topic)l.Tag;
                if (_RETURNKEY)
                {
                    this.Tag = t.topic_id;
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    if (_Existing.Where(p => p.key.ToLower() == t.key.ToLower()).Count() != 0)
                    {
                        f.OpenDocument(_Existing.Where(p => p.key.ToLower() == t.key.ToLower()).ElementAt(0));
                    }
                    else
                    {
                        f.OpenDocument(t.info);
                    }
                }
            }
            this.Close();
        }


        /// <summary> 
        /// Handles the DrawItem event of the listBox1 control. 
        /// </summary> 
        /// <param name="sender">The source of the event.</param> 
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param> 
        private void listBox1_DrawItem( object sender, DrawItemEventArgs e ) 
        {
            ListViewItem l = (ListViewItem)listBox1.Items[e.Index];
            string name = l.Text;
            //string name = listBox1.Items[e.Index].ToString();
            topic t = (topic)l.Tag;
            if (ItemExists(t.name1 + "_" +  t.number + "_" + t.type + "_" + t.year1 + "_" + t.year2))
            {
                e.DrawBackground();
                Graphics g = e.Graphics;
                // draw the background color you want 
                // mine is set to olive, change it to whatever you want     
                if (t.doctype == "xml")
                {
                    g.FillRectangle(new SolidBrush(Color.LightGreen), e.Bounds);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(Color.HotPink), e.Bounds);
                }
                // draw the text of the list item, not doing this will only show     
                // the background color     
                // you will need to get the text of item to display 
                g.DrawString(name, e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));
                e.DrawFocusRectangle();
            }
            else
            {
                e.DrawBackground();
                Graphics g = e.Graphics;
                // draw the background color you want 
                // mine is set to olive, change it to whatever you want     
                
                if (t.doctype == "xml")
                {
                    g.FillRectangle(new SolidBrush(Color.Transparent), e.Bounds);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(Color.IndianRed), e.Bounds);
                }

                // draw the text of the list item, not doing this will only show     
                // the background color     
                // you will need to get the text of item to display 
                g.DrawString(name, e.Font, new SolidBrush(e.ForeColor), new PointF(e.Bounds.X, e.Bounds.Y));
                e.DrawFocusRectangle();

            }
        }
        
        
        private List<existing> _Existing = null;

        private void LoadExisting()
        {
            _Existing = new List<existing>();
            bool saveChange = false;
            XDocument xmlDoc = null;
                
            if (File.Exists(global._dbPath + "import.xml"))
            {
                xmlDoc = XDocument.Load(global._dbPath + "import.xml");
            }

            if (xmlDoc != null)
            {
                foreach (XElement d in xmlDoc.Descendants("document"))
                {
                    string name = "";
                    if (d.Attribute("name1") == null && d.Element("name")!=null) 
                    {
                        name = d.Element("name").Value;
                        name.IdentifyMatchForarbeider(d);
                        saveChange = true;
                    }
                    if (d.Attribute("name1") != null)
                    {
                        //_Existing.Add(d.Element("name").Value.GetForarbeidNumber(_REGEXPQ));

                        existing ge = new existing();
                        ge.key = ((d.Attribute("name1") == null ? "none" : d.Attribute("name1").Value)
                        + (d.Attribute("number") == null ? "_none" : "_" + d.Attribute("number").Value)
                        + (d.Attribute("type") == null ? "_none" : "_" + d.Attribute("type").Value)
                        + (d.Attribute("year1") == null ? "_none" : "_" + d.Attribute("year1").Value)
                        + (d.Attribute("year2") == null ? "_none" : "_" + d.Attribute("year2").Value)).ToLower();
                        ge.name = name;
                        ge.area = d.Element("host") == null ? "" : d.Element("host").Value;
                        ge.topic_id = d.Element("topic_id") == null ? "" : d.Element("topic_id").Value;
                        
                        _Existing.Add(ge);

                        if (ge.topic_id == "")
                        {
                            string xx = "";
                        }
                        if (global.m_DB_TOPICS.Where(p => p.key.ToLower() == ge.key.ToLower() && ge.topic_id == "").Count() == 1)
                        {
                            saveChange = true;
                            if (d.Element("topic_id") == null)
                                d.Add(new XElement("topic_id", global.m_DB_TOPICS.Where(p => p.key.ToLower() == ge.key.ToLower() && ge.topic_id == "").First().topic_id));
                            else
                                d.Element("topic_id").Value = global.m_DB_TOPICS.Where(p => p.key.ToLower() == ge.key.ToLower() && ge.topic_id == "").First().topic_id;
                        }
                        if (global.m_DB_TOPICS.Where(p => p.key.ToLower() == ge.key.ToLower()).Count() == 0)
                        {
                            topic t = new topic();
                            t.topic_id = "";
                            t.name = ge.name;
                            t.info = ((d.Attribute("name1") == null ? "" : d.Attribute("name1").Value)
                                    + ". "
                                    + (d.Attribute("number") == null ? "" : "" + d.Attribute("number").Value)
                                    + (d.Attribute("type") == null ? "" : "" + d.Attribute("type").Value)
                                    + " ("
                                    + (d.Attribute("year1") == null ? "" : "" + d.Attribute("year1").Value)
                                    + (d.Attribute("year2") == null ? "" : "-" + d.Attribute("year2").Value)).ToLower()
                                    + ")";
    
                            t.year = (d.Attribute("year1") == null ? "_none" : "_" + d.Attribute("year1").Value);
                            t.name1 = (d.Attribute("name1") == null ? "none" : d.Attribute("name1").Value);
                            t.number = (d.Attribute("number") == null ? "none" : d.Attribute("number").Value);
                            t.type = (d.Attribute("type") == null ? "none" : d.Attribute("type").Value);
                            t.year1 = (d.Attribute("year1") == null ? "none" : d.Attribute("year1").Value);
                            t.year2 = (d.Attribute("year2") == null ? "none" : d.Attribute("year2").Value);
                            t.key = ge.key;
                            global.m_DB_TOPICS.Add(t);
                        }
                    }

                    
                    if (saveChange)
                    {
                        xmlDoc.Save(global._dbPath +  "import.xml");
                    }
                }
            }
        }
        private bool ItemExists(string name)
        {
            //string test = name.GetForarbeidNumber(_REGEXPQ);
            if (_Existing.Where(p => p.key.Trim().ToLower() == name.Trim().ToLower()).Count() != 0)
            {
                return true;
            }
            return false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
                if (comboBox1.SelectedItem.ToString() == "(Alle)")
                {
                    LoadComboBox();
                }
                else
                    LoadComboBox(comboBox1.SelectedItem.ToString());

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null )
            {
                if (comboBox1.SelectedItem.ToString() != "(Alle)")
                {
                    LoadComboBox(comboBox1.SelectedItem.ToString(), comboBox2.SelectedItem.ToString());
                }
                else
                    MessageBox.Show("Velg en type!");
                    

            }

        }

    }
}
