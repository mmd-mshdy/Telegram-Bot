using System.Numerics;
using System.Text;
using Newtonsoft.Json;
namespace Crypto
{
    internal class Cryptoo
    {
        HttpClient httpClient = new HttpClient();
        string stringAPI = "https://api.wallex.ir/v1/currencies/stats";


        public async Task InitAsync()
        {
            HttpResponseMessage response = await httpClient.GetAsync(stringAPI);
            if (response.IsSuccessStatusCode)
            {
                string apiresponse = await response.Content.ReadAsStringAsync();
                ApiResponseWrapper apiWrapper = JsonConvert.DeserializeObject<ApiResponseWrapper>(apiresponse);
                List<ResultItem> ResultItems = apiWrapper.Result;
            }
        }




        public class ApiResponseWrapper
        {
            public List<ResultItem> Result { get; set; }
        }


        public class ResultItem
        {
            public string key { get; set; }
            public string name_en { get; set; }
            public double price { get; set; }

            public double? percent_change_1h { get; set; }

            public string prediction()
            {
                double newPrice = 0;
                newPrice = price + (price * Convert.ToDouble(percent_change_1h) / 100);
                string output = $"{newPrice}";
                return output;
            }
        }
    }
}