using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TransformData
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmTransformParent());
            //Application.Run(new frmImportLD());
            //Application.Run(new Form1());
            //Application.Run(new frmXmlTest());
            //Application.Run(new frmHtmlImport());
            //Application.Run(new frmProject());
            //Application.Run(new frmReferance());
            //Application.Run(new frmConvert());
            
            //Application.Run(new frmReadDocument());
            //Application.Run(new frmSectionFile());
            //Application.Run(new frmTest());
            //Application.Run(new frmRegexEditor());
            //Application.Run(new frmIndexIFRS());
        }
    }
}
