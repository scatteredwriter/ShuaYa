using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 刷呀
{
    class JsonToObject
    {

        public JsonToObject()
        {

        }

        public static News News_JsonConvert(string json, int i)//新闻Json反序列化
        {
            News news = new News();
            try
            {//有图片
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["showapi_res_body"];
                JObject jo3 = (JObject)jo2["pagebean"];
                JArray ja1 = (JArray)jo3["contentlist"];
                news.title = ja1[i]["title"].ToString();
                news.source = ja1[i]["source"].ToString();
                news.pubDate = ja1[i]["pubDate"].ToString();
                news.link = ja1[i]["link"].ToString();
                news.desc = ja1[i]["desc"].ToString();
                JArray ja2 = (JArray)ja1[i]["imageurls"];
                news.imageurls = ja2[0]["url"].ToString();
                return news;
            }
            catch (Exception)
            {//无图片
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["showapi_res_body"];
                JObject jo3 = (JObject)jo2["pagebean"];
                JArray ja1 = (JArray)jo3["contentlist"];
                news.title = ja1[i]["title"].ToString();
                news.source = ja1[i]["source"].ToString();
                news.pubDate = ja1[i]["pubDate"].ToString();
                news.link = ja1[i]["link"].ToString();
                news.desc = ja1[i]["desc"].ToString();
                news.imageurls = "";
                return news;
                throw;
            }

        }

        public static int All_News_Num(string json)//获取新闻的总条数
        {
            int all_num;
            try
            {
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["showapi_res_body"];
                JObject jo3 = (JObject)jo2["pagebean"];
                all_num = Convert.ToInt32(jo3["allNum"].ToString());
                return all_num;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static News_Channel News_Channel_JsonConvert(string json, int i)//新闻频道反序列化
        {
            try
            {
                News_Channel news_channel = new News_Channel();
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["showapi_res_body"];
                JArray ja1 = (JArray)jo2["channelList"];
                news_channel.name = ja1[i]["name"].ToString();
                return news_channel;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static Travellists Travellists_JsonConvert(string json, int i)//游记Json反序列化
        {
            Travellists travellists = new Travellists();
            try
            {
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["data"];
                JArray ja1 = (JArray)jo2["books"];
                travellists.text = ja1[i]["text"].ToString();
                travellists.likeCount = ja1[i]["likeCount"].ToString();
                travellists.viewCount = ja1[i]["viewCount"].ToString();
                travellists.commentCount = ja1[i]["commentCount"].ToString();
                travellists.routeDays = ja1[i]["routeDays"].ToString();
                travellists.startTime = ja1[i]["startTime"].ToString();
                travellists.userHeadImg = ja1[i]["userHeadImg"].ToString();
                travellists.userName = ja1[i]["userName"].ToString();
                travellists.headImage = ja1[i]["headImage"].ToString();
                travellists.title = ja1[i]["title"].ToString();
                travellists.bookUrl = ja1[i]["bookUrl"].ToString();
                travellists.elite = Convert.ToBoolean(ja1[i]["elite"].ToString());
                return travellists;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static Cities_Lists Cities_Lists_JsonConvert(string json, int i)//获取城市列表
        {
            Cities_Lists cities_lists = new Cities_Lists();
            try
            {
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JArray ja1 = (JArray)jo1["tngou"];
                cities_lists.province = ja1[i]["province"].ToString();
                return cities_lists;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static Jokes Jokes_JsonConvert(string json, int i)//笑话Json反序列化
        {
            Jokes jokes = new Jokes();
            try
            {//文字笑话
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["showapi_res_body"];
                JArray ja1 = (JArray)jo2["contentlist"];
                jokes.title = ja1[i]["title"].ToString();
                jokes.text = ja1[i]["text"].ToString();
                jokes.ct = ja1[i]["ct"].ToString();
                return jokes;
            }
            catch (Exception)
            {//图片笑话
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["showapi_res_body"];
                JArray ja1 = (JArray)jo2["contentlist"];
                jokes.title = ja1[i]["title"].ToString();
                jokes.ct = ja1[i]["ct"].ToString();
                jokes.img = ja1[i]["img"].ToString();
                return jokes;
                throw;
            }
        }

        public static Weather Weather_JsonConvert(string json)//天气Json反序列化
        {
            Weather weather = new Weather();
            try
            {
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["retData"];
                weather.city = jo2["city"].ToString();
                weather.date = jo2["date"].ToString();
                weather.time = jo2["time"].ToString();
                weather.weather = jo2["weather"].ToString();
                weather.temp = jo2["temp"].ToString();
                weather.l_tmp = jo2["l_tmp"].ToString();
                weather.h_tmp = jo2["h_tmp"].ToString();
                return weather;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static int All_Page(string class_name, string json)//获取到某类型的所有返回页
        {
            int all_page = 0;
            switch (class_name)
            {
                case "News":
                    {
                        JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                        JObject jo2 = (JObject)jo1["showapi_res_body"];
                        JObject jo3 = (JObject)jo2["pagebean"];
                        all_page = Convert.ToInt32(jo3["allPages"].ToString());
                    }; break;
                case "Jokes":
                    {
                        JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                        JObject jo2 = (JObject)jo1["showapi_res_body"];
                        all_page = Convert.ToInt32(jo2["allPages"].ToString());
                    }; break;
            }
            return all_page;
        }

        public static int Travellists_Count(string json)//获取游记搜索结果的总游记数
        {
            int count;
            try
            {
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["data"];
                count = Convert.ToInt32(jo2["count"].ToString());
                return count;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static int All_News_Channel_Num(string json)//获取到新闻频道所有频道的数量
        {
            try
            {
                int all_num = 0;
                JObject jo1 = (JObject)JsonConvert.DeserializeObject(json);
                JObject jo2 = (JObject)jo1["showapi_res_body"];
                all_num = Convert.ToInt32(jo2["totalNum"].ToString());
                return all_num;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
