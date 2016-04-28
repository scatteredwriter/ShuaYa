using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace 刷呀
{
    class Get_Resquest
    {

        public Get_Resquest()
        {

        }

        public async Task<string> Get_Resquset_Result(Uri uri)
        {
            string result = "";
            HttpClient httpclient = new HttpClient();
            return await Task.Run(async () =>
            {
                try
                {
                    httpclient.DefaultRequestHeaders.Add("apikey", "9a84555d8b243d4afa83cac9855b60e7");
                    HttpResponseMessage response = httpclient.GetAsync(uri).Result;
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        result = "error";
                    }
                    return result;
                }
                catch (Exception)
                {
                    throw;
                }
            });
        }
    }
}
