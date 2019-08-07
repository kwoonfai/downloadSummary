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
using System.Net;
using System.Diagnostics;
using System.Media;

namespace downloadSummary
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string pathToDownloadCounters = @"C:\Users\kwoon\Desktop\Programming\downloadCounters\downloadCounters\bin\Debug\";
        private async void Button1_Click(object sender, EventArgs e)
        {
            //Testf();

            //string test = "gibberish<test yoyoyo>hahaha</test>gibberish";
            //string html = @"<a href=""//markets.ft.com/data/equities/tearsheet/summary?s=DFK:FRA"" class=""mod-ui-link"" data-oda=""{"" category"":""link="""" click="""" through"",""name"":""equities="""" tearsheet"",""label"":""mod-equities-results"",""value"":null}""="""" data-oda-event=""click""><span class=""mod-ui-hide-xsmall"">01 Communique Laboratory Inc</span><span class=""mod-ui-hide-small-above"">DFK:FRA</span></a>";

            //var parsed = ExtractAndParse(ref test, "<test", "</test>");

            for (int a=1; ; a++)
            {
                if (File.Exists(pathToDownloadCounters + a + ".txt"))
                {
                    await ProcessFile(pathToDownloadCounters + a + ".txt");
                }
                else
                {
                    break;
                }
            }
            Debug.WriteLine("Finished");
        }

        async Task ProcessFile(string filename)
        {
            string input = File.ReadAllText(filename);
            var result = System.Text.RegularExpressions.Regex.Unescape(input);
            var decodeHtml = HttpUtility.HtmlDecode(result);
            List<string> urlList = new List<string>();
            Dictionary<string, string> urlDictionary = new Dictionary<string, string>();
            string urlBase = @"https://markets.ft.com/data/equities/tearsheet/summary?s=";

            var parsed = ExtractAndParse(ref decodeHtml, "<tr>", "</tr>"); // ignore first <tr>

            while (true)
            {
                var tr = ExtractAndParse(ref decodeHtml, "<tr>", "</tr>");
                if (tr == "") break;
                var td = Parser(tr, "<td", "</td>");
                if (td == "") continue;
                var companyDescription = Parser(td, @"<span class=""mod-ui-hide-xsmall"">", "</span>");

                var match = Regex.Match(td, @"\?s=(.*?)\""");
                if (match.Success)
                {
                    var companyCode = match.Groups[1].Value;
                    urlList.Add(urlBase + companyCode);

                    if (!urlDictionary.ContainsKey(companyCode))
                        urlDictionary.Add(companyCode, urlBase + companyCode);
                }

            }

            foreach (var v in urlDictionary)
            {
                Debug.WriteLine("Downloading " + v.Value);
                await DownloadCompany(v.Key, v.Value);
            }
        }

        async Task DownloadCompany (string companyCode, string url)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            string toTxtName = companyCode + ".htm";

            foreach (char c in invalid)
            {
                toTxtName = toTxtName.Replace(c.ToString(), "_");
            }

            if (File.Exists(toTxtName)) return;

            WebClient wc = new WebClient();
            string downloadText;
            try
            {
                downloadText = await wc.DownloadStringTaskAsync(url);
            }
            catch
            {
                //SystemSounds.Asterisk.Play();
                Debug.WriteLine("Error downloading " + url);
                return;
            }

            File.WriteAllText(toTxtName, downloadText);
        }

        void Testf()
        {
            string apple = @"C:\Temp\Apple.html";
            string input = File.ReadAllText(apple);
            var result = System.Text.RegularExpressions.Regex.Unescape(input);
            var decodeHtml = HttpUtility.HtmlDecode(result);
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
                int pos1 = input.IndexOf(tag); if (pos1 == -1) return ""; // LKF: 26 Jul
                int pos2 = input.IndexOf(">", pos1 + 1); if (pos2 == -1) return ""; // LKF: 26 Jul
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
                int pos1 = input.IndexOf(tag); if (pos1 == -1) return ""; // LKF: 26 Jul
                int pos2 = input.IndexOf(">", pos1 + 1); if (pos2 == -1) return ""; // LKF: 26 Jul
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
