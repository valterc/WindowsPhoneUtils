using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WindowsPhoneUtils.Http
{
    public class HttpWebStringResponse
    {
        public HttpWebResponse WebResponse;
        public String StringResponse;

        public String CleanedStringResponse
        {
            get
            {
                return StripAndCleanHTML(StringResponse);
            }
        }

        private static readonly Dictionary<String, String> ascii = new Dictionary<String, String>();
        public static String StripAndCleanHTML(String s)
        {
            Regex regex = new Regex("<[^>]*>");
            String newString = regex.Replace(s, "");

            if (ascii.Count == 0)
            {
                ascii.Add("\n", " ");
                ascii.Add("&#8211;", "–");
                ascii.Add("&#8230;", "…");
                ascii.Add("&#8216;", "‘");
                ascii.Add("&#8217;", "’");
                ascii.Add("&#8220;", "“");
                ascii.Add("&#8221;", "”");
                ascii.Add("&#8243;", "\"");
                ascii.Add("&eacute;", "é");
                ascii.Add("&Eacute;", "É");
                ascii.Add("&aacute;", "á");
                ascii.Add("&Aacute;", "Á");
                ascii.Add("&agrave;", "à");
                ascii.Add("&Agrave;", "À");
                ascii.Add("&atilde;", "ã");
                ascii.Add("&Atilde;", "Ã");
                ascii.Add("&iacute;", "í");
                ascii.Add("&Iacute;", "Í");
                ascii.Add("&ecirc;", "ê");
                ascii.Add("&Ecirc;", "Ê");
                ascii.Add("&ccedil;", "ç");
                ascii.Add("&Ccedil;", "Ç");
                ascii.Add("&Uacute;", "Ú");
                ascii.Add("&uacute;", "ú");
                ascii.Add("&Oacute;", "Ó");
                ascii.Add("&oacute;", "ó");
                ascii.Add("&quot;", "\"");
                ascii.Add("&#39;", "'");
                ascii.Add("&nbsp;", " ");
                ascii.Add("&amp;", "&");
            }

            StringBuilder sb = new StringBuilder(newString);

            foreach (String key in ascii.Keys)
            {
                sb.Replace(key, ascii[key]);
            }

            return sb.ToString();
        }

    }
}
