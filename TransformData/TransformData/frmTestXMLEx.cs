using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DIB.RegExp.ExternalStaticLinks;

namespace TransformData
{
    public partial class frmTestXMLEx : Form
    {
        public string _regexpFileName = "";
        public XElement _ACTIONS = null;
        public string _ACTIONSFILENAME = "";
        public frmTestXMLEx(string filename)
        {
            _regexpFileName = filename;
            InitializeComponent();
        }

        Dictionary<string, string> _regexps = null;
        private Dictionary<string, string> BuildRegexp()
        {

            Dictionary<string, string> returnValue = null;
            if (!File.Exists(_regexpFileName)) return returnValue;
            try
            {
                XElement regExp = XElement.Load(_regexpFileName);
                ReadRegExExpressionsEx1 rBuild = new ReadRegExExpressionsEx1(0);

                Dictionary<string, string> dict = rBuild.Build_Regexp_Dictionary(regExp);
                returnValue = dict;
            }
            catch (SystemException e)
            {
                throw new SystemException(e.Message);
            }
            return returnValue;
        }

        private void LoadRegexp(Dictionary<string, string> regexps)
        {
            _regexps = regexps ;
            cbRegexp.Items.Clear();
            foreach (KeyValuePair<string, string> k in regexps)
            {
                cbRegexp.Items.Add(k.Key);
            }
        }
        
        public frmTestXMLEx(Dictionary<string,string> regexps, string filename, string  actionsFileName)
        {
            InitializeComponent();
            _ACTIONSFILENAME = actionsFileName;
            _ACTIONS = XElement.Load(_ACTIONSFILENAME);
            _regexpFileName = filename;
            LoadRegexp(regexps);
        }

        private class qResult
        {
            public int n { get; set; }
            public string name { get; set; }
            public string value { get; set; }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                
                string reg = textBox1.Text;
                Regex r = new Regex(reg);
                
                
                string text = textBox2.Text;
                textBox3.Text = "";
                textBox3.Refresh();
                int i = 0;
                var grpNames = r.GetGroupNames();
                foreach (Match m in r.Matches(text))
                {
                    GroupCollection groups = m.Groups;
                    List<qResult> qr = new List<qResult>();
                    foreach (var grpName in grpNames.Where(p=>!Regex.IsMatch(p,@"^\d+$")))
                    {
                        if (groups[grpName].Success)
                        {
                            foreach (Capture c in groups[grpName].Captures)
                            {
                                qResult re = new qResult();
                                re.n = c.Index;
                                re.name = grpName;
                                re.value = c.Value;
                                qr.Add(re);
                                
                            }
                        }
                    }
                    foreach (qResult q in qr.OrderBy(p=>p.n))
                    {
                        textBox3.Text = textBox3.Text + q.n.ToString().PadLeft(5, ' ') + " :  " + q.name.PadRight(20, ' ') + "->" + q.value + "\r\n";
                    }

                    //XElement mainActions = _ACTIONS.Descendants("action").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == "total").FirstOrDefault();
                    //mainActions.ExecuteAction(m, textBox3);

                    //if (textBox3.Text == "")
                    //{
                    //    ExtractMatces(m, ref i);
                    //}

                }
            }
            catch (SystemException error)
            {
                MessageBox.Show(error.ToString());
            }
        }

        private void ExtractMatces(Match m, ref int i )
        {
            textBox3.Text = textBox3.Text + "Match: " + i.ToString() + ": '" + m.Value + "'\r\n";

            int n = 0;
            n = m.Groups["mlovdate"].Captures.Count;
            for (int ii = 0; ii < n; ii++)
            {
                textBox3.Text = textBox3.Text + "mlovdate: '" + m.Groups["mlovdate"].Captures[ii].Value + "'\r\n";
            }

            n = m.Groups["mlovom"].Captures.Count;
            for (int ii = 0; ii < n; ii++)
            {
                textBox3.Text = textBox3.Text + "mlovom: '" + m.Groups["mlovom"].Captures[ii].Value + "'\r\n";
            }

            n = m.Groups["mlovom_name"].Captures.Count;
            for (int ii = 0; ii < n; ii++)
            {
                textBox3.Text = textBox3.Text + "mlovom_name: '" + m.Groups["mlovom_name"].Captures[ii].Value + "'\r\n";
            }



            if (m.Groups["paranr"].Success)
            {

                n = m.Groups["para"].Captures.Count;
                for (int ii = 0; ii < n; ii++)
                {
                    textBox3.Text = textBox3.Text + "para: '" + m.Groups["para"].Captures[ii].Value + "'\r\n";
                }

                for (int ii = 0; ii < m.Groups["pbet"].Captures.Count; ii++)
                {
                    textBox3.Text = textBox3.Text + "pbet: '" + m.Groups["pbet"].Captures[ii].Value + "'\r\n";
                }

                n = m.Groups["paratot"].Captures.Count;
                for (int ii = 0; ii < n; ii++)
                {
                    textBox3.Text = textBox3.Text + "paratot: '" + m.Groups["paratot"].Captures[ii].Value + "'\r\n";
                }

                n = m.Groups["paranr"].Captures.Count;
                for (int ii = 0; ii < n; ii++)
                {
                    textBox3.Text = textBox3.Text + "paranr: '" + m.Groups["paranr"].Captures[ii].Value + "'\r\n";
                }

                n = m.Groups["kapnr"].Captures.Count;
                for (int ii = 0; ii < n; ii++)
                {
                    textBox3.Text = textBox3.Text + "kapnr: '" + m.Groups["kapnr"].Captures[ii].Value + "'\r\n";
                }

                n = m.Groups["delnr"].Captures.Count;
                for (int ii = 0; ii < n; ii++)
                {
                    textBox3.Text = textBox3.Text + "delnr: '" + m.Groups["delnr"].Captures[ii].Value + "'\r\n";
                }


            }
            i++;

        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void cbRegexp_SelectedIndexChanged(object sender, EventArgs e)
        {
            object selectedItem = cbRegexp.SelectedItem;

            string s = _regexps.Where(p => p.Key == selectedItem.ToString()).Select(p => p.Value).FirstOrDefault();
            textBox1.Clear();
            textBox1.Text = s;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {

                _ACTIONS = XElement.Load(_ACTIONSFILENAME);

                Dictionary<string, string> regexps = BuildRegexp();
                if (regexps != null)
                    LoadRegexp(regexps);
            }
            catch (SystemException err)
            {
                MessageBox.Show(err.Message);
            }
        }

    }

    static class MyExtentions
    {
        
        public static void ExecuteAction(this XElement actions, Match m, TextBox textbox)
        {
            bool bBreak = false;
            bool bMatchAction = false;
            bool bMatchFound = false;
            if (actions.Elements().Count() == 0) return;
            foreach (XElement action in actions.Elements())
            #region //foreach action
            {

                string preText = "";
                switch (action.Name.LocalName)
                {
                    case "internal":
                        textbox.Text = textbox.Text + "INTERNAL action\r\n";
                        action.ExecuteAction(m, textbox);
                        break;
                    case "runaction":
                        #region
                        {
                            string aName = action.Attribute("name")==null ? "" : action.Attribute("name").Value;
                            if (aName == "")
                            {
                                textbox.Text = textbox.Text + "RUNACTION mangler navn\r\n";
                            }
                            else
                            {
                                XElement newAction = action
                                    .Ancestors()
                                    .Last()
                                    .Descendants("action")
                                    .Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == aName)
                                    .FirstOrDefault();

                                if (newAction == null)
                                {
                                    textbox.Text = textbox.Text + "RUNACTION finner ikke action '" + aName + "'\r\n";
                                }
                                else
                                {
                                    newAction.ExecuteAction(m, textbox);
                                }
                            }
                        }
                        #endregion
                        break;
                    case "true":
                    case "false":
                    case "action":
                        #region
                        action.ExecuteAction(m, textbox);
                        bBreak = true;
                        break;
                        #endregion
                    case "foreach":
                        #region
                        string queryType = (action.Attribute("querytype") == null ? "" : action.Attribute("querytype").Value);
                        string groups =  (action.Attribute("groups") == null ? "" : action.Attribute("groups").Value);
                        if (queryType == "group")
                        {
                            int n = m.Groups[groups].Captures.Count;
                            for (int i = 0; i < n; i++)
                            {

                            Capture c = m.Groups[groups].Captures[i];
                                textbox.Text = textbox.Text + "FOREACH: group='" + groups + "' value='" + c.Value + "'\r\n";
                            }
                        }
                        else
                        {
                            textbox.Text = textbox.Text + "FOREACH Error: Finnes ikke  '" + queryType +  "' \r\n";
                        }
                        bBreak = true;
                        break;
                        #endregion
                    case "match":
                        #region
                        bMatchAction = true;
                        string tMatchs = action.Attribute("name") == null ? "" : action.Attribute("name").Value;
                        if (tMatchs=="")
                        {
                            textbox.Text = textbox.Text + "Match Error: Uten navn\r\n";
                        }
                        else
                        {
                            bool bMatch = true;
                            foreach (string tMatch in tMatchs.Split('|'))
                            if (!m.Groups[tMatch].Success)
                            {
                                bMatch = false;
                                break;
                            }

                            if (bMatch)
                            {
                                bMatchFound = true;
                                textbox.Text = textbox.Text + "MATCHES: '" + tMatchs + "'\r\n";
                                foreach (string tMatch in tMatchs.Split('|'))
                                {
                                    textbox.Text = textbox.Text + "MATCH: '" + tMatch + "'='" + m.Groups[tMatch].Value + "'\r\n";
                                }
                                action.ExecuteAction(m, textbox);
                                bBreak = true;
                            }
                        }
                        break;
                        #endregion

                    case "get": 
                    case "mark": 
                    #region
                        preText = action.Name.LocalName == "get" ? "GET" : "MARK";
                        string tagValue = "";
                        string tTagname = action.Attribute("tag") == null ? "" : action.Attribute("tag").Value;
                        string tGroups = action.Attribute("groups") == null ? "" : action.Attribute("groups").Value;
                        if (tTagname == "" || tGroups == "")
                        {
                            if (tTagname == "" && preText == "GET")
                                textbox.Text = textbox.Text + preText + " Error: tag ikke angitt\r\n";

                            if (tGroups == "")
                                textbox.Text = textbox.Text + preText  + " Error: groups ikke angitt\r\n";
                        }
                        else
                        {
                            bool bMatch = true;
                            foreach (string tGroup in tGroups.Split('|'))
                            {
                                if (!tGroup.StartsWith("$") && !tGroup.EndsWith("$"))
                                {
                                    if (!m.Groups[tGroup].Success)
                                    {
                                        bMatch = false;
                                        break;
                                    }
                                    else
                                    {
                                        tagValue = tagValue + m.Groups[tGroup].Value;
                                    }
                                }
                                else
                                {
                                    tagValue = tagValue + tGroup.Replace("$", "");
                                }
                            }
                            if (bMatch)
                            {
                                textbox.Text = textbox.Text + preText + ": " + (preText =="MARK" ? "" : "'" + tTagname + "' =") + " '" + tagValue + "'\r\n";
                                action.ExecuteAction(m, textbox);
                            }
                            else
                            {
                                textbox.Text = textbox.Text + preText + ": " + (preText == "MARK" ? "" : tTagname + "=") + "ikke funnet!\r\n";
                                action.ExecuteAction(m, textbox);
                            }
                        }
                        bBreak = true;
                        break;
                    #endregion

                }
                if (bBreak) break;
            }
            #endregion
            if (bMatchAction && !bMatchFound && (actions.Attribute("name") == null ? "" : actions.Attribute("name").Value) != "test")
            {
                textbox.Text = textbox.Text + "Match ikke funnet i total!\r\n";
                XElement test = actions.Ancestors("actions").First().Descendants("action").Where(p => (p.Attribute("name") == null ? "" : p.Attribute("name").Value) == "test").FirstOrDefault();
                test.ExecuteAction(m, textbox);
            }

        }

    }
}
