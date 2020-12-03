//Author: Chandler Ford
//Class: CSS 490 A
//Last Modified: 10/11/18
//Description: Simple Web Crawler

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace myCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var client = new HttpClient())
            {
                //Take two arguments amd check if valid string and integer
                string startingURL = "";
                char[] validStringArg = args[0].ToCharArray();  //Convert string to char to see if contains 'http'
                if (!(validStringArg[0] == 'h' && validStringArg[1] == 't' && validStringArg[2] == 't' && validStringArg[3] == 'p' && validStringArg[4] == ':' && validStringArg[5] == '/' && validStringArg[6] == '/'))
                {
                    Console.WriteLine("Invalid argument. Must be valid HTTP address.");  //Error messsage if address doesn't include HTTP
                }
                else
                {
                    startingURL = args[0];  //Initialize variable with string argument
                    int numHops = 0;
                    bool validIntArg = (int.TryParse(args[1], out numHops));  //Check if integer can be parsed out of argument

                    if (!validIntArg)
                    {
                        Console.WriteLine("Invalid argument. Must be integer value.");  //Error message if not integer
                    }
                    else
                    {
                        //Download starting HTML
                        HttpResponseMessage response = client.GetAsync(startingURL).Result;
                        Console.WriteLine("-----------------------------------------------");
                        Console.WriteLine("Status Code:");
                        Console.WriteLine(response.StatusCode);
                        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                        {
                            Console.WriteLine("Program has encountered return code.");
                        }
                        response.EnsureSuccessStatusCode();
                        string result = response.Content.ReadAsStringAsync().Result;  //HTML converted to string

                        //Parse HTML with regular expression to find <a href> reference tag
                        string patternTag = "<a (.*\\s)?href=\"?'?(.*?)\"?'?(\\s.*)?>";
                        Regex findTag = new Regex(patternTag);

                        //Parse HTML with regular expression to get contents of HREF
                        string patternHREF = "((http | https)://)?[a-zA-Z]\\w*(\\.\\w+)+(/\\w*(\\.\\w+)*)*(\\?.+)*";
                        Regex findHREF = new Regex(patternHREF);

                        //Creates list to track URL pages visited
                        List<string> visitedURL = new List<string>();
                        visitedURL.Add(startingURL);
                        Console.WriteLine("-----------------------------------------------");
                        Console.WriteLine("STARTING URL:");
                        Console.WriteLine(startingURL);
                        Console.WriteLine();

                        //Download page and repeat numHop times
                        for (int currentHop = 0; currentHop < numHops; currentHop++)
                        {
                            Console.WriteLine("Current Hop:");
                            Console.WriteLine(currentHop);
                            Console.WriteLine("of numHops:");
                            Console.WriteLine(numHops);
                            Console.WriteLine();

                            //List of URLs on page
                            List<string> listURL = new List<string>();

                            //Use regular expressions to find all URLs in page
                            foreach (Match matchTag in findTag.Matches(result))
                            {
                                if (matchTag.Success)
                                {
                                    foreach (Match matchHREF in findHREF.Matches(matchTag.Value))
                                    {
                                        if (matchHREF.Success)
                                        {
                                            if (visitedURL.Contains(matchHREF.Value))  //If URL already exist don't add to list of possible URLs
                                            {
                                                //Console.WriteLine("URL already visited!");
                                                //Console.WriteLine(matchHREF.Value);
                                                //Console.WriteLine();
                                            }
                                            else
                                            {
                                                listURL.Add(matchHREF.Value);  //If URL unique add to list of possible URLs
                                                //Console.WriteLine("Added URL to current list:");
                                                //Console.WriteLine(matchHREF.Value);
                                                //Console.WriteLine();
                                            }
                                        }
                                    }
                                }
                            }

                            //If all pages already visited
                            if (listURL.Count == 0)
                            {
                                Console.WriteLine("-----------------------------------------------");
                                Console.WriteLine("FINAL HTML:");
                                Console.WriteLine(result);
                                Console.WriteLine();

                                currentHop = numHops;
                            }
                            //Find and update chosenURL and add to list of visited URLs
                            else
                            {
                                visitedURL.Add(listURL[0]);
                                string chosenURL = "http://";
                                chosenURL += listURL[0];

                                //Download page
                                try
                                {
                                    response = client.GetAsync(chosenURL).Result;
                                    Console.WriteLine("-----------------------------------------------");
                                    Console.WriteLine("Status Code:");
                                    Console.WriteLine(response.StatusCode);
                                    response.EnsureSuccessStatusCode();
                                    result = response.Content.ReadAsStringAsync().Result;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("-----------------------------------------------");
                                    Console.WriteLine("Program has encountered return code.");
                                }

                                //Print value of final URL as well as HTML
                                if (currentHop == numHops - 1)
                                {
                                    Console.WriteLine("-----------------------------------------------");
                                    Console.WriteLine("FINAL URL:");
                                    Console.WriteLine(chosenURL);
                                    Console.WriteLine("FINAL HTML:");
                                    Console.WriteLine(result);
                                    Console.WriteLine();

                                    currentHop = numHops;
                                }
                                //Print chosenURL
                                else
                                {
                                    Console.WriteLine("-----------------------------------------------");
                                    Console.WriteLine("CHOSEN URL:");
                                    Console.WriteLine(chosenURL);
                                    Console.WriteLine();
                                }
                            }
                        }
                    }
                }   
            }
        }
    }
}