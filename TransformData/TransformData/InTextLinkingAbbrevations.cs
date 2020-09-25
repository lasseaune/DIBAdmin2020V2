using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;


namespace DIB.InTextLinking
{
    public class InTextLinkingAbbrevations
    {
        public string Tag1()
        {
            //New tag1
            XElement e = XElement.Load(@"D:\DIBProduction\idParts.xml");

            string s = e.Value;
            //string s = File.ReadAllText(@"D:\DIBProduction\test.txt");

            return "(" + s + ")";
        }
        public string GetAbbrevations(List<string> abbrevations)
        {
            string returnValue = string.Empty;
            foreach (string abbrevation in abbrevations)
            {
                
                string expression = ReplaceCharWithRegexp(abbrevation);
                if (expression!= string.Empty)
                    returnValue += (returnValue == string.Empty ? expression : "|" + expression);
                
            }
            if (returnValue != string.Empty)
            {
                //returnValue = returnValue + @"|([A-ZÆØÅ])+|([a-zæøåA-ZÆØÅ\-])+(l|L|F|f)(\.|ov(a|en(e)?)?|orskrift(en))?";
                returnValue = "(" + returnValue + ")";
            }
            return returnValue;
        }
        
        private string ReplaceCharWithRegexp(string abbrevation)
        {
            string returnValue = string.Empty;
            try
            {
                //if (!(Regex.IsMatch(abbrevation.Trim(), @"^(([A-ZÆØÅ])+|([a-zæøåA-ZÆØÅ\-])+(l|L|F|f)(\.|ov(a|en(e)?)?|orskrift(en))?)$")))
                //{
                  
                if (Regex.IsMatch(abbrevation, "^[a-zæøå][a-zæøå]"))
                    {
                        abbrevation = "(" + abbrevation.Substring(0, 1).ToUpper() + "|" + abbrevation.Substring(0, 1).ToLower() + ")" + abbrevation.Substring(1);
                    }
                   
                    abbrevation = Regex.Replace(abbrevation, @"\s+", @"\s+");
                    abbrevation = Regex.Replace(abbrevation, @"\/", @"(\s+)?\/(\s+)?");
                    abbrevation = abbrevation.Replace(@".", @"(\.)?");
                    abbrevation = abbrevation.Replace(@"(2015)", @"\(2015\)");
                    returnValue = abbrevation;
                //}
                
            }
            catch (SystemException e)
            {
                returnValue = string.Empty;
            }
            return returnValue;
        }

    }
}
