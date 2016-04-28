using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 刷呀
{
    public class Travellists//旅游游记类
    {
        public string text { get; set; }
        public string likeCount { get; set; }
        public string viewCount { get; set; }
        public string commentCount { get; set; }
        public string routeDays { get; set; }
        public string startTime { get; set; }
        public string userHeadImg { get; set; }
        public string userName { get; set; }
        public string headImage { get; set; }
        public string title { get; set; }
        public string bookUrl { get; set; }
        public bool elite { get; set; }
        public Windows.UI.Xaml.Visibility IsElite { get; set; }
    }
}
