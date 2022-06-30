using ConsoleApp1;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace ConsoleApp1
{
    class Program
    {
        private static HttpClient client = new();

        static void Main(string[] args)
        {
            var response = GetToken().Result;
            Console.WriteLine("Here's your Token : " + response.AccessToken);

            var data = GetDataAsync(response.AccessToken).Result;

            foreach (var item in data.Results)
            {
                Console.WriteLine(item.Name);
                Console.WriteLine(item.MaxOccupancy);
                Console.WriteLine(item.SumIns);
                Console.WriteLine(item.DateTime);
                Console.WriteLine();
            }
        }


        private static async Task<ReposToken> GetToken()
        {
            var client = new HttpClient();

            var collect = new Dictionary<string, string>();
            collect.Add("grant_type", "client_credentials");
            collect.Add("client_id", "c8ccd4c4-a31e-4992-ad0f-6bad90893aaa");
            collect.Add("client_secret", "b45edbd8-6028-440c-85bb-e92671bb8e0c");


            var Json = JsonConvert.SerializeObject(collect);

            var streamTask = await client.PostAsync("https://auth.sensourceinc.com/oauth/token", new StringContent(Json, Encoding.UTF8, "application/json"));
            var repositories = await System.Text.Json.JsonSerializer.DeserializeAsync<ReposToken>(await streamTask.Content.ReadAsStreamAsync());

            return repositories;
        }

        private static async Task<OccupancyResponce> GetDataAsync(string AuthToken)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

            var streamTask = client.GetStreamAsync("https://vea.sensourceinc.com/api/data/occupancy?relativeDate=thishour&dateGroupings=hour&entityType=space&metrics=occupancy%28max%29");
            var data = await System.Text.Json.JsonSerializer.DeserializeAsync<OccupancyResponce>(await streamTask);

            return data;
        }


    }
}