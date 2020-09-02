using DIBAdminAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;


namespace DIBAdminAPI.Source
{
    
    public static class Extensions
    {
        
        public static string Md5Hash(this string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public static string GetCallerName(this string s)
        {
            var method = new StackTrace().GetFrame(1).GetMethod();
            return string.Format("{1}:{0} -> ", method.Name, method.DeclaringType);
        }

        public static string GetProgramName(this string s)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
        }
    }
}
