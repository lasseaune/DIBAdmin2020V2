using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Regjeringen
{
    public partial class frmDownloadRegjeringen : Form
    {
        public frmDownloadRegjeringen()
        {
            InitializeComponent();
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            string downloadhttp = "";
            if (txtHTTPadress.Text.StartsWith("https://www.regjeringen.no"))
            {
                downloadhttp = txtHTTPadress.Text;
            }
            else
            {
                MessageBox.Show("Angi filname på Regjeringen EPUB!");
                return;
            }
                
            string path = "";
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK)
            {
                path = fbd.SelectedPath + @"\";
                txtTempFolderName.Text = path;
                if (Directory.Exists(txtTempFolderName.Text))
                {
                    Directory.Delete(txtTempFolderName.Text, true);
                }
                
                Directory.CreateDirectory(txtTempFolderName.Text);
            }
            else
                return;



            string filename = downloadhttp.Split('/').LastOrDefault();
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(downloadhttp, path + filename);
            }

            string extention = filename.Split('.').LastOrDefault();
            string newfileName = filename.Replace("." + extention, ".zip");
            ////File.Move(path + filename, newfileName);

            string zipPath = path + filename;
            ZipFile.ExtractToDirectory(zipPath, path);

            return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = txtTempFolderName.Text;
            if (!path.EndsWith(@"\"))
            {
                txtTempFolderName.Text = txtTempFolderName.Text + @"\";
                path = txtTempFolderName.Text;
            }
            if (!Directory.Exists(path + @"OEBPS"))
            {
                MessageBox.Show("OEBPS katalogen eksistere ikke");
                return;
            }

            if (!Directory.Exists(path + @"xml"))
            {
                Directory.CreateDirectory(path + @"xml");
            }
            string myf = File.ReadAllText(path + @"OEBPS\css\styles.css");
            CssParser cp = new CssParser();
            List<CssParserRule> cpr = cp.ParseAll(myf).ToList();

            List<CssParserDeclaration> dc =
                cpr.Where(p => p.Selectors.Where(s => s.ToLower().Split(' ').Contains(".k-sperret")).Count() != 0).SelectMany(p => p.Declarations.Where(d => "font-weight;font-decoration".Split(';').Contains(d.Property))).ToList();


            XElement html = (path + @"OEBPS\").TransformEpubRegEx();

            XElement xslt = new XElement("root",
                html
                .Elements()
                .GroupBy(p => p.Name.LocalName + (((string)p.Attributes("class").FirstOrDefault() ?? "") == "" ? "" : "-" + (string)p.Attributes("class").FirstOrDefault()).Replace(" ", "-").ToLower())
                .Select(p =>
                    new XElement(p.Key,
                        html.Descendants()
                        .Where(a => (a.Name.LocalName + (((string)a.Attributes("class").FirstOrDefault() ?? "") == "" ? "" : "-" + (string)a.Attributes("class").FirstOrDefault()).Replace(" ", "-").ToLower()) == p.Key.ToString())
                        .Select(a => a.Parent)
                        .ToList()
                        .GroupBy(a => a.Name.LocalName + (((string)a.Attributes("class").FirstOrDefault() ?? "") == "" ? "" : "-" + (string)a.Attributes("class").FirstOrDefault()).Replace(" ", "-").ToLower())
                        .Select(a =>
                                new XElement("parent",
                                    new XAttribute("name", a.Key

                                )
                            )
                        ),
                        html.Descendants()
                        .Where(c => (c.Name.LocalName + (((string)c.Attributes("class").FirstOrDefault() ?? "") == "" ? "" : "-" + (string)c.Attributes("class").FirstOrDefault()).Replace(" ", "-").ToLower()) == p.Key)
                        .SelectMany(c => c.Elements())
                        .GroupBy(c => c.Name.LocalName + (((string)c.Attributes("class").FirstOrDefault() ?? "") == "" ? "" : "-" + (string)c.Attributes("class").FirstOrDefault()).Replace(" ", "-").ToLower())
                        .Select(c =>
                            new XElement("element",
                                new XAttribute("name",
                                c.Key
                                )
                            )
                        )
                    )
                )
            );


            xslt.Save(path + "xslt.xml");
            html.Save(path + "html.xml");
            txtHtmlFileName.Text = path + "html.xml";

        }

        private void btnMakeSegment_Click(object sender, EventArgs e)
        {
            if (!File.Exists(txtHtmlFileName.Text))
            {
                MessageBox.Show("Filen: '" + txtHtmlFileName.Text + " finnes ikke!");
                return;
            }
            FileInfo fi = new FileInfo(txtHtmlFileName.Text);
            string path = fi.DirectoryName + @"\";
            XElement html = XElement.Load(txtHtmlFileName.Text);
            XElement document = html.MakeRegjeringenHierarky();
            //XElement document = html;



            document.Descendants("h2").Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimEnd())));
            document.Descendants("h2").Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimStart())));

            document.Descendants("h3").Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimEnd())));
            document.Descendants("h3").Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimStart())));





            document.Descendants().Where(p => p.ElementClassName() == "h1-k-tit-seksjon" && p.Parent.Name.LocalName == "section" && p == p.Parent.Elements().FirstOrDefault()).ToList().ForEach(p => p.ReplaceWith(new XElement("h" + p.Ancestors("section").Count().ToString(), p.Nodes())));
            document.Descendants().Where(p => p.ElementClassName() == "h2-k-tit-seksjon" && p.Parent.Name.LocalName == "section" && p == p.Parent.Elements().FirstOrDefault()).ToList().ForEach(p => p.ReplaceWith(new XElement("h" + p.Ancestors("section").Count().ToString(), p.Nodes())));



            string subelementClassName = "";
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: subelementClassName = "h2-k-tit-undertit"; break;
                    case 1: subelementClassName = "h2-k-tit-mutit"; break;
                    case 2: subelementClassName = "h2-k-tit-mutit2"; break;
                    case 3: subelementClassName = "h2-k-tit-mtit"; break;
                    default: subelementClassName = ""; break;
                }
                if (subelementClassName != "")
                {
                    document
                    .Descendants("section")
                    .Where(p => p.Elements().Where(s => s.ElementClassName() == subelementClassName).Count() > 0)
                    .Reverse()
                    .ToList()
                    .ForEach(p => p.MakeSubSection(subelementClassName));
                }
            }











            document.Save(path + "document.xml");

        }

        private void btnXHTML_Click(object sender, EventArgs e)
        {

            string path = @"D:\_data\_regjeringen\";
            XDocument d = XDocument.Load(path +  @"NOU199619960003000DDDKRNL.xhtml");
            if (d == null) return;
            XElement body = d.Root.Elements("body").FirstOrDefault();
            if (body == null) return;
            XElement document = new XElement("document", body.Nodes());


            document.Descendants("h2").Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimEnd())));
            document.Descendants("h2").Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimStart())));

            document.Descendants("h3").Select(p => p.DescendantNodes().OfType<XText>().LastOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimEnd())));
            document.Descendants("h3").Select(p => p.DescendantNodes().OfType<XText>().FirstOrDefault()).Where(p => p != null).ToList().ForEach(p => p.ReplaceWith(new XText(p.Value.TrimStart())));



            document.Descendants().Where(p => p.ElementClassName() == "h1-k-tit-seksjon" && p.Parent.Name.LocalName == "section" && p == p.Parent.Elements().FirstOrDefault()).ToList().ForEach(p => p.ReplaceWith(new XElement("h" + p.Ancestors("section").Count().ToString(), p.Nodes())));
            document.Descendants().Where(p => p.ElementClassName() == "h2-k-tit-seksjon" && p.Parent.Name.LocalName == "section" && p == p.Parent.Elements().FirstOrDefault()).ToList().ForEach(p => p.ReplaceWith(new XElement("h" + p.Ancestors("section").Count().ToString(), p.Nodes())));



            string subelementClassName = "";
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: subelementClassName = "h2-k-tit-undertit"; break;
                    case 1: subelementClassName = "h2-k-tit-mutit"; break;
                    case 2: subelementClassName = "h2-k-tit-mutit2"; break;
                    case 3: subelementClassName = "h2-k-tit-mtit"; break;
                    default: subelementClassName = ""; break;
                }
                if (subelementClassName != "")
                {
                    document
                    .Descendants("section")
                    .Where(p => p.Elements().Where(s => s.ElementClassName() == subelementClassName).Count() > 0)
                    .Reverse()
                    .ToList()
                    .ForEach(p => p.MakeSubSection(subelementClassName));
                }
            }

            document.Save(path + "document.xml");



        }
    }
}
