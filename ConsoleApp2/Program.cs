using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Data.SqlClient;
using Newtonsoft;

namespace ConsoleApp2
{
    class Program
    {
        private static HttpClient client = new();

        public static void Main(string[] args)
        {
            var response = GetToken().Result;


            Console.WriteLine("Here's your Token : " + response.AccessToken);

            var data = GetDataAsync(response.AccessToken).Result;
            /*
            foreach (var item in data.Results)
            {
                int Id = item.Name.GetHashCode();
                Console.WriteLine(item.Name);
                Console.WriteLine(item.MaxOccupancy);
                Console.WriteLine(item.SumIns);
                Console.WriteLine(item.DateTime);
                Console.WriteLine();
                Insert(item.MaxOccupancy, item.SumIns, item.DateTime);
            }*/

            var Unify = GetunifyResponce(0).Result;
            if (Unify != null)
            {
                foreach (var item in Unify.Data)
                {
                    Console.WriteLine(item.SiteId);
                    Console.WriteLine(item.UserId);
                }
            }
        }


        // This will post the credentials to the Sensource site to get the authentication token needed to acess the data.
        private static async Task<ReposToken> GetToken()
        {
            var client = new HttpClient();

            var collect = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", "c8ccd4c4-a31e-4992-ad0f-6bad90893aaa" },
                { "client_secret", "b45edbd8-6028-440c-85bb-e92671bb8e0c" }
            };


            var Json = JsonConvert.SerializeObject(collect);

            var streamTask = await client.PostAsync("https://auth.sensourceinc.com/oauth/token", new StringContent(Json, Encoding.UTF8, "application/json"));
            var repositories = await System.Text.Json.JsonSerializer.DeserializeAsync<ReposToken>(await streamTask.Content.ReadAsStreamAsync());

            return repositories;
        }


        //This uses the authentication token provided to recieve the sensor data from Sensource and return it as Json.
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


        private static async Task<UnifyGetResponce> GetunifyResponce(int fails)
        {
            HttpClientHandler Handler = new HttpClientHandler();
            var cli = new HttpClient(Handler);

            var body = new
            {
                username = "API",
                password = "Sphe+ica#Dreamy"
            };

            var Json = JsonConvert.SerializeObject(body);

            var result = await cli.PostAsync("https://unifi.longwoodgardens.org:8443/api/login", new StringContent(Json, Encoding.UTF8, "application/json"));

            if ((int)result.StatusCode == 200)
            {
                HttpClient client = new(Handler);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("None");

                var stringTask = await client.GetAsync("https://unifi.longwoodgardens.org:8443/api/s/default/stat/device").ConfigureAwait(false);
                if (stringTask.IsSuccessStatusCode)
                {
                    var Results = JsonConvert.DeserializeObject<UnifyGetResponce>(await stringTask.Content.ReadAsStringAsync());
                    return Results;
                }
                else
                {
                    throw new Exception("Could not get data");
                }
            }
            else
            {
                throw new Exception("Could not authenticate against the Ubiquiti API.");
            }


        }


        /*private static async Task<HttpResponseMessage> PostUnifyResponceAsync()
        {
            var cli = new HttpClient();

            var body = new
            {
                username = "API",
                password = "Sphe+ica#Dreamy"
            };

            var Json = JsonConvert.SerializeObject(body);

            var result = await cli.PostAsync("https://unifi.longwoodgardens.org:8443/api/login", new StringContent(Json, Encoding.UTF8, "application/json"));

            return result;
        }*/


        //This takes in the stated parameters and stores the into the OccupancyData Database.
        public static void Insert(int MaxOccupancy, int SumIns, DateTime DateTime)
        {
            string ConnString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\aespinosa.DOMAIN01\source\repos\ConsoleApp1\ConsoleApp2\Counts.mdf;Integrated Security=True;";
            using (SqlConnection Conn = new(ConnString))
            {
                Conn.Open();
                SqlTransaction Transaction = Conn.BeginTransaction();
                SqlCommand Cmd = new()
                {
                    Connection = Conn,
                    Transaction = Transaction,
                    CommandText = "Insert into OccupancyData (max_occupancy,sum_ins, date_time) VALUES(@max_occupancy,@sum_ins,@date_time)"
                };
                Cmd.Parameters.AddWithValue("@max_occupancy", MaxOccupancy);
                Cmd.Parameters.AddWithValue("@sum_ins", SumIns);
                Cmd.Parameters.AddWithValue("@date_time", DateTime);
                Cmd.ExecuteNonQuery();
                Transaction.Commit();
                Conn.Close();
            }


        }
    }
}