using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;

namespace TransformData
{
    [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]    
    [System.Runtime.InteropServices.ComVisible(true)]
    public class WebEventHandler 
    {
        private WebBrowser _WebBrowser;
        public WebEventHandler(WebBrowser w)
        {
            _WebBrowser = w;
            _WebBrowser.Navigating +=new WebBrowserNavigatingEventHandler(_WebBrowser_Navigating);
        }
        public void Submit()
        {
            
        }

        private void _WebBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
        }
    }
}
