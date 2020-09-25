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
using DIB.RegExp.ExternalStaticLinks;
using System.IO;
using DIB.InTextLinking;
namespace TransformData
{
    public partial class frmReferance : Form
    {

        private Dictionary<string, string> _regexps = null;
        
        public frmReferance()
        {
            InitializeComponent();
            cbListOfRegExp.DropDown +=new EventHandler(cbListOfRegExp_DropDown);
        }


        private void cbListOfRegExp_DropDown(object sender, EventArgs e)
        {
            if (File.Exists(textBox2.Text))
            {
                XElement regexps = XElement.Load(textBox2.Text);
                foreach (XElement regexp in regexps.Descendants("regexp"))
                {
                    string id = regexp.Element("id")==null ? "" : regexp.Element("id").Value;
                    if (id != "")
                        cbListOfRegExp.Items.Add(id); 
                }
            }
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                rb1.Checked = true;
                reg1.Checked = true;

            }
            
        }

        private Dictionary<string, string> BuildRegexp()
        {
            
            Dictionary<string, string> returnValue = null;
            if (!File.Exists(textBox2.Text)) return returnValue;
            try
            {
                XElement regExp = XElement.Load(textBox2.Text);
                ReadRegExExpressionsEx1 rBuild = new ReadRegExExpressionsEx1(0);

                Dictionary<string, string> dict = rBuild.Build_Regexp_Dictionary(regExp);
                returnValue = dict;
            }
            catch (SystemException e)
            {
                MessageBox.Show("BuildRegexp - Error:" + e.Message.ToString());
            }
            return returnValue;
        }

        private void btnReadRef_Click(object sender, EventArgs e)
        {

            //if (!File.Exists(textBox1.Text)) return;
            //FileInfo f = new FileInfo(textBox1.Text);
            //string path = f.DirectoryName;
            //XDocument d = XDocument.Load(textBox1.Text);
            //XElement xe = d.Root;
            XElement xe = null;
            //ExternalStaticLinksEx1 el = new ExternalStaticLinksEx1(0);
            ExtendedStaticLinks el = new  ExtendedStaticLinks(0);
            if (reg2.Checked)
            {
                if (File.Exists(textBox2.Text))
                {

                    _regexps = BuildRegexp();
                    if (_regexps == null)
                    {
                        MessageBox.Show("Kunne ikke bygge regexp.xml!");
                    }
                    if (!el.SetRegexpList(_regexps))
                    {
                        MessageBox.Show("Kunne ikke laste regexp.xml!");
                        return;
                    }

                }
            }

            if (rb2.Checked)
            {
                if (File.Exists(textBox3.Text))
                {
                    XElement actions = null;
                    try
                    {
                        actions = XElement.Load(textBox3.Text);
                    }
                    catch
                    {
                        MessageBox.Show("Kunne ikke laste actions.xml!");
                        return;
                    }
                    if (!el.SetActionsList(actions))
                    {
                        MessageBox.Show("Kunne ikke sette variabel actions!");
                        return;
                    }

                }
            }

            xe = new XElement("root", new XElement("content", new XElement("p", "I lov 12. juni 1981 nr. 52 om verdipapirfond § 2-5 nytt tredje ledd")));
            
            XElement missing = new XElement("missing");
            //el.GetExternalStaticLinks(xe, "no",ref missing);
            //MessageBox.Show(el.MatchCounter.ToString() + " RegExp Match, " +  el.LinkCounter.ToString() + " linker lagt til dokumentet!");
            //missing.Save(path + @"\missing.xml");
            //xe.Save(path + @"\test.xml");
            XElement links = new XElement("links");
            el.GetStaticLinks(xe, "no", ref links );
        }

        private void btnTestRegexp_Click(object sender, EventArgs e)
        {
            Form f = null;
            _regexps = BuildRegexp();
            if (_regexps == null)
            {
                MessageBox.Show("Kunne ikke bygge regexp.xml!");
                return;
            }

            if (!File.Exists(textBox3.Text))
            {
                MessageBox.Show("Finner ikke actions file: '" + textBox3.Text + "'!");
                return;
            }
            
            

            if (_regexps != null && File.Exists(textBox3.Text))
            {
                f = new frmTestXMLEx(_regexps, textBox2.Text, textBox3.Text);
            }
            else
            {
                f = new frmTestXMLEx(textBox2.Text);
            }
            f.Show();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void cbListOfRegExp_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnValidate_Click(object sender, EventArgs e)
        {
            if (cbListOfRegExp.Text != "")
            {
                if (File.Exists(textBox2.Text))
                {
                    XElement regexps = XElement.Load(textBox2.Text);
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
                            result.Add(top);
                            
                            string fileName = @"C:/_test/_regexpTest.xml";
                            result.Save(fileName);
                        }
                    }
                }
            }
        }
    

    }
}
