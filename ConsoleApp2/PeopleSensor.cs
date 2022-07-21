using Newtonsoft.Json;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace ConsoleApp2
{
    public class PeopleSensor
    {
        // This will post the credentials to the Sensource site to get the authentication token needed to acess the data.
        public static async Task<PeopleAuth> GetToken()
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
            var repositories = await System.Text.Json.JsonSerializer.DeserializeAsync<PeopleAuth>(await streamTask.Content.ReadAsStreamAsync());

            return repositories;
        }

        //This uses the authentication token provided to recieve the sensor data from Sensource and return it as Json.
        public static async Task<PeopleResponce> GetPeopleData(string AuthToken, HttpClient client)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);

            var streamTask = client.GetStreamAsync("https://vea.sensourceinc.com/api/data/occupancy?relativeDate=thishour&dateGroupings=hour&entityType=space&metrics=occupancy%28max%29");
            var data = await System.Text.Json.JsonSerializer.DeserializeAsync<PeopleResponce>(await streamTask);


            return data;
        }

        //This takes in the stated parameters and stores them into the OccupancyData Database.
        public static void UpDateSensorData(string Location, int MaxOccupancy, int SumIns, DateTime DateTime)
        {
            string ConnString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\aespinosa.DOMAIN01\source\repos\ConsoleApp1\ConsoleApp2\Counts.mdf;Integrated Security=True;";
            string InsertStatement = @"Insert into OccupancyData (Location,max_occupancy,sum_ins, date_time) VALUES(@Location,@max_occupancy,@sum_ins,@date_time)";
            string SensorStatement = "Select * from OccupancyData";
            using (SqlConnection Conn = new(ConnString))
            {
                Conn.Open();
                using (SqlTransaction Transaction = Conn.BeginTransaction())
                using (SqlCommand cmd = new(SensorStatement, Conn, Transaction))
                {
                    SqlDataAdapter adapter = new(cmd);
                    DataTable dt = new();
                    adapter.Fill(dt);
                    if (dt.Rows.Count == 0)
                    {
                        using (SqlCommand Cmd = new(InsertStatement, Conn, Transaction))
                        {
                            Cmd.Parameters.AddWithValue("@Location", Location);
                            Cmd.Parameters.AddWithValue("@max_occupancy", MaxOccupancy);
                            Cmd.Parameters.AddWithValue("@sum_ins", SumIns);
                            Cmd.Parameters.AddWithValue("@date_time", DateTime);
                            Cmd.ExecuteNonQuery();
                            Transaction.Commit();
                        }
                    }
                    else
                    {
                        int counter = 0;
                        foreach (DataRow row in dt.Rows)
                        {
                            if (!Location.Equals(row.Field<string>("Location")))
                            {
                                counter++;
                            }
                            else
                            {
                                string UpDateStatement = @"UPDATE OccupancyData SET max_occupancy = @max_occupancy, sum_ins = @sum_ins, date_time = @date_time WHERE Location = @Location";
                                using (SqlCommand Cmd = new(UpDateStatement, Conn, Transaction))
                                {
                                    Cmd.Parameters.AddWithValue("@Location", Location);
                                    Cmd.Parameters.AddWithValue("@max_occupancy", MaxOccupancy);
                                    Cmd.Parameters.AddWithValue("@sum_ins", SumIns);
                                    Cmd.Parameters.AddWithValue("@date_time", DateTime);
                                    Cmd.ExecuteNonQuery();
                                    Transaction.Commit();
                                }
                            }
                        }
                        if (counter == dt.Rows.Count)
                        {
                            using (SqlCommand Cmd = new(InsertStatement, Conn, Transaction))
                            {
                                Cmd.Parameters.AddWithValue("@Location", Location);
                                Cmd.Parameters.AddWithValue("@max_occupancy", MaxOccupancy);
                                Cmd.Parameters.AddWithValue("@sum_ins", SumIns);
                                Cmd.Parameters.AddWithValue("@date_time", DateTime);
                                Cmd.ExecuteNonQuery();
                                Transaction.Commit();
                            }
                        }
                    }
                    
                }
                Conn.Close();
            }


        }
    }

    //This will take Json data and parse the Authentication and refresh token from it.
    public class PeopleAuth
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    //This will take Json data and parse the named data from it
    public class PeopleData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("maxoccupancy")]
        public int MaxOccupancy { get; set; }

        [JsonPropertyName("sumins")]
        public int SumIns { get; set; }

        [JsonPropertyName("sumouts")]
        public int SumOuts { get; set; }

        [JsonPropertyName("recordDate_hour_1")]
        public DateTime DateTime { get; set; }
    }

    public class PeopleResponce
    {
        

        [JsonPropertyName("results")]
        public List<PeopleData> Results { get; set; }
    }
}
