using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Text;

namespace APDateRetriever
{
    public class Table
    {
        public static HttpClient client = new();
        static HttpClientHandler Handler = new HttpClientHandler();
        public static readonly List<string> ConservatoryAps = new List<string> { "Ballroom 2nd Floor", "Ballroom Mezzanine", "Children's Garden", "East Conservatory", "East Roof", "Fern Floor",
            "Fern Floor Balcony", "Garden Path", "Pantry", "Patio of Oranges", "Performing Arts", "Silver Garden", "XG - West Side" };
        public static readonly List<string> Exclutions = new List<string> { "STP", "Substation West", "Security", "Anvil", "South Park", "SouthPark Pole", "Nursery Headhouse", "Pole Barn",
            "B-70 Headhouse", "Civil", "Equip Maint", "Paint Shop", "Electric Shop", "Facilities Building", "Education T54", "Dorm 55", "Dorm 57", "Dorm 59", "Dorm 61",
            "Dorm 63", "Dorm 65", "Dorm 66 - 2nd Floor", "Education T72", "Education T76", "Education T77", "B-75 Office space", "Data Center", "Nuttery IT", "Nuttery Conf Room" };
        //This will retrieve the client data from Unifi
        public static async Task<ClientGetResponce> GetClientResponce(int fails)
        {
            if ((int)await UnifySignIn() == 200)
            {
                HttpClient client = new(Handler);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var stringTask = await client.GetAsync("").ConfigureAwait(false);
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

        public static async Task<AccessPointGetResponce> GetAccessPointResponce()
        {
            if ((int)await UnifySignIn() == 200)
            {
                HttpClient client = new(Handler);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var stringTask = await client.GetAsync("").ConfigureAwait(false);
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
                username = "",
                password = ""
            };

            var Json = JsonConvert.SerializeObject(body);

            var result = await cli.PostAsync("", new StringContent(Json, Encoding.UTF8, "application/json"));
            return (int)result.StatusCode;
        }

        public static void InsertClient(string Mac, string Client)
        {
            string ConnString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\aespinosa.DOMAIN01\source\repos\ConsoleApp1\ConsoleApp2\Counts.mdf;Integrated Security=True;";
            string ClientStatement = "Select * from Clients";
            string InsertStatement = "Insert into Clients (client,AP_Id) VALUES(@client,@AP_Id)";
            string APStatement = "Select * from APs";
            using (SqlConnection Conn = new(ConnString))
            {
                Conn.Open();
                using (SqlTransaction Transaction = Conn.BeginTransaction())
                using (SqlCommand Cmd = new(ClientStatement, Conn, Transaction))
                using (SqlCommand cmd = new(APStatement, Conn, Transaction))
                {
                    SqlDataAdapter DataAdapter = new SqlDataAdapter(Cmd);
                    SqlDataAdapter DataAdapter2 = new SqlDataAdapter(cmd);
                    DataTable dt = new();
                    DataTable dt2 = new();
                    DataAdapter.Fill(dt);
                    DataAdapter2.Fill(dt2);
                    int counter = 0;
                    Boolean ValidMac = false;
                    int AP = 0;
                    foreach (DataRow row in dt2.Rows)
                    {
                        if (Mac.Equals(row.Field<string>("mac")))
                        {
                            ValidMac = true;
                            AP = row.Field<int>("Id");
                        }
                    }
                    if (ValidMac)
                    {
                        if (dt.Rows.Count == 0)
                        {
                            using (SqlCommand Comd = new(InsertStatement, Conn))
                            {
                                Comd.Parameters.AddWithValue("@client", Client);
                                Comd.Parameters.AddWithValue("@AP_Id", AP);
                                Comd.ExecuteNonQuery();
                                Transaction.Commit();
                            }
                        }
                        else
                        {
                            foreach (DataRow row in dt.Rows)
                            {
                                if (!Client.Equals(row.Field<string>("client")))
                                {
                                    counter++;
                                }
                                else
                                {
                                    if (AP != row.Field<int>("AP_Id"))
                                    {
                                        string UpDateStatement = @"UPDATE Clients SET AP_Id = @AP_Id WHERE client = @client";
                                        using (SqlCommand Comd = new(UpDateStatement, Conn, Transaction))
                                        {
                                            Comd.Parameters.AddWithValue("@AP_Id", AP);
                                            Comd.Parameters.AddWithValue("@client", Client);
                                            Comd.ExecuteNonQuery();
                                            Transaction.Commit();
                                        }
                                    }
                                }
                            }
                            if (counter == dt.Rows.Count)
                            {
                                using (SqlCommand Comd = new(InsertStatement, Conn, Transaction))
                                {
                                    Comd.Parameters.AddWithValue("@client", Client);
                                    Comd.Parameters.AddWithValue("@AP_Id", AP);
                                    Comd.ExecuteNonQuery();
                                    Transaction.Commit();
                                }
                            }
                        }
                    }

                }
                Conn.Close();
            }

        }

        public static void InsertAP(string AP, string Mac)
        {
            string ConnString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\aespinosa.DOMAIN01\source\repos\ConsoleApp1\ConsoleApp2\Counts.mdf;Integrated Security=True;";
            string APStatement = "Select * from APs";
            string InsertStatement = "Insert into APs (access_point,mac) VALUES(@access_point,@mac)";
            using (SqlConnection Conn = new(ConnString))
            {
                Conn.Open();
                using (SqlTransaction Trabsaction = Conn.BeginTransaction())
                using (SqlCommand Cmd = new(APStatement, Conn, Trabsaction))
                {
                    SqlDataAdapter DataAdapter = new SqlDataAdapter(Cmd);
                    DataTable dt = new();
                    DataAdapter.Fill(dt);
                    int counter = 0;
                    if (dt.Rows.Count == 0)
                    {
                        using (SqlCommand cmd = new(InsertStatement, Conn))
                        {
                            Cmd.Parameters.AddWithValue("@mac", Mac);
                            Cmd.Parameters.AddWithValue("@access_point", AP);
                            Cmd.ExecuteNonQuery();
                            Trabsaction.Commit();
                        }
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (!AP.Equals(row.Field<string>("access_point")))
                            {
                                counter++;
                            }
                        }
                        if (counter == dt.Rows.Count)
                        {
                            using (SqlCommand cmd = new(InsertStatement, Conn))
                            {
                                Cmd.Parameters.AddWithValue("@mac", Mac);
                                Cmd.Parameters.AddWithValue("@access_point", AP);
                                Cmd.ExecuteNonQuery();
                                Trabsaction.Commit();
                            }
                        }
                    }
                }
                Conn.Close();
            }
        }

        public static void UpDateClients(ClientGetResponce ClientResponse)
        {
            string ConnString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\aespinosa.DOMAIN01\source\repos\ConsoleApp1\ConsoleApp2\Counts.mdf;Integrated Security=True;";
            string ClientStatement = "Select * from Clients";
            using (SqlConnection Conn = new(ConnString))
            {
                Conn.Open();
                using (SqlTransaction Transaction = Conn.BeginTransaction())
                using (SqlCommand Cmd = new(ClientStatement, Conn, Transaction))
                {
                    SqlDataAdapter DataAdapter = new SqlDataAdapter(Cmd);
                    DataTable dt = new();
                    DataAdapter.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        int counter = 0;
                        string client = row.Field<string>("client");
                        foreach (var Client in ClientResponse.Data)
                        {
                            if (Client.Essid == "FreeGardenWIFI")
                            {
                                if (Client.UserId.Equals(client))
                                {
                                    counter++;
                                }
                            }
                        }
                        if (counter == 0)
                        {
                            string DeleteStatement = "Delete from Clients Where client = @client";
                            using (SqlConnection Con = new(ConnString))
                            {
                                Con.Open();
                                using (SqlTransaction Tranaction = Con.BeginTransaction())
                                using (SqlCommand Delete = new SqlCommand(DeleteStatement, Con, Tranaction))
                                {
                                    Delete.Parameters.AddWithValue("@client", client);
                                    Delete.ExecuteNonQuery();
                                    Tranaction.Commit();
                                }
                                Con.Close();
                            }
                        }
                    }
                }
                Conn.Close();
            }
        }

        public static void UpDateSensorToAP()
        {
            string ConnString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\aespinosa.DOMAIN01\source\repos\ConsoleApp1\ConsoleApp2\Counts.mdf;Integrated Security=True;";
            string OccupancyStatement = "Select * from OccupancyData";
            string APStatement = "Select * from APs";
            string SensorStatement = "Select * from SensorsToAPs";
            using (SqlConnection Conn = new(ConnString))
            {
                Conn.Open();
                using (SqlCommand Cmd = new(OccupancyStatement, Conn))
                using (SqlCommand cmd = new(APStatement, Conn))
                using (SqlCommand comd = new(SensorStatement, Conn))
                {
                    SqlDataAdapter dataAdapter = new(Cmd);
                    SqlDataAdapter adapter = new(cmd);
                    SqlDataAdapter SAPAdapter = new(comd);
                    DataTable OD = new();
                    dataAdapter.Fill(OD);
                    DataTable APD = new();
                    adapter.Fill(APD);
                    DataTable SAPD = new();
                    SAPAdapter.Fill(SAPD);
                    if (SAPD.Rows.Count == 0)
                    {
                        foreach (DataRow ODRow in OD.Rows)
                        {
                            string InsertStatement = "Insert into SensorsToAps (Location,AP_Id) VALUES(@Location,@AP_Id)";
                            if ("Gift Shop space".Equals(ODRow.Field<string>("Location")))
                            {
                                using (SqlTransaction Transaction = Conn.BeginTransaction())
                                using (SqlCommand Insertion = new SqlCommand(InsertStatement, Conn, Transaction))
                                {
                                    Insertion.Parameters.AddWithValue("@Location", "Gift Shop space");
                                    Insertion.Parameters.AddWithValue("@AP_Id", 525);
                                    Insertion.ExecuteNonQuery();
                                    Transaction.Commit();
                                }
                            }
                            else if ("Conservatory".Equals(ODRow.Field<string>("Location")))
                            {
                                foreach (DataRow APRow in APD.Rows)
                                {
                                    if (ConservatoryAps.Contains(APRow.Field<string>("access_point")))
                                    {
                                        using (SqlTransaction Transaction = Conn.BeginTransaction())
                                        using (SqlCommand Insertion = new SqlCommand(InsertStatement, Conn, Transaction))
                                        {
                                            Insertion.Parameters.AddWithValue("@Location", ODRow.Field<string>("Location"));
                                            Insertion.Parameters.AddWithValue("@AP_Id", APRow.Field<int>("Id"));
                                            Insertion.ExecuteNonQuery();
                                            Transaction.Commit();
                                        }
                                    }
                                }
                            }
                            else if ("Entry Into Gardens".Equals(ODRow.Field<string>("Location")))
                            {
                                foreach (DataRow APRow in APD.Rows)
                                {
                                    if (!ConservatoryAps.Contains(APRow.Field<string>("access_point")) && !Exclutions.Contains(APRow.Field<string>("access_point")))
                                    {
                                        using (SqlTransaction Transaction = Conn.BeginTransaction())
                                        using (SqlCommand Insertion = new SqlCommand(InsertStatement, Conn, Transaction))
                                        {
                                            Insertion.Parameters.AddWithValue("@Location", ODRow.Field<string>("Location"));
                                            Insertion.Parameters.AddWithValue("@AP_Id", APRow.Field<int>("Id"));
                                            Insertion.ExecuteNonQuery();
                                            Transaction.Commit();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public class PercetageData
        {
            [JsonProperty("Location")]
            public string? Location { get; set; }
            [JsonProperty("People")]
            public int? People { get; set; }
            [JsonProperty("Clients")]
            public int? Clients { get; set; }
            [JsonProperty("Percentage")]
            public double? Percentage { get; set; }
            public PercetageData(string location, int people, int clients, double percentage)
            {
                Location = location;
                People = people;
                Clients = clients;
                Percentage = percentage;
            }
        }

        public class PercetageDataList
        {
            [JsonProperty("Body")]
            public PercetageData[] Body { get; set; }

            public PercetageDataList(PercetageData first, PercetageData second, PercetageData third)
            {
                Body = new PercetageData[] { first, second, third};
            }

        }
        public static PercetageData PercetageDataMaker(string Location)
        {
            string ConnString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\aespinosa.DOMAIN01\source\repos\ConsoleApp1\ConsoleApp2\Counts.mdf;Integrated Security=True;";
            string SensorStatement = "Select * from SensorsToAPs";
            string OccupancyStatement = "Select * from OccupancyData Where Location = @Location";
            int clients = 0;
            int people = 0;
            double percentage = 0;
            using (SqlConnection Conn = new(ConnString))
            {
                Conn.Open();
                using (SqlCommand comd = new(SensorStatement, Conn))
                {
                    SqlDataAdapter DataAdapter = new(comd);
                    DataTable dt = new();
                    DataAdapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (Location.Equals(dr.Field<string>("Location")))
                        {
                            string ClientStatement = "Select * from Clients Where AP_Id =" + dr.Field<int>("AP_Id");
                            using (SqlCommand Cmd = new SqlCommand(ClientStatement, Conn))
                            {
                                SqlDataAdapter DataAdapter2 = new(Cmd);
                                DataTable dt2 = new();
                                DataAdapter2.Fill(dt2);
                                clients += dt2.Rows.Count;

                            }
                        }
                    }
                }
                using (SqlCommand comd = new(OccupancyStatement, Conn))
                {
                    comd.Parameters.AddWithValue("@Location", Location);
                    SqlDataAdapter DataAdapter = new(comd);
                    DataTable dt = new();
                    DataAdapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        people += dr.Field<int>("sum_ins");
                    }
                }
               
                Conn.Close();
            }
            percentage = (double)clients / (double)people * 100;
            return new PercetageData(Location, people, clients, percentage);
        }
    }
}