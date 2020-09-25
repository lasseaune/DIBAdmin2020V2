using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using DIB.InTextLinking;
using Dib.AutoRef;

namespace TransformData
{
    public partial class RegexpTest : UserControl
    {
        public XElement _Actions;
        private string _regExpID = "";
        private class qResult
        {
            public int n { get; set; }
            public int length { get; set; }
            public string name { get; set; }
            public string value { get; set; }
            public List<qResultDiff> listqR{get; set;}
        }
        private class qResultDiff
        {
            public int diff { get; set; }
            public qResult qR { get; set; }
        }

        public void ResetActions(XElement actions)
        {
            _Actions = actions;
        }

        public void Reset()
        {
            _regExpID = "";
            tbRegexp.Text = "";
            tbResult.Text = "";
        }
        public void ResetRegexp(string regexp, string id)
        {
            _regExpID = id;
            tbRegexp.Text = regexp;
            tbResult.Text = "";
        }
        public RegexpTest(string regexp, XElement actions, string id)
        {
            
            InitializeComponent();
            _regExpID = id;
            _Actions = actions;
            tbRegexp.Text = regexp;
        }

        private class RegExpAction
        {
            public string type { get; set; }
            public RegExpAction(XElement e)
            {
                this.type = e.Name.LocalName; 
            }
        }

        private class MatchEvaluator
        {
            public string result { get; set; }
            private MatchEvaluator _parent { get; set; }
            public RegExpAction action { get; set; }
            public string groupName { get; set; }
            public IEnumerable<MatchEvaluator> actions { get; set; }
            public MatchEvaluator(XElement e)
            {
                this.action = new RegExpAction(e);
                this.actions = e.Elements()
                    .Select(p => new MatchEvaluator(e));
            }
            public string GetMatchData(Match m)
            {
                foreach (MatchEvaluator action in actions)
                {

                }
                return this.TopParent(this).result = "xx";
            }
            public MatchEvaluator Parent()
            {
                return this._parent;
            }
            public MatchEvaluator TopParent(MatchEvaluator m)
            {
                if (this._parent == null)
                    return m;
                else
                    return this.TopParent(m);
            }

        }

        private IEnumerable<MatchEvaluator> BuildAction(XElement e)
        {
            return e.Elements()
                .Select(p => new MatchEvaluator(e));
                                
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                if (_Actions == null) 
                {
                    MessageBox.Show("Actions er ikke satt!");
                    return;
                }

                Regex rx = new Regex(tbRegexp.Text);
                
                RegexMatchActions rm = new RegexMatchActions(rx, _Actions);
                
                IdElement idEl = new IdElement();
                idEl.text = tbTest.Text;
                rm.Execute(idEl);

                tbResult.Text = "";
                if (tbTest.Text == "") return;
                string reg = tbRegexp.Text;
                Regex r = new Regex(reg);


                string test = tbTest.Text;
                tbResult.Text = "";
                List<string> grpNames = r.GetGroupNames().Where(p => !Regex.IsMatch(p, @"^\d+$")).ToList();


                foreach (Match m in r.Matches(test))
                {
                    GroupCollection groups = m.Groups;

                    //List<match> ma =  matches.Where(p => groups[p.name].Success).ToList();

                    List<qResult> qr = new List<qResult>();
                    foreach (string grpName in grpNames)
                    {
                        if (groups[grpName].Success)
                        {
                            foreach (Capture c in groups[grpName].Captures)
                            {
                                qResult re = new qResult();
                                re.n = c.Index;
                                re.length = c.Length;
                                re.name = grpName;
                                re.value = c.Value;
                                qr.Add(re);
                            }
                        }
                    }

                    if (qr.Count() == 0)
                    {
                        tbResult.Text =  tbResult.Text +  "Ingen navnede grupper: value=" +  m.Value + "\r\n";
                        foreach (Capture capture in m.Captures)
                        {
                            tbResult.Text =  tbResult.Text + "capture: index=" + capture.Index.ToString() + " value=" + capture.Value + "\r\n";
                            try
                            {
                                DateTime date = DateTime.Parse(capture.Value);
                                string returnValue = String.Format("{0:yyyy-MM-dd}", date);
                                tbResult.Text = tbResult.Text + "dato=" + returnValue + "\r\n";
                            }
                            catch
                            {
                                tbResult.Text = tbResult.Text + "kunne ikke parse dato! \r\n";
                            }
                        }
                    }

                    foreach (qResult q in qr.OrderBy(p => p.n).OrderByDescending(q=>q.length))
                    {
                        List<qResultDiff> qrdiffs = new List<qResultDiff>();
                        foreach (qResult o in qr.OrderBy(p => p.n))
                        {
                            if (o.n <= q.n && (o.n + o.length) >= (q.n + q.length))
                            {
                                if (o.n != q.n || o.length != q.length)
                                {
                                    qResultDiff qrdiff = new qResultDiff();
                                    qrdiff.diff = (q.n - o.n) + ((o.n + o.length) - (q.n + q.length));
                                    qrdiff.qR = o;
                                    qrdiffs.Add(qrdiff);
                                }
                            }
                        }
                        if (qrdiffs.Count() != 0)
                        {
                            double min = qrdiffs.Min(p=>p.diff);
                            
                        }
                    }


                    foreach (qResult q in qr.OrderBy(p => p.n))
                    {
                        //if new Rectangle(min, 1, max, 1).(new Rectangle(value, 1, 1, 1));
                        
                        tbResult.Text = tbResult.Text + q.n.ToString().PadLeft(5, ' ') + "," + (q.n + q.length).ToString().PadLeft(5, ' ') + " :  " + q.name.PadRight(20, ' ') + "->" + q.value + "\r\n";
                    }

                }
            }
            catch(SystemException x)
            {
                MessageBox.Show(x.Message);
            }


        }

        private void btmSave_Click(object sender, EventArgs e)
        {
            if (_regExpID == "") return;
            if (tbRegexp.Text.Trim() == "") return;

            try
            {
                Regex r = new Regex(tbRegexp.Text);
            }
            catch(SystemException err)
            {
                MessageBox.Show("Kunne ikke kompilere regex utrykket!\r\nError:\r\n" + err.Message);
            }

            if (MessageBox.Show("Vil du virkelig oppdatere '" + _regExpID + "'?", "Oppdatere server rexexp", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            InTextLinkingData data = new InTextLinkingData(true);
            if (!data.Global_Regexp_Update(_regExpID, tbRegexp.Text, "UpdateApp")) MessageBox.Show("En feil oppstod!");

        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            if (_regExpID == "") return;
            if (tbRegexp.Text.Trim() == "") return;

            try
            {
                Regex r = new Regex(tbRegexp.Text);
            }
            catch (SystemException err)
            {
                MessageBox.Show("Kunne ikke kompilere regex utrykket!\r\nError:\r\n" + err.Message);
            }

            SaveFileDialog sf = new SaveFileDialog();
            //"Vil du lagre '" + _regExpID + "'?", "Lagre regexp fil",, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            sf.Filter = "XML filer (*.xml)|*.xml";
            sf.FilterIndex = 0;
            if (sf.ShowDialog() == DialogResult.OK)
            {
                XElement root = new XElement("root",
                    new XText(tbRegexp.Text)
                    );
                root.Save(sf.FileName);

            }
        }
    }

    public static class Extention
    {
        public static string xxx(this string yy)
        {
            return yy;
        }
        public static IEnumerable<T> AsEnumerable<T>(this MatchCollection enumerable)
        {
            foreach (object item in enumerable)
                yield return (T)item;
        }
    }
}
