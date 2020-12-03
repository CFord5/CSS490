using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace myCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //Take two arguments
            string startingURL = args[1];
            int numHops = args[2];

            //Download starting HTML
            HttpResponseMessage response = client.GetAsync(startingURL).Result;
            response.EnsureSuccessStatusCode();
            string result = response.Content.ReadAsStringAsync().Result;

            //Parse HTML to find first <a href> reference
            string pattern = "<a (.*\\s)?href=\"?'?(.*?)\"?'?(\\s.*)?>";
            Regex findURL = new Regex(pattern);

            //Creates list to track URL pages
            list<string> listURL = new list<string>();
            list<string> visitedURL = new list<string>();
            visitedURL.Add(startingURL);
            string nextURL;
            bool alreadyVisited;

            //Download page and repeat numHop times
            for (int i = 0; i < numHops; i++)
            {
                //Use regex to find all URLs in page
                foreach (Match match in findURL.Matches(result))
                {
                    if (match.Success)
                    {
                        listURL.Add(match.Value);
                    }
                }

                //Save URL and check if visited
                for (int i = 0; i < listURL.Count; i++)
                {
                    alreadyVisited = false;
                    for (int j = 0; i < visitedURL.Count; j++)
                    { 
                        if (listURL[i] == visitedURL[j])
                        {
                            alreadyVisited = true;
                        }
                    }
                    if (alreadyVisited == false)
                    {
                        nextURL = listURL[i];
                        visitedURL.Add(nextURL);
                    }
                    else
                    {
                        cout << "All links visited." << endl;
                        goto Finish;
                    }
                }
               
                //Download page
                HttpResponseMessage response = client.GetAsync(nextURL).Result;
                response.EnsureSuccessStatusCode();
                result = response.Content.ReadAsStringAsync().Result;

                //Increment counter and repeat
                i++;
            }

        //Print value of final URL as well as HTML
        Finish:
            Console.WriteLine(result);
            Console.WriteLine(nextURL);
        }
    }
}