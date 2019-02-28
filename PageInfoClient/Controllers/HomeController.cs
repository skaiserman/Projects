using Newtonsoft.Json;
using PageInfoClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PageInfoClient.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        string baseUrl = "http://localhost/";

        public ActionResult Index()
        {
            
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(string url)
        {
            
            PageInfo pageInfo = null;

            Uri uriResult;
            
            //Check if url is valid.
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            
            if (result)
            {
                pageInfo = new PageInfo();
                using (var client = new HttpClient())
                {
                    //Passing service base url  
                    client.BaseAddress = new Uri(baseUrl);

                    client.DefaultRequestHeaders.Clear();
                    //Define request data format  
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    //Sending request to find web api REST service resource LoadUrl using HttpClient  
                    HttpResponseMessage Res = await client.GetAsync("informationmanager/api/info/LoadUrl/?url=" + url);

                    //Checking the response is successful or not which is sent using HttpClient  
                    if (Res.IsSuccessStatusCode)
                    {
                        //Storing the response details recieved from web api   
                        var response = Res.Content.ReadAsStringAsync().Result;

                        //Deserializing the response recieved from web api and storing into PageInfo entity  
                        pageInfo = JsonConvert.DeserializeObject<PageInfo>(response);
                        List<Words> list = new List<Words>();
                        foreach (Words word in pageInfo.Words)
                        {
                            word.Word = word.Word.Replace("\n", "");

                        }
                    }

                    //Create structure for Chart.
                    CreateDataPoints(pageInfo.Words.ToList());
                }
            }
            else
            {
                ModelState.AddModelError("Url", "Please enter a valid url with http:// or https:// in the content");
            }
            return View(pageInfo);
        }
        /// <summary>
        /// Create structure with Top 10 words to show on chart.
        /// </summary>
        /// <param name="words"></param>
        private void CreateDataPoints(List<Words> words)
        {
            List<DataPoint> dataPoints = new List<DataPoint>();
            var top10 = words.OrderByDescending(w => w.Count).Take(10);
            foreach (Words word in top10)
            {
                dataPoints.Add(new DataPoint(word.Word, word.Count));
            }
            ViewBag.DataPoints = dataPoints;
        }
    }
}