using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using static ConsoleApp2.PeopleSensor;
using System.Data.SqlClient;

namespace ConsoleApp2
{
    class Program
    {
        private static HttpClient client = new();
        static HttpClientHandler Handler = new HttpClientHandler();
       // List<string> CList = {}

        public static void Main(string[] args)
        {
            var response = GetToken().Result;

            var data = GetPeopleData(response.AccessToken, client).Result;
            
            foreach (var item in data.Results)
            {
                int Id = item.Name.GetHashCode();
                Console.WriteLine(item.Name);
                Console.WriteLine(item.MaxOccupancy);
                Console.WriteLine(item.SumIns);
                Console.WriteLine(item.DateTime);
                Console.WriteLine();
                //InsertSensorData(item.MaxOccupancy, item.SumIns, item.DateTime);
            }

            var ClientResponce = GetClientResponce(0).Result;
            int cl = 0;
            int bl = 0;
            
            var AccessPointResponce = GetAccessPointResponce().Result;
            if (AccessPointResponce != null)
            {
                foreach (var point in AccessPointResponce.Data)
                {
                    cl++;
                    int clients = 0;
                    Console.WriteLine("Mac = " + point.Mac);
                    Console.WriteLine(cl + " Name = " + point.Name);
                    Console.WriteLine();
                    foreach (var device in ClientResponce.Data)
                    { 
                        if (device.APMac == point.Mac && device.Essid == "FreeGardenWIFI")
                        {
                            bl++;
                            clients++;
                            Console.WriteLine(bl + " User Id = " + device.UserId);
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine();
                    
                }
            }
        }


        //This will retrieve the client data from Unifi
        private static async Task<ClientGetResponce> GetClientResponce(int fails)
        {
            if ((int)await UnifySignIn() == 200)
            {
                HttpClient client = new(Handler);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var stringTask = await client.GetAsync("https://unifi.longwoodgardens.org:8443/api/s/default/stat/sta").ConfigureAwait(false);
                if (stringTask.IsSuccessStatusCode)
                {
                    var str = await stringTask.Content.ReadAsStringAsync();
                    var Results = JsonConvert.DeserializeObject<ClientGetResponce>(await stringTask.Content.ReadAsStringAsync());

                    return Results;
                }
                else
                {
                    throw new Exception("Could not get data");
                }
            }
            else
            {
                throw new Exception("Could not authenticate against the Unifi API.");
            }


        }


        private static async Task<AccessPointGetResponce> GetAccessPointResponce()
        {
            if ((int)await UnifySignIn() == 200)
            {
                HttpClient client = new(Handler);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var stringTask = await client.GetAsync("https://unifi.longwoodgardens.org:8443/api/s/default/stat/device-basic").ConfigureAwait(false);
                if (stringTask.IsSuccessStatusCode)
                {
                    var str = await stringTask.Content.ReadAsStringAsync();
                    var Results = JsonConvert.DeserializeObject<AccessPointGetResponce>(await stringTask.Content.ReadAsStringAsync());

                    return Results;
                }
                else
                {
                    throw new Exception("Could not get data");
                }
            }
            else
            {
                throw new Exception("Could not authenticate against the Unifi API.");
            }
        }

        private static async Task<int> UnifySignIn()
        {
            var cli = new HttpClient(Handler);

            var body = new
            {
                username = "API",
                password = "Sphe+ica#Dreamy"
            };

            var Json = JsonConvert.SerializeObject(body);

            var result = await cli.PostAsync("https://unifi.longwoodgardens.org:8443/api/login", new StringContent(Json, Encoding.UTF8, "application/json"));
            return (int)result.StatusCode;
        }

        public static void InsertClientTOAPData (String AccessPoint, int Clients)
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
                    CommandText = "Insert into ClientToAPData (clients,access_point) VALUES(@clients,@access_point)"
                };
                Cmd.Parameters.AddWithValue("@clients", Clients);
                Cmd.Parameters.AddWithValue("@access_point", AccessPoint);
                Cmd.ExecuteNonQuery();
                Transaction.Commit();
                Conn.Close();
            }
        }
    }
}