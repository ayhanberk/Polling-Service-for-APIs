using PollingService.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PollingService
{
    internal class StartApp
    {
        private static Timer timer;
        private static readonly string connectionString = "Data Source=SERVER-NAME; Initial Catalog=DB-NAME; User ID=USER-NAME; Password=PASSWORD; MultipleActiveResultSets=True; Pooling=true";
        private static readonly List<APIList> apiList = new List<APIList>
        {
            new APIList { ApiURL = "URL", ApiTag = "URL-TAG", ApiTimer="TIMER TYPE INT MINUTE", LastRequestTime=DateTime.Now },
        };

        public static async Task Run()
        {

            await FirstRequest(); // if want to first request with time
            timer = new Timer(60000); // 1 min - every 1 min check conditions
            timer.Elapsed += async (sender, e) => await OnTimerRequest();
            timer.Start();

            Console.ReadLine();
        }


  
        private static async Task<string> GetApiResponse(string toURL)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(toURL);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                    return responseBody;


                }
                catch (HttpRequestException e)
                {
                    // Hata işleme
                    Console.WriteLine($"Request error: {e.Message}");
                    return null;
                }

            }
        }
        private static async Task FirstRequest()
        {
            Console.WriteLine("Initial process started...");

            foreach (var item in apiList)
            {
                string apiResponse = await GetApiResponse(item.ApiURL);
                if (apiResponse != null)
                {
                    SaveToDatabase(apiResponse, item);
                }
            }

            Console.WriteLine("Initial process completed.");
        }
        private static async Task OnTimerRequest()
        {
            foreach (var item in apiList)
            {
                if (DateTime.Now >= item.LastRequestTime.AddMinutes(item.ApiTimer))
                {
                    Console.WriteLine($"{item.ApiTag} API Request Sending...");
                    string apiResponse = await GetApiResponse(item.ApiURL);
                    if (apiResponse != null)
                    {
                        SaveToDatabase(apiResponse, item);
                    }
                }
            }

        }

        private static void SaveToDatabase(string data, APIList APIs)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string checkOldData = "SELECT * FROM SavedApiData WHERE ApiName = @ApiName";
                    using (SqlCommand checkCommand = new SqlCommand(checkOldData, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@ApiName", APIs.ApiTag);
                        using (SqlDataReader reader = checkCommand.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                // Kayıt var
                                Console.WriteLine("Find same Tag");
                                string query = "UPDATE SavedApiData SET LastUpdateAt=@UpdateDate, ApiData=@Data WHERE ApiName=@ApiName";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@UpdateDate", DateTime.Now);
                                    command.Parameters.AddWithValue("@Data", data);
                                    command.Parameters.AddWithValue("@ApiName", APIs.ApiTag);
                                    command.ExecuteNonQuery();
                                }
                                APIs.LastRequestTime = DateTime.Now;
                            }
                            else
                            {
                                // Kayıt yok, yeni kayıt ekle
                                string query = "INSERT INTO SavedApiData (LastUpdateAt, ApiData, ApiName) VALUES (@UpdateDate, @Data, @ApiName)";
                                using (SqlCommand command = new SqlCommand(query, connection))
                                {
                                    command.Parameters.AddWithValue("@UpdateDate", DateTime.Now);
                                    command.Parameters.AddWithValue("@Data", data);
                                    command.Parameters.AddWithValue("@ApiName", APIs.ApiTag);
                                    command.ExecuteNonQuery();
                                }
                                APIs.LastRequestTime = DateTime.Now;
                            }
                        }
                    }
                    connection.Close();
                    foreach (var i in apiList)
                    {
                        Console.WriteLine($"{i.ApiTag}-{i.LastRequestTime}");
                    }
                    Console.WriteLine(APIs.ApiTag);
                    Console.WriteLine(APIs.LastRequestTime);
                    Console.WriteLine("-----------------------------------------------------------------------------\n");


                }
                catch (SqlException e)
                {
                    // Hata işleme
                    Console.WriteLine($"Database error: {e.Message}");
                }
            }
        }
    }
}
