using AngleSharp;
using AngleSharp.Dom;
using IMABI.Extractor.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IMABI.Extractor.Utils
{
    public static class IMABIUtils
    {
        public static string GetPageContent(string imabiURL)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(imabiURL);
            httpWebRequest.UserAgent = "PostmanRuntime/7.26.1";

            string page = default(string);

            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (Stream dataStream = httpWebResponse.GetResponseStream())
            {
                StreamReader streamReader = new StreamReader(dataStream);
                page = streamReader.ReadToEnd();
            }

            return page;
        }

        public static IList<string> GetURLLessons(IDocument document)
        {
            // Getting all lessons links from the 'https://www.imabi.net/tableofcontents.htm'
            IEnumerable<string> allLessonsLinks = document.QuerySelectorAll("a.fw_link_page").Attr("href");
            IList<string> allLessonsUniqueLinks = new List<string>();

            // Removing all repeated links.
            foreach (string s in allLessonsLinks)
            {
                if (!allLessonsUniqueLinks.Any(x => x.Contains(s)))
                    allLessonsUniqueLinks.Add(s);

                else
                    Console.WriteLine(string.Format("Repeated: {0}", s));
            }

            return allLessonsUniqueLinks;
        }

        public static string GetLessonContent(string lessonLink, IBrowsingContext context)
        {
            string htmlCode = IMABIUtils.GetPageContent(lessonLink);

            Task<IDocument> documentTask = context.OpenAsync(req => req.Content(htmlCode));
            documentTask.Wait();

            string lessonCode = default(string);
            documentTask.ContinueWith(d =>
            {
                IElement lessonContent = d.Result.QuerySelectorAll("section")
                                                .SingleOrDefault(m => m.ClassList.Contains("webs-content")
                                                                   && m.ClassList.Contains("webs-body"));

                lessonCode = lessonContent.OuterHtml;
            }).Wait();

            return lessonCode;
        }
    }
}
