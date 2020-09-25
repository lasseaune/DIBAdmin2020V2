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
using System.Text.RegularExpressions;
using TransformData.Global;
using System.Diagnostics;


namespace TransformData
{
    public partial class frmBookmarks : Form
    {
        private XElement _CONTAINER;
        private XElement _DOCUMENT;
        
        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                DataGridViewCell c = dataGridView1.CurrentCell;
                ContextMenuStrip.Show();
            }
        }
        
        public frmBookmarks(XElement container, XElement document)
        {
            InitializeComponent();
            _CONTAINER = container;
            _DOCUMENT = document;

            //myWebBrowser.DocumentCompleted +=new WebBrowserDocumentCompletedEventHandler(myWebBrowser_DocumentCompleted);
            dataGridView1.MouseClick += new MouseEventHandler(dataGridView1_MouseClick);
            if (_CONTAINER.Descendants("token").Count() != 0)
            {
                int i = 1;
                foreach (XElement t in _CONTAINER.Descendants("token"))
                {
                    switch (i)
                    {
                        case 1: t1.Text = t.Attribute("text").Value; r1.Text = t.Attribute("replace").Value; s1.Text = (t.Attribute("substitute") == null ? "" : t.Attribute("substitute").Value); a1.Text = (t.Attribute("add") == null ? "" : t.Attribute("add").Value); break;
                        case 2: t2.Text = t.Attribute("text").Value; r2.Text = t.Attribute("replace").Value; s2.Text = (t.Attribute("substitute") == null ? "" : t.Attribute("substitute").Value); a2.Text = (t.Attribute("add") == null ? "" : t.Attribute("add").Value); break;
                        case 3: t3.Text = t.Attribute("text").Value; r3.Text = t.Attribute("replace").Value; s3.Text = (t.Attribute("substitute") == null ? "" : t.Attribute("substitute").Value); a3.Text = (t.Attribute("add") == null ? "" : t.Attribute("add").Value); break;
                        case 4: t4.Text = t.Attribute("text").Value; r4.Text = t.Attribute("replace").Value; s4.Text = (t.Attribute("substitute") == null ? "" : t.Attribute("substitute").Value); a4.Text = (t.Attribute("add") == null ? "" : t.Attribute("add").Value); break;
                    }
                    i++;
                }
            }

            foreach (XElement bookmark in container.Descendants("reference").OrderBy(p => p.Attribute("to").Value))
            {
                dataGridView1.Rows.Add(
                    bookmark.Attribute("to").Value
                    , ""
                    , (bookmark.Element("title") == null ? "" : bookmark.Element("title").Attribute("text").Value)
                    );
            }
        }

        private void ReloadBookmarks()
        {
            XElement tokens = null;
            if (_CONTAINER.Descendants("token").Count() != 0)
                _CONTAINER.Descendants("token").Remove();
            if (_CONTAINER.Descendants("tokens").Count() == 0)
            {
                tokens = new XElement("tokens");
                _CONTAINER.Add(tokens);
            }
            else
            {
                tokens = _CONTAINER.Descendants("tokens").First();
            }
            for (int i = 1; i < 5; i++)
            {
                string token = "";
                string replace = "";
                switch (i)
                {
                    case 1: token = t1.Text; replace = r1.Text; if (t1.Text != "") tokens.Add(new XElement("token", new XAttribute("text", t1.Text), new XAttribute("replace", r1.Text), new XAttribute("substitute", s1.Text), new XAttribute("add", a1.Text))); break;
                    case 2: token = t2.Text; replace = r2.Text; if (t2.Text != "") tokens.Add(new XElement("token", new XAttribute("text", t2.Text), new XAttribute("replace", r2.Text), new XAttribute("substitute", s2.Text), new XAttribute("add", a2.Text))); break;
                    case 3: token = t3.Text; replace = r3.Text; if (t3.Text != "") tokens.Add(new XElement("token", new XAttribute("text", t3.Text), new XAttribute("replace", r3.Text), new XAttribute("substitute", s3.Text), new XAttribute("add", a3.Text))); break;
                    case 4: token = t4.Text; replace = r4.Text; if (t4.Text != "") tokens.Add(new XElement("token", new XAttribute("text", t4.Text), new XAttribute("replace", r4.Text), new XAttribute("substitute", s4.Text), new XAttribute("add", a4.Text))); break;
                }
            }
            _CONTAINER.Parent.Save(global.m_forarbeidertoimport);

            dataGridView1.Rows.Clear();
            foreach (XElement bookmark in _CONTAINER.Descendants("reference").OrderBy(p => p.Attribute("to").Value))
            {

                string fromDocument = bookmark.Attribute("from_document").Value;
                string fromBm = bookmark.Attribute("from").Value;
                string id = "";
                string ctitle = "";

                
                string bm = bookmark.Attribute("to").Value;
                string bmR = "";
                string regexp = "";
                for (int i = 1; i < 5; i++)
                {
                    string token = "";
                    string replace = "";
                    string substitute = "";
                    string add = "";
                    switch (i)
                    {
                        case 1: token = t1.Text; replace = r1.Text; substitute = s1.Text; add = a1.Text; break;
                        case 2: token = t2.Text; replace = r2.Text; substitute = s2.Text; add = a2.Text; break;
                        case 3: token = t3.Text; replace = r3.Text; substitute = s3.Text; add = a3.Text; break;
                        case 4: token = t4.Text; replace = r4.Text; substitute = s4.Text; add = a4.Text; break;
                    }
                    if (token != "")
                    {
                        regexp = token;
                        if (Regex.IsMatch(bm, regexp, RegexOptions.IgnoreCase))
                        {
                            bmR = Regex.Replace(bm, token, replace);

                            if (substitute.Split(';').Count() != 0)

                                bmR = Regex.Replace(bmR, substitute.Split(';')[0], substitute.Split(';')[1]);
                            string newBMR = "";
                            foreach (string s in bmR.Split(substitute.Split(';')[1].ToCharArray()[0]))
                            {
                                try
                                {
                                    int x = Convert.ToInt32(s);
                                    newBMR = (newBMR == "" ? newBMR : newBMR + substitute.Split(';')[1]) + x.ToString();
                                }
                                catch
                                {
                                    newBMR = (newBMR == "" ? newBMR : newBMR + substitute.Split(';')[1]) + s;
                                }
                            }
                            if (newBMR != "") bmR = newBMR;
                            if (add != "")
                                bmR = add + bmR;

                            if (bmR != "")
                            {
                                //string title2 = Regex.Replace(bookmark.Element("title").Attribute("text").Value.Trim().ToLower(), @"\s+", "");
                                if (_DOCUMENT.Descendants("section").Where(p => (p.Element("title") == null ? "" : p.Element("title").Value).TrimStart().ToLower().StartsWith(bmR.TrimStart().ToLower())).Count() != 0)
                                {
                                    XElement section = _DOCUMENT.Descendants("section").Where(p => (p.Element("title") == null ? "" : p.Element("title").Value).TrimStart().ToLower().StartsWith(bmR.TrimStart().ToLower())).First();
                                    if (section.Element("title").Value.TrimStart().ToLower().Length > bmR.TrimStart().ToLower().Length)
                                    {
                                        if (section.Element("title").Value.TrimStart().ToLower().StartsWith(bmR.TrimStart().ToLower() + " "))
                                        {
                                            id = section.Attribute("id").Value;
                                            ctitle = section.Element("title").Value;
                                        }
                                    }
                                    else
                                    {
                                        id = section.Attribute("id").Value;
                                        ctitle = section.Element("title").Value;
                                    }
                                }
                            }

                            if (id != "")
                            {
                                dataGridView1.Rows.Add(
                                      bookmark.Attribute("to").Value
                                    , bmR
                                    , fromDocument
                                    , fromBm
                                    , ctitle
                                    , id
                                    );
                            }
                            break;
                        }
                    }
                }

                if (id == "")
                {
                    dataGridView1.Rows.Add(
                          bookmark.Attribute("to").Value
                        , bmR
                        , fromDocument
                        , fromBm
                        , ctitle
                        , id
                        );
                }


            }

        }

        private void btnKoble_Click(object sender, EventArgs e)
        {
            ReloadBookmarks();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void limInnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string returnText = Clipboard.GetText(TextDataFormat.Text);
                DataGridViewCell c = dataGridView1.CurrentCell;
                c.Value = returnText;
            }

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            XElement references =  new XElement("references");
            foreach (DataGridViewRow r in dataGridView1.Rows)
            {
                //if (r.Cells["topicBmId"].Value.ToString() != "")
                //{
                //     references.Add(new XElement("reference"
                //   , new XAttribute("from_id", from_id)
                //   , (from_bm == "" ? null : new XAttribute("from_bm", from_bm))
                //   , new XAttribute("to_bm", to_bm)
                //   ));

                //}
                //r.Cells["bookmarkOrg"].Value;
                //r.Cells["bookmarkReset"].Value;
                //r.Cells["topicName"].Value;
                //r.Cells["topicBm"].Value;
                //r.Cells["topicBmTitle"].Value;
                //r.Cells["topicBmId"].Value;

            }
        }
    }
}
