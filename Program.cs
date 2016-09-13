using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LUISbegin {
    internal class Program {
        private static readonly string api = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?mode=proof";
        private static readonly string key = "get your key here";
        static string input;
        private static readonly Regex rgx = new Regex("\\s+");
        static void Main(string[] args) {
            Console.WriteLine("Welcome to Spell Checker v0.1 powered by the Microsoft Bing Spell Check API.\n");

            while (true) {
                Console.WriteLine("Enter your sentence below for a spell check:");
                input = Console.ReadLine();
                input = rgx.Replace(input, "+");
                input = "Text=" + input;

                Task t = new Task(QueryApi);
                t.Start();
                //showcasing async by printing something
                Console.WriteLine("Sending your query...\n");
                Console.ReadLine();
            }
        }
        static async void QueryApi() {
            //if you need a proxy
            //WebProxy myProx = new WebProxy{
            //};

            WebRequest request = WebRequest.Create(api);
            //request.Proxy = myProx;

            ((HttpWebRequest)request).UserAgent = ".NET Framework Client";
            request.Method = "POST";

            byte[] byteArray = Encoding.ASCII.GetBytes(input);
            request.ContentLength = byteArray.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Add("Ocp-Apim-Subscription-Key", key);

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            try {
                using (WebResponse response = await request.GetResponseAsync()) {
                    Stream data = response.GetResponseStream();
                    StreamReader reader = new StreamReader(data);
                    JObject results = JObject.Parse(reader.ReadToEnd());
                    JArray errors = (JArray)results.GetValue("flaggedTokens");
                    if (errors.Count == 0) {
                        Console.WriteLine("You have no spelling mistakes in the above input.");
                    }
                    else {
                        for (int i = 0; i < errors.Count; i++) {
                            JObject err = (JObject)errors[i];
                            Console.WriteLine("Error " + (i + 1) + ": " + err.GetValue("token").ToString());
                            Console.WriteLine("Suggestions: " + err.GetValue("suggestions").ToString());
                        }
                    }
                    response.Close();
                    data.Close();
                    reader.Close();
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }

        }
    }
}
