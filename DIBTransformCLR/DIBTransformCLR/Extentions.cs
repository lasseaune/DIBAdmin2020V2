using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace DIBTransformCLR
{
    public static class Extentions
    {
        public static void SetAttributeValueEx(this XElement e, string name, string value)
        {
            XAttribute att = e.Attributes(name).FirstOrDefault();
            if (att != null)
            {
                att.SetValue(value);
            }
            else
            {
                e.Add(new XAttribute(name, value));
            }
        }
    }
}
