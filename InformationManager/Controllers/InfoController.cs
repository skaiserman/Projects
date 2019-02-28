using HtmlAgilityPack;
using InformationManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Http;

namespace InformationManager.Controllers
{
    public class InfoController : ApiController
    {
        [HttpGet]
        public PageInfo LoadUrl(string url)
        {
            
            PageInfo info = null;
            
            if (ValidateUrl(url))
            {
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        //html agility pack load
                        var document = new HtmlWeb().Load(url);

                        //Get information about images and words
                        List<string> imgFormated = ExtractImages(document,url);
                        IEnumerable<Words> wordList = ExtractWords(document, url);
                        
                        //Create a object to store all information and return to the consumer
                        info = new PageInfo()
                        {
                            Images = imgFormated,
                            Url = url,
                            Words = wordList,
                            TotalWords = wordList.Sum(w => w.Count),
                        };
                    }
                    catch
                    {
                        var resp = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                        {
                            Content = new StringContent(string.Format("InternalError = {0}", url)),
                            ReasonPhrase = "Internal Error"
                        };
                        throw new HttpResponseException(resp);
                    }

                }
            }
            else //Feedback to consumer
            {
                var resp = new HttpResponseMessage(HttpStatusCode.NotAcceptable)
                {
                    Content = new StringContent(string.Format("Invalid Url = {0}", url)),
                    ReasonPhrase = "Invalid url"
                };
                throw new HttpResponseException(resp);
            }
            return info;
        }

        /// <summary>
        /// Format img path adding or not the url to it
        /// </summary>
        /// <param name="dataUrl"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private string FormatImageUrl(string dataUrl, string url)
        {
            var matchedValues = Regex.Match(dataUrl, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;

            if (!String.IsNullOrEmpty(matchedValues) || ((dataUrl.Contains("http:") || dataUrl.Contains("https:"))))
            {
                return dataUrl;
            }
            else
            {
                return url + dataUrl;
            }
        }

        private bool ValidateUrl(string url)
        {
            Uri uriResult;

            //Check if url is valid.
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }
        private IEnumerable<Words> ExtractWords(HtmlDocument document, string url)
        {
            //Get text from body tag.
            var node = document.DocumentNode.SelectSingleNode("//body").InnerText;

            //Create a list of words and how many time it appears on the page
            var wordsList = node.Split(new[] { " ", "\n", "\t" }, StringSplitOptions.RemoveEmptyEntries)
                            .GroupBy(r => r)
                            .Select(grp => new Words
                            {
                                Word = grp.Key,
                                Count = grp.Count()
                            })
                            .OrderBy(w => w.Word);
            return wordsList;
        }

        private List<string> ExtractImages(HtmlDocument document,string url)
        {
            //// For every tag in the HTML containing the node img.
            var imgUrls = document.DocumentNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));
            //For every "src" check and format the value to be able to show the image correctly.
            List<string> imgFormated = new List<string>();
            foreach (String src in imgUrls)
            {
                imgFormated.Add(FormatImageUrl(src, url));
            }
            return imgFormated;
        }

    }
}
