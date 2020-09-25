using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Security.Permissions;

namespace TransformData
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
    [System.Runtime.InteropServices.ComVisible(true)]
    public partial class frmEdit : Form
    {
        private WebBrowser myWebBrowser;
        private string _htmlText;
        public string returnHtml = "";

        public frmEdit(XElement text)
        {
            InitializeComponent();
            CreateWebBrowser();
            myWebBrowser.Navigate(System.Windows.Forms.Application.StartupPath + @"\html\full.html");
            myWebBrowser.ObjectForScripting = this;
            _htmlText = (new XElement("html", text.Nodes())).ToString();
        }

        private class EditItem
        {
            public XElement partxml { set; get; }
            public string partname { set; get; }
            public string sectionId { set; get; }
        }

        private void CreateWebBrowser()
        {
            myWebBrowser = new System.Windows.Forms.WebBrowser();
            splitContainer1.Panel1.Controls.Add(myWebBrowser);
            myWebBrowser.Dock = DockStyle.Fill;
            myWebBrowser.Visible = true;
            myWebBrowser.BringToFront();
            myWebBrowser.Navigate(@"about:blank");
            myWebBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(myWebBrowser_DocumentCompleted);


        }

        private void myWebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.ToString().EndsWith("full.html"))
            {
                HtmlElement el = myWebBrowser.Document.GetElementById("elm1");
                el.InnerText = _htmlText;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.HtmlDocument doc = myWebBrowser.Document;
            returnHtml = doc.InvokeScript("save").ToString();
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
