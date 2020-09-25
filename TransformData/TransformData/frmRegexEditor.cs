using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using DIB.InTextLinking;
using System.Xml;
using System.Xml.Linq;

namespace TransformData
{
    public partial class frmRegexEditor : Form
    {
        private XElement _actions = null; 
        private List<Category> items = new List<Category>();
        private XElement _regexps = null;
        public frmRegexEditor()
        {
            InitializeComponent();
            cbListOfRegExp.DropDown += new EventHandler(cbListOfRegExp_DropDown);
        }

        void cbListOfRegExp_DropDown(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists(tbFileName.Text))
                {
                    MessageBox.Show("Filen '" + tbFileName.Text + "' finnes ikke!");
                    return;
                }
                //XElement regexps = XElement.Load(tbFileName.Text);
                //foreach (XElement regexp in regexps.Descendants("regexp"))
                //{
                //    string id = regexp.Element("id") == null ? "" : regexp.Element("id").Value;
                //    if (id != "")
                //        cbListOfRegExp.Items.Add(id);
                //}

            }
            catch (SystemException err)
            {
            }
        }

        private class RegexItem
        {
            public string id { get; set; }
            public string parent { get; set; }
            public string type {get; set;}
            public string name { get; set; }
            public XElement expression { get; set; }
            public RegexItem(XElement el)
            {
                switch (el.Name.LocalName)
                {
                    case "regexp":
                        id = el.Element("id").Value;
                        parent = "regexps";
                        
                        type = el.Name.LocalName;
                        name = type;
                        expression = el;
                        break;
                    case "part":
                        //int level = el.Ancestors("part").Count();
                        //int dept = el.NodesBeforeSelf().Count();
                        //id = el.Ancestors("regexp").First().Element("id").Value + ";" + level.ToString() + ";" + dept.ToString();   
                        //type = el.Name.LocalName;
                        id = el.Attribute("id").Value;
                        parent =  (el.Parent.Name.LocalName == "regexp" ? el.Parent.Element("id").Value :  el.Attribute("id").Value);
                        name = el.Name.LocalName;
                        type = el.Attribute("type").Value;
                        expression = el;
                        break;
                }
            }
        }


        private void åpneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cbListOfRegExp.Items.Clear();
            ResetTest();
            openFileDialog1.DefaultExt = ".xml";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbFileName.Text = openFileDialog1.FileName;
                if (File.Exists(tbFileName.Text))
                {
                    try
                    {
                        XElement regexps = XElement.Load(tbFileName.Text);

                        if (regexps.Name.LocalName != "regexps")
                        {
                            MessageBox.Show("Filen '" + tbFileName.Text + "' inneholder ikke XML tag regexps!");
                            return;
                        }
                        LoadRegExps(regexps);
                    }
                    catch (SystemException err)
                    {
                        MessageBox.Show(err.Message);
                    }
                }
            }
        }

        private void validerFilToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(tbFileName.Text) && cbListOfRegExp.Text!="")
            {
                XElement regexps = XElement.Load(tbFileName.Text);
                XElement top = regexps.Descendants("regexp").Where(p => (p.Element("id") == null ? "" : p.Element("id").Value) == cbListOfRegExp.Text).FirstOrDefault();
                if (top != null)
                {
                    ValidateRegexp vr = new ValidateRegexp();
                    XElement result = vr.GetBuild(top, regexps);
                    if (result.Name.LocalName == "errors")
                    {
                        MessageBox.Show(result.ToString());
                    }
                    else
                    {
                        saveFileDialog1.DefaultExt = ".xml";
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            string fileName = saveFileDialog1.FileName;
                            result.Save(fileName);
                        }
                        LoadRegExps(result);
                    }
                }
            }
        }

        private void LoadRegExps(XElement regexps)
        {
            _regexps = regexps;
            List<XElement> re =  regexps.Descendants("regexp").Where(p=>p.Element("id")==null).ToList();
            if (re.Count() != 0) 
            {
                foreach (XElement r in re)
                {
                    MessageBox.Show("regexp mangler id element!\r\n" + r.ToString());
                }
            }
            int id = 1;
            regexps.Descendants("part").Where(p => p.Attribute("id")!=null).ToList().ForEach(p=>p.Attribute("id").Remove());
            regexps.Descendants("part").ToList().ForEach(p => p.Add(new XAttribute("id", id++)));
            List<XElement> pa = regexps.Descendants("part").Where(p=>p.Attribute("id")==null).ToList();
            if (pa.Count() != 0)
            {
                foreach (XElement p in pa)
                {
                    MessageBox.Show("regexp mangler id element!\r\n" + p.ToString());
                }
            }

            twRegexps.Nodes.Clear();
            twRegexps.Nodes.Clear();
            twRegexps.Tag = regexps;
            twRegexps.Nodes.Add(new TreeNode(regexps.Name.LocalName));
            TreeNode tNode = new TreeNode();
            tNode = twRegexps.Nodes[0];
            cbListOfRegExp.Items.Clear();

            foreach (XElement rx in _regexps.Descendants("regexp"))
            {
                string rxId = rx.Element("id") == null ? "" : rx.Element("id").Value;
                if (rxId != "")
                    cbListOfRegExp.Items.Add(rxId);
            }
            AddNode(regexps, tNode);

        }

        private void AddNode(XElement inXElement, TreeNode inTreeNode)
        {
            TreeNode tNode;
            
            int i = 0;

            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXElement.Elements("regexp").Count()!=0)
            {
                foreach (XElement el in inXElement.Elements("regexp"))
                {
                    inTreeNode.Nodes.Add(new TreeNode("<" + el.Name.LocalName + "> " + el.Element("id").Value));
                    tNode = inTreeNode.Nodes[i];
                    tNode.Tag = new RegexItem(el);
                    i++;
                    if (el.Descendants("expression").Count() == 1)
                    {
                        AddParts(el.Descendants("expression").First(), tNode);
                    }

                }
            }
        }

        private void AddParts(XElement inXElement, TreeNode inTreeNode)
        {
            TreeNode tNode;

            int i = 0;
            // Loop through the XML nodes until the leaf is reached.
            // Add the nodes to the TreeView during the looping process.
            if (inXElement.Elements("part").Count() != 0)
            {
                foreach (XElement el in inXElement.Elements("part"))
                {
                    //inTreeNode.Nodes.Add(new TreeNode("<" + el.Name.LocalName + el.Attributes().Select(p=> (" " + p.Name.LocalName + "=" + p.Value).ToString()).StringConcat()  + ">"));
                    inTreeNode.Nodes.Add(new TreeNode(el.Attribute("type").Value));
                    tNode = inTreeNode.Nodes[i];
                    tNode.Tag = new RegexItem(el);
                    i++;
                    AddParts(el, tNode);
                    
                }
            }
        }   
        
        private void buildtree()
        {
            twRegexps.Nodes.Clear();    // Clear any existing items
            twRegexps.BeginUpdate();    // prevent overhead and flicker
            LoadBaseNodes();            // load all the lowest tree nodes
            twRegexps.EndUpdate();      // re-enable the tree
            twRegexps.Refresh();        // refresh the treeview display        
            twRegexps.Nodes[0].Expand();
        }

        private void LoadBaseNodes()
        {
            string baseParent = "";                 // Find the lowest root category parent value
            TreeNode node;
            foreach (Category cat in items)
            {
                if (cat.ParentID == baseParent)
                    baseParent = cat.ParentID;
            }
            foreach (Category cat in items)         // iterate through the categories
            {
                if (cat.ParentID == baseParent)     // found a matching root item 
                {
                    node = twRegexps.Nodes.Add("(" + cat.ID + ") " + cat.NodeText); // add it to the tree
                    node.Tag = cat;                 // send the category into the tag for future processing
                    getChildren(node);              // load all the children of this node
                }
            }
        }
        private void getChildren(TreeNode node)
        {
            TreeNode Node = null;
            Category nodeCat = (Category)node.Tag;  // get the category for this node
            foreach (Category cat in items)         // locate all children of this category
            {
                if (cat.ParentID == nodeCat.ID)     // found a child
                {
                    Node = node.Nodes.Add("(" + cat.ID + ") " + cat.NodeText);    // add the child
                    Node.Tag = cat;                         // set its tag to its category
                    getChildren(Node);                      // find this child's children
                }
            }
        }
        class Category
        {
            public string ID;
            public string ParentID;
            public string NodeText;
            public Category(string ID, string ParentID, string NodeText)
            {
                this.ID = ID;
                this.ParentID = ParentID;
                this.NodeText = NodeText;
            }
            public override string ToString()
            {
                return this.NodeText;
            }
        }

        private void btnBuild_Click(object sender, EventArgs e)
        {
            if (File.Exists(tbFileName.Text) && cbListOfRegExp.Text!="")
            {
                XElement regexps = XElement.Load(tbFileName.Text);
                
                string test = regexps.RegexBuild(cbListOfRegExp.Text);

                ShowTextResult(test, cbListOfRegExp.Text);

                
            }
        }
        private RegexpTest rt = null;

        private void ResetTest()
        {
            cbListOfRegExp.Items.Clear();
            if (splitContainer2.Panel2.Controls.Contains(rt))
            {
                rt.Reset();
            }
        }
        private void ShowTextResult(string text, string id)
        {

            if (splitContainer2.Panel2.Controls.Contains(rt))
            {
                rt.ResetRegexp(text, id);
            }
            else
            {
                splitContainer2.Panel2.Controls.Clear();
                rt = new RegexpTest(text,_actions, id);
                rt.Visible = false;
                splitContainer2.Panel2.Controls.Add(rt);
                rt.Dock = DockStyle.Fill;
                rt.Visible = true;
            }
        }


        private void twRegexps_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByKeyboard || e.Action == TreeViewAction.ByMouse)
            {
                TreeNode n = e.Node;
                RegexItem regexitem = n.Tag as RegexItem;
                if (regexitem == null) return;
                XElement element = regexitem.expression;
                XElement regexps = twRegexps.Tag as XElement;
                string test = element.RegexBuildFromTag(regexps);
                Cursor.Current = Cursors.WaitCursor;
                string id = (element.Element("id") == null ? "" : element.Element("id").Value);
                ShowTextResult(test, id);
                Cursor.Current = Cursors.Default;
            }
        }

        private void åpneAksjonsfillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.DefaultExt = ".xml";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                tbActionsFileName.Text = openFileDialog1.FileName;
                if (File.Exists(tbActionsFileName.Text))
                {
                    try
                    {
                        XElement actions = XElement.Load(tbActionsFileName.Text);

                        if (actions.Name.LocalName != "actions")
                        {
                            MessageBox.Show("Filen '" + tbFileName.Text + "' inneholder ikke XML tag actions!");
                            return;
                        }
                        _actions = actions;
                        if (rt != null)
                            rt._Actions = _actions;
                    }
                    catch (SystemException err)
                    {
                        MessageBox.Show(err.Message);
                    }
                }
            }
        }

        private void åpneTotalRegexpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InTextLinkingData data = new InTextLinkingData(true);
            XElement regexps = data.LoadGlobalXML("total");
            tbFileName.Text = "";
            LoadRegExps(regexps);
        }

        private void lagreToolStripMenuItem_Click(object sender, EventArgs e)
        {
                if (_regexps == null) return;
                SaveFileDialog sf = new SaveFileDialog();
                sf.Filter = "XML filer (*.xml)|*.xml";
                sf.FilterIndex = 0;
                sf.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
                if (sf.ShowDialog() != DialogResult.OK) return;
                _regexps.Save(sf.FileName);
            
        }

        private void egenskaperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (twRegexps.SelectedNode == null) return;

            RegexItem r = twRegexps.SelectedNode.Tag as RegexItem;
            if (r == null) return;
            if (r.name == "part")
            {

            }
            else if (r.name == "regexp")
            {
            }
        }

        private void cbListOfRegExp_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
