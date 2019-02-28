using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InformationManager.Models
{
    public class PageInfo
    {
        public IEnumerable<string> Images { get; set; }
        public IEnumerable<Words> Words { get; set; }
        public int TotalWords { get; set; }
        public string Url { get; set; }

    }

    public class Words
    {
        public string Word { get; set; }
        public int Count { get; set; }
    }
}