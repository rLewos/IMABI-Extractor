using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IMABI.Extractor
{
    class Program
    {
        static string ReturnPage(string imabiWebsite)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(imabiWebsite);
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

        static async Task Main(string[] args)
        {
            Console.WriteLine("IMABI Extractor");
            Console.WriteLine("Transform website version into html/pdf to study offline.");

            string imabiWebsite = "https://www.imabi.net/tableofcontents.htm";
            string htmlCode = default(string);
            
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);

            try
            {
                string currentDir = currentDir = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "IMABIWebsiteLessons.html");
                Console.WriteLine("File will be saved on: " + currentDir);

                using (StreamWriter file = new StreamWriter(currentDir))
                {
                    // Website request.                    
                    htmlCode = ReturnPage(imabiWebsite);
                    IDocument document = await context.OpenAsync(req => req.Content(htmlCode));

                    // Getting all lessons links from the 'https://www.imabi.net/tableofcontents.htm'
                    IEnumerable<string> allLessonsLinks = document.QuerySelectorAll("a.fw_link_page").Attr("href");

                    // Removing all repeated links.
                    IList<string> allLessonsUniqueLinks = new List<string>();
                    foreach (string s in allLessonsLinks)
                    {
                        if (!allLessonsUniqueLinks.Any(x => x.Contains(s)))
                        {
                            allLessonsUniqueLinks.Add(s);
                        }
                        else
                        {
                            Console.WriteLine(string.Format("Repeated: {0}", s));
                        }
                    }

                    // Write table of contents to file.
                    Console.WriteLine("Tables of Content.");
                    IElement eph = document.QuerySelectorAll("div").GetElementById("fw-bigcontain");
                    file.WriteLine(eph.OuterHtml);

                    // Write all lessons to file.
                    Console.WriteLine("All Lessons");
                    foreach (string lessonLink in allLessonsUniqueLinks)
                    {
                        try
                        {
                            // Content.
                            Console.WriteLine(lessonLink);
                            htmlCode = ReturnPage(lessonLink);

                            document = await context.OpenAsync(req => req.Content(htmlCode));
                            eph = document.QuerySelectorAll("div").GetElementById("fw-bigcontain");

                            file.WriteLine(eph.OuterHtml);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.StackTrace);
                        }
                    }

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
                Console.WriteLine(e.StackTrace);
            }

            Console.ReadKey();
            
        }
    }
}
