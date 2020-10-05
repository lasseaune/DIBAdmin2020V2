using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace RegexEditor
{
    class commxml
    {
        public string Execute(string strCommand)
        {

            string SQLxml = "";

            SQLxml = SQLxml + "<?xml version='1.0' encoding='utf-8'?>";
            SQLxml = SQLxml + "<soap:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>";
            SQLxml = SQLxml + "<soap:Body>";
            SQLxml = SQLxml + "<GetData2 xmlns='http://tempuri.org/'>";
            SQLxml = SQLxml + "<xml>";
            SQLxml = SQLxml + "<XMLString xsi:type=\"xsd:string\">";
            SQLxml = SQLxml + "<![CDATA[";
            SQLxml = SQLxml + "<root xmlns:sql='urn:schemas-microsoft-com:xml-sql' >";
            SQLxml = SQLxml + "<sql:query>";
            SQLxml = SQLxml + strCommand;
            SQLxml = SQLxml + "</sql:query>";
            SQLxml = SQLxml + "</root>";
            SQLxml = SQLxml + "]]>";
            SQLxml = SQLxml + "</XMLString>";
            SQLxml = SQLxml + "</xml>";
            SQLxml = SQLxml + "</GetData2>";
            SQLxml = SQLxml + "</soap:Body></soap:Envelope>";



            //string url = "http://adminservice.dibkunnskap.no/service.asmx";
            string url = "http://t-adminservice3admin.dib.no/service.asmx";


            HttpWebRequest request = (HttpWebRequest)
                                    HttpWebRequest.Create(url); 

            String xmlString = SQLxml;
            UTF8Encoding encoding = new UTF8Encoding();

            byte[] bytesToWrite = encoding.GetBytes(xmlString); 

            request.Method = "POST";
            request.ContentLength = bytesToWrite.Length;
            request.Headers.Add("SOAPAction: \"http://tempuri.org/GetData2\""); //You need to change this
            //request.Headers.Add("Accept: \"text/xml; charset=UTF-8\""); 
            request.ContentType = "text/xml; charset=utf-8"; 

            Stream newStream = request.GetRequestStream();
            newStream.Write(bytesToWrite, 0, bytesToWrite.Length);
            newStream.Close(); 

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream); 

            string responseFromServer = reader.ReadToEnd();
            return responseFromServer;
        }
    }
}
