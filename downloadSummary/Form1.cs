using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace downloadSummary
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string pathToDownloadCounters = @"C:\Users\kwoon\Desktop\Programming\downloadCounters\downloadCounters\bin\Debug\";
        private void Button1_Click(object sender, EventArgs e)
        {
            string test = "gibberish<test yoyoyo>hahaha</test>gibberish";
            string html = @"<a href=""//markets.ft.com/data/equities/tearsheet/summary?s=DFK:FRA"" class=""mod-ui-link"" data-oda=""{"" category"":""link="""" click="""" through"",""name"":""equities="""" tearsheet"",""label"":""mod-equities-results"",""value"":null}""="""" data-oda-event=""click""><span class=""mod-ui-hide-xsmall"">01 Communique Laboratory Inc</span><span class=""mod-ui-hide-small-above"">DFK:FRA</span></a>";

            //var parsed = ExtractAndParse(ref test, "<test", "</test>");

            string input = File.ReadAllText(pathToDownloadCounters + "1.txt");
            var result = System.Text.RegularExpressions.Regex.Unescape(input);
            var decodeHtml = HttpUtility.HtmlDecode(result);

            //var parsed = ExtractAndParse(ref decodeHtml, "<tbody>", "</tbody>");
            var parsed = ExtractAndParse(ref decodeHtml, "<tr>", "</tr>"); // ignore first <tr>

            while (true)
            {
                var tr = ExtractAndParse(ref decodeHtml, "<tr>", "</tr>");
                if (tr == "") break;
                var td = Parser(tr, "<td", "</td>");
                var spanText = Parser(td, @"<span class=""mod-ui-hide-xsmall"">", "</span>");

                var match = Regex.Match(td, @"\?s=(.*?)\""");
                var matchedText = match.Groups[1].Value;
            }
        }

        string ExtractAndParse (ref string input, string tag, string endTag)
        {
            string extracted = Extract(ref input, tag, endTag);
            if (extracted == "") return "";

            string extractedWithoutTags = Parser(extracted, tag, endTag);
            return extractedWithoutTags;
        }

        // "gibberish<test>hahaha</test>gibberish";
        // f("<test>", "</test>) - hahaha
        string Parser(string input, string tag, string endTag)
        {
            if (!tag.EndsWith(">"))
            {
                int pos1 = input.IndexOf(tag);
                int pos2 = input.IndexOf(">", pos1 + 1);
                tag = input.Substring(pos1, pos2 - pos1 + 1);
                if (tag == "") return "";
            }

            if (input.IndexOf(tag) == -1) return "";
            if (input.IndexOf(endTag) == -1) return "";

            int start = input.IndexOf(tag);
            start += tag.Length;

            int len = input.Length - (start + (input.Length - input.IndexOf(endTag, input.IndexOf(tag) + 1))); //

            string extract = input.Substring(start, len);
            return extract;
        }

        // extract text including tag
        string Extract (ref string input, string tag, string endTag)
        {
            if (!tag.EndsWith(">"))
            {
                int pos1 = input.IndexOf(tag);
                int pos2 = input.IndexOf(">", pos1 + 1);
                tag = input.Substring(pos1, pos2 - pos1 + 1);
                if (tag == "") return "";
            }

            if (input.IndexOf(tag) == -1) return "";
            if (input.IndexOf(endTag) == -1) return "";

            int start = input.IndexOf(tag);

            int len = input.Length - (input.IndexOf(tag)) - (input.Length - input.IndexOf(endTag, input.IndexOf(tag) + 1) - endTag.Length); //

            string extract = input.Substring(start, len);

            input = input.Replace(extract, "");

            return extract;
        }
    }
}
