using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using DIB.RegExp.ExternalStaticLinks;
using System.Runtime.InteropServices;


namespace TransformData
{
    public partial class frmIndexIFRS : Form
    {
        public frmIndexIFRS()
        {
            InitializeComponent();
            listView1.ItemDrag += new ItemDragEventHandler(listView1_ItemDrag);
            listView1.DragDrop += new DragEventHandler(listView1_DragDrop);
            listView1.DragEnter += new DragEventHandler(listView1_DragEnter);
            listView1.DragOver += new DragEventHandler(listView1_DragOver);
            listView1.DoubleClick += new EventHandler(listView1_DoubleClick);
        }

        void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            if (listView1.SelectedItems.Count > 1)
            {
                MessageBox.Show("Flere enn en rad valgt!");
                return;
            }
            Form f = new frmXMLAttributesEdit(listView1.SelectedItems[0].Tag as XElement);
            f.ShowDialog();

        }

        void listView1_DragOver(object sender, DragEventArgs e)
        {
        }

        void listView1_DragDrop(object sender, DragEventArgs e)
        {
            
            Point cp = listView1.PointToClient(new Point(e.X, e.Y));
            ListViewItem dragToItem = listView1.GetItemAt(cp.X, cp.Y);
            if (dragToItem == null) return; 
            int dropIndex = dragToItem.Index;
            int idx = dragToItem.Index;

            foreach (ListViewItem item in listView1.SelectedItems)
            {
                XElement el = new XElement(item.Tag as XElement);
                item.Remove();

                listView1.Items.Insert(idx, CreateListViewItem(el));
                idx++;
            }
        }
        
        void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Link);
        }

        void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Link;
            
        }


        private void GetElements(XElement document, ref XElement root)
        {
            if (root.Element("documents") == null) root.Add(new XElement("documents"));
            if (root.Element("elements") == null) root.Add(new XElement("elements"));

            string name = document.Name.LocalName;
            string parentName = document.Parent == null ? "" : document.Parent.Name.LocalName;

            if (parentName == "")
            {
                if (root.Element("documents").Elements().Where(p => p.Name.LocalName == document.Name.LocalName).Count() == 0)
                    root.Element("documents").Add(new XElement(document.Name.LocalName));
            }
            else
            {
                if (root.Element("documents").Elements().Where(p => p.Name.LocalName == parentName).Count() != 0)
                {
                    if (root.Element("documents").Elements().Where(p => p.Name.LocalName == parentName).Elements().Where(p => p.Name == document.Name.LocalName).Count() == 0)
                    {
                        root.Element("documents").Elements().Where(p => p.Name.LocalName == parentName).First().Add(new XElement(document.Name.LocalName));
                        if (root.Element("elements").Elements().Where(p => p.Name.LocalName == document.Name.LocalName).Count() == 0)
                        {
                            root.Element("elements").Add(new XElement(document.Name.LocalName));
                        }
                    }

                }
                else
                {
                    if (root.Element("elements").Elements().Where(p => p.Name.LocalName == parentName).Count() == 0)
                    {
                        root.Element("elements").Add(new XElement(parentName));
                    }
                    if (root.Element("elements").Elements().Where(p => p.Name.LocalName == parentName).Elements().Where(p => p.Name == document.Name.LocalName).Count() == 0)
                        root.Element("elements").Elements().Where(p => p.Name.LocalName == parentName).First().Add(new XElement(document.Name.LocalName));
                }

            }
            foreach (XElement e in document.Elements())
            {
                GetElements(e, ref root);
            }
        }


        private string ElementText(XElement e)
        {
            string returnValue = "";
            foreach (XNode n in e.Nodes())
            {
                if (n.NodeType == XmlNodeType.Element)
                {
                    if (((XElement)n).Name == "linebreak")
                        returnValue = returnValue + " ";
                    else if (((XElement)n).Name == "edu_insert")
                        returnValue = returnValue + "";
                    else if (((XElement)n).Name == "edu_para")
                        returnValue = returnValue + "";
                    else if (((XElement)n).Name == "footnote")
                        returnValue = returnValue + "";
                    else
                        returnValue = returnValue + ElementText((XElement)n);
                }
                else
                {
                    returnValue = returnValue + Regex.Replace(n.ToString(), "\r\n|\n\r|\r|\n", " ");
                    returnValue = returnValue.Replace((char)0x2009, '-');
                    returnValue = returnValue.Replace((char)0x2010, '-');
                    returnValue = returnValue.Replace((char)0x2011, '-');
                    returnValue = returnValue.Replace((char)0x2013, '-');
                    returnValue = Regex.Replace(returnValue, @"-", "-");
                }
            }
            return returnValue;
        }

        private XElement GetFileInfo(string path, string versjon, XDocument result, ref XElement root)
        {
            try
            {
                XDocument newDoc = XDocument.Load(path);
                XElement document = null;
                if (newDoc != null)
                {
                    if (newDoc.Root.Name.LocalName == "links") return null;
                    GetElements(newDoc.Root, ref root);
                    document = new XElement("document");

                    document.Add(new XElement("name", newDoc.Root.Name));

                    if (newDoc.Root.Attribute("id") != null)
                    {
                        document.Add(new XElement("id", newDoc.Root.Attribute("id").Value.ToString()));
                    }

                    if (newDoc.Root.Attribute("type") != null)
                        document.Add(new XElement("type", newDoc.Root.Attribute("type").Value));
                    else
                        document.Add(new XElement("type", newDoc.Root.Name));

                    if (newDoc.Root.Attribute("number") != null)
                        document.Add(new XElement("number", newDoc.Root.Attribute("number").Value));

                    if (newDoc.Root.Attribute("header") != null)
                        document.Add(new XElement("header", newDoc.Root.Attribute("header").Value));
                    //if (newDoc.Root.Element("title") == null || newDoc.Root.Element("title").Value.Trim() == "") Debug.Print("No title");

                    if ((newDoc.Root.Element("title") == null ? "" : ElementText(newDoc.Root.Element("title")).Trim()) != "")
                    {
                        document.Add(new XElement("title", ElementText(newDoc.Root.Element("title")).Trim()));
                    }
                    else if (newDoc.Root.Descendants("title").Where(p => p.Value != "").Count() != 0)
                    {
                        document.Add(new XElement("title", ElementText(newDoc.Root.Descendants("title").Where(p => p.Value != "").First()).Trim()));
                    }

                    XElement file = new XElement("file");
                    XAttribute version = new XAttribute("version", versjon);
                    file.Add(version);

                    XAttribute newPath = new XAttribute("path", path);

                    file.Add(newPath);

                    document.Add(file);
                }

                return document;
            }
            catch
            {
                return null;
            }
        }


        private XElement GetXMLDirectoryInfo(string path)
        {
            XElement returnVal = null;
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] bmpfiles = dir.GetFiles("*.xml");


            foreach (FileInfo f in bmpfiles)
            {
                try
                {
                    XDocument d = XDocument.Load(f.FullName);
                    if (d != null)
                    {
                        string name = d.Root.Name.LocalName;

                        string id = (string)d.Root.Attributes("id").FirstOrDefault();
                        string type = (string)d.Root.Attributes("type").FirstOrDefault();
                        string number = (string)d.Root.Attributes("number").FirstOrDefault();
                        string header = (string)d.Root.Attributes("header").FirstOrDefault();
                        string filename = f.Name;
                        string title = d.Root.Elements("title")
                                    .DescendantNodes()
                                    .Select(p =>
                                        p.NodeType == XmlNodeType.Element ?
                                        (((XElement)p).Name.LocalName == "linebreak" ? new XText(" ") : p)
                                        :
                                        (p.NodeType == XmlNodeType.ProcessingInstruction ?
                                        (((XProcessingInstruction)p).Target == "Pub" && ((XProcessingInstruction)p).Data.StartsWith(@"_newline") ? new XText(" ") : null) : p)
                                        )
                                    .OfType<XText>().Where(p =>
                                                        p.Ancestors("edu_insert").FirstOrDefault() == null
                                                        && p.Ancestors("edu_para").FirstOrDefault() == null
                                                        && p.Ancestors("footnote").FirstOrDefault() == null
                                                        ).Select(p => p.ToString()).StringConcatenate(" ");

                        if (number != null)
                            number = number.PadLeft(3, '0');
                        
                        if (returnVal == null) returnVal = new XElement("documents");

                        returnVal.Add(new XElement("document",
                            header == null ? null : new XAttribute("header", header),
                            title == null ? null : new XAttribute("title", title),
                            id == null ? null : new XAttribute("id", id),
                            type == null ? null : new XAttribute("type", type),
                            number == null ? null : new XAttribute("number", number),
                            filename == null ? null : new XAttribute("filename", filename)
                            ));
                    }
                }
                catch
                {

                }
            }
            if (returnVal != null)
            {
                returnVal = new XElement("documents",  
                                         returnVal.Elements("document")
                                        .OrderBy(p =>(p.Attribute("type") == null ? "" : p.Attribute("type").Value))
                                        .ThenBy(p=>(p.Attribute("number") == null ? "" : p.Attribute("number").Value))
                                        .Select(p => p));
            }

            return returnVal;
        }

        private XDocument GetDirectoryInfo(string path, string versjon, ref XElement el)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] bmpfiles = dir.GetFiles("*.xml");

            XDocument result = new XDocument();
            XElement root = new XElement("root");

            foreach (FileInfo f in bmpfiles)
            {
                
                
                XElement document = GetFileInfo(f.FullName, versjon, result, ref el);
                if (document != null)
                    root.Add(document);

            }

            if (root.HasElements)
            {
                result.Add(root);
                return result;
            }
            else
            {
                return null;
            }

        }

        private void ReadIndexFromFolder()
        {
            if (txtFolder.Text == "") return;
            if (!Directory.Exists(txtFolder.Text)) return;
            DirectoryInfo dir = new DirectoryInfo(txtFolder.Text);
            XElement el = new XElement("root");

            XDocument newInfo = GetDirectoryInfo(txtFolder.Text, "2011", ref el);

            string indexPath = txtFolder.Text + @"\index";
            if (!Directory.Exists(indexPath)) Directory.CreateDirectory(indexPath);
            el.Save(indexPath + @"\elements.xml");

            XElement content = new XElement("content");

            //Debug.Print("INNLEDNING");

            foreach (XElement d in newInfo.Descendants("document")
            .Where(p => p.Element("type").Value == "introduction")
            .OrderBy(p => p.Element("number") == null ? "" : p.Element("number").Value))
            {
                content.Add(new XElement(d));
                //Debug.Print(d.Element("title").Value);
            }

            //Debug.Print("HANDBOOK");

            foreach (XElement d in newInfo.Descendants("document")
                .Where(p => p.Element("type").Value == "HB" && p.Element("number").Value != "0000")
                .OrderBy(p => Convert.ToInt32(p.Element("number") == null ? "" : p.Element("number").Value.Replace("0000", "-0"))))
            {
                content.Add(new XElement(d));
                //Debug.Print(d.Element("title").Value);
            }

            //Debug.Print("IFRSPREFACE");
            foreach (XElement d in newInfo.Descendants("document")
            .Where(p => p.Element("type").Value == "IFRSPREFACE")
            .OrderBy(p => p.Element("number") == null ? "" : p.Element("number").Value))
            {
                content.Add(new XElement(d));
                //Debug.Print(d.Element("title").Value);
            }

            //Debug.Print("FRAMEWORK");

            foreach (XElement d in newInfo.Descendants("document")
            .Where(p => p.Element("type").Value == "FRAMEWORK")
            .OrderBy(p => p.Element("number") == null ? "" : p.Element("number").Value))
            {
                content.Add(new XElement(d));
                //Debug.Print(d.Element("title").Value);
            }


            //Debug.Print("NEW STANDARD");

            foreach (XElement d in newInfo.Descendants("document")
            .Where(p => p.Element("type").Value == "IFRS" && p.Element("number").Value == "001")
            .OrderBy(p => Convert.ToInt32(p.Element("number") == null ? "" : p.Element("number").Value)))
            {
                content.Add(new XElement(d));
                //Debug.Print(d.Element("number").Value + " " + d.Element("title").Value);
            }



            //Debug.Print("IFRS");
            XElement part = new XElement("document",
                    new XElement("title", "International Financial Reporting Standards (IFRSs)"));



            foreach (XElement d in newInfo.Descendants("document")
                .Where(p => p.Element("type").Value == "IFRS" && p.Element("number").Value != "001")
                .OrderBy(p => Convert.ToInt32(p.Element("number") == null ? "" : p.Element("number").Value)))
            {
                part.Add(new XElement(d));
                //Debug.Print(d.Element("number").Value + " " +  d.Element("title").Value);
            }

            content.Add(part);

            //Debug.Print("IAS");
            part = new XElement("document",
                    new XElement("title", "International Accounting Standards (IASs)"));

            foreach (XElement d in newInfo.Descendants("document")
                .Where(p => p.Element("type").Value == "IAS")
                .OrderBy(p => p.Element("number") == null ? 0 : Convert.ToInt32(p.Element("number").Value)))
            {
                part.Add(new XElement(d));
                //Debug.Print(d.Element("number").Value + " " + d.Element("title").Value);
            }

            content.Add(part);

            //Debug.Print("IFRIC OG SIC");
            //Debug.Print("IFRIC");

            part = new XElement("document",
                new XElement("title", "Interpretations"));

            foreach (XElement d in newInfo.Descendants("document")
                .Where(p => p.Element("type").Value == "IFRIC")
                .OrderBy(p => p.Element("number") == null ? 0 : Convert.ToInt32(p.Element("number").Value)))
            {
                part.Add(new XElement(d));
                //Debug.Print(d.Element("number").Value + " " + d.Element("title").Value);
            }

            //Debug.Print("SIC");

            foreach (XElement d in newInfo.Descendants("document")
                .Where(p => p.Element("type").Value == "SIC")
                .OrderBy(p => p.Element("number") == null ? 0 : Convert.ToInt32(p.Element("number").Value)))
            {
                part.Add(new XElement(d));
                //Debug.Print(d.Element("number").Value + " " + d.Element("title").Value);
            }
            content.Add(part);

            //Debug.Print("HANDBOOK");

            foreach (XElement d in newInfo.Descendants("document")
                .Where(p => p.Element("type").Value == "HB" && p.Element("number").Value == "0000")
                .OrderBy(p => Convert.ToInt32(p.Element("number") == null ? "" : p.Element("number").Value.Replace("0000", "-0"))))
            {
                content.Add(d);
                //Debug.Print(d.Element("title").Value);
            }

            foreach (XElement d in newInfo.Descendants("document")
                .Where(p => p.Element("name").Value == "glossary"))
            {
                XElement title = new XElement("title", "Glossary of Terms");
                if (d.Element("title") != null)
                    d.Element("title").ReplaceWith(title);
                else
                    d.Add(title);

                content.Add(d);
                //Debug.Print(d.Element("title").Value);
            }


            content.Save(indexPath + @"\content.xml");


        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (txtFolder.Text  == "" ) return;
            XElement documents = null;
            if (rb1.Checked)
            {
                if (!Directory.Exists(txtFolder.Text)) return;
                documents = GetXMLDirectoryInfo(txtFolder.Text);
            }
            else if (rb2.Checked)
            {
                if (!File.Exists(txtFolder.Text)) return;
                try
                {
                    XDocument d = XDocument.Load(txtFolder.Text);
                    documents = d.Root;
                }
                catch
                {
                    MessageBox.Show("Feil ved åpning av XML-fil!");
                    return;

                }
            }
            if (documents != null)
            {
                foreach (XElement el in documents.Elements())
                {
                    this.listView1.Items.Add(CreateListViewItem(el));
                }
            }

        }

        private ListViewItem CreateListViewItem(XElement element)
        {
            string title = (string)element.Attributes("title").FirstOrDefault();
            if (title == null) title = (string)element.Attributes("header").FirstOrDefault();
            ListViewItem lvi = new ListViewItem(new string[] { 
                    title,
                    (string)element.Attributes("id").FirstOrDefault(),
                    (string)element.Attributes("type").FirstOrDefault(),
                    (string)element.Attributes("number").FirstOrDefault(),
                    (string)element.Attributes("filename").FirstOrDefault()
                });
            lvi.Tag = element;
            return lvi;
        }

        private void btnFolderName_Click(object sender, EventArgs e)
        {
            if (rb1.Checked)
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = fbd.SelectedPath;
                }
            }
            else if (rb2.Checked)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "XML filer (*.xml)|*.xml";
                ofd.FilterIndex = 0;
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    txtFolder.Text = ofd.FileName;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (listView1.Items.Count != 0)
            {
                XElement documents = new XElement("document");
                foreach (ListViewItem item in listView1.Items)
                {
                    documents.Add(item.Tag as XElement);
                }

                SaveFileDialog sf = new SaveFileDialog();
                sf.Filter = "XML filer (*.xml)|*.xml";
                sf.FilterIndex = 0;
                if (sf.ShowDialog() == DialogResult.OK)
                {
                    documents.Save(sf.FileName);
                }

            }
        }

        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 0)
            {
                if (MessageBox.Show("Vil du slette " + listView1.SelectedItems.Count.ToString() + " fil(er)?", "Slette", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    for (int i = listView1.SelectedItems.Count; i > 0; i--)
                    {
                        listView1.Items.Remove(listView1.SelectedItems[i - 1]);
                    }
                }
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }




    }

    class ListViewBase : ListView
    {
        private Timer tmrLVScroll;
        private System.ComponentModel.IContainer components;
        private int mintScrollDirection;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        const int WM_VSCROLL = 277; // Vertical scroll
        const int SB_LINEUP = 0; // Scrolls one line up
        const int SB_LINEDOWN = 1; // Scrolls one line down


        public ListViewBase()
        {
            InitializeComponent();
        }
        protected void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrLVScroll = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            //
            // tmrLVScroll
            //
            this.tmrLVScroll.Tick += new System.EventHandler(this.tmrLVScroll_Tick);
            //
            // ListViewBase
            //
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.ListViewBase_DragOver);
            this.ResumeLayout(false);

        }

        protected void ListViewBase_DragOver(object sender, DragEventArgs e)
        {
            Point position = PointToClient(new Point(e.X, e.Y));

            if (position.Y <= (Font.Height / 2))
            {
                // getting close to top, ensure previous item is visible
                mintScrollDirection = SB_LINEUP;
                tmrLVScroll.Enabled = true;
            }
            else if (position.Y >= ClientSize.Height - Font.Height / 2)
            {
                // getting close to bottom, ensure next item is visible
                mintScrollDirection = SB_LINEDOWN;
                tmrLVScroll.Enabled = true;
            }
            else
            {
                tmrLVScroll.Enabled = false;
            }
        }

        private void tmrLVScroll_Tick(object sender, EventArgs e)
        {
            SendMessage(Handle, WM_VSCROLL, (IntPtr)mintScrollDirection, IntPtr.Zero);
        }
    }
}
