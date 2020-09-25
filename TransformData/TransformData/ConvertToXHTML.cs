using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;


namespace Dib.ConvertToXHTML
{
    class ConvertToXHTML
    {
        private Process myProcess = new Process();
        private int elapsedTime;
        private bool eventHandled;


        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            eventHandled = true;
            Debug.Print("Exit time:    {0}\r\n" +
                "Exit code:    {1}\r\nElapsed time: {2}", myProcess.ExitTime, myProcess.ExitCode, elapsedTime);
        }

        public bool ConvertDocumentToXHTML(string htmlFile, string xhtmlFile, string inCharSet, string outCharSet)
        {
            bool returnValue = true;
            try
            {
                eventHandled = false;
                myProcess = new System.Diagnostics.Process();
                myProcess.EnableRaisingEvents = false;
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                myProcess.StartInfo.FileName = @"html2xhtml.exe";
                myProcess.StartInfo.Arguments = htmlFile + " -o " + xhtmlFile + " -c " + inCharSet + " -d " + outCharSet;
                myProcess.EnableRaisingEvents = true;
                myProcess.Exited += new EventHandler(myProcess_Exited);

                myProcess.Start();

            }
            catch (Exception ex)
            {
                Debug.Print("En feil oppstod ved konvertering av  \"{0}\":" + "\n" + ex.Message, htmlFile);
                return false;
            }

            // Wait for Exited event, but not more than 30 seconds.
            const int SLEEP_AMOUNT = 100;
            while (!eventHandled)
            {
                elapsedTime += SLEEP_AMOUNT;
                if (elapsedTime > 30000)
                {
                    break;
                }
                Thread.Sleep(SLEEP_AMOUNT);
            }
            return returnValue;
        }

    }

}
