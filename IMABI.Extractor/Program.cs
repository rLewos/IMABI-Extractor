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

namespace IMABI.Extractor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("IMABI Extractor");
            Console.WriteLine("Transform website version into html to study offline. You can \"print\" to PDF too. ");

            string imabiWebsite = "https://www.imabi.net/tableofcontents.htm";
            
            IConfiguration config = Configuration.Default;
            IBrowsingContext context = BrowsingContext.New(config);

            try
            {
                string currentDir = currentDir = string.Format("{0}\\{1}", Directory.GetCurrentDirectory(), "IMABIWebsiteLessons.html");
                Console.WriteLine("File will be saved on: " + currentDir);

                string htmlCode = IMABIUtils.GetPageContent(imabiWebsite);
                IDocument document = await context.OpenAsync(req => req.Content(htmlCode));
                IList<string> allUniqueLessonsLinkList = IMABIUtils.GetURLLessons(document);

                Console.WriteLine("Extracting lessons...");
                using (StreamWriter file = new StreamWriter(currentDir))
                {
                    foreach (string lessonLink in allUniqueLessonsLinkList)
                    {
                        Console.WriteLine(lessonLink);
                        try
                        {
                            file.WriteLine(IMABIUtils.GetLessonContent(lessonLink, context));
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

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();            
        }
    }
}
