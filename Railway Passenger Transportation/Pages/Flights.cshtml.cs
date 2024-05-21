using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Railway_Passenger_Transportation.Pages
{
    public class FlightsModel : PageModel
    {
        private readonly DatabaseConnection databaseConnection;
        public List<Flight> Flights { get; set; }

        public FlightsModel(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            databaseConnection = new DatabaseConnection(connectionString);
            Flights = new List<Flight>();
        }

        public void OnGet(string departure, string destination, DateTime date)
        {
            using (NpgsqlConnection connection = databaseConnection.GetConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.select_flights(@departure, @destination, @date);", connection))
                {
                    command.Parameters.AddWithValue("@departure", departure);
                    command.Parameters.AddWithValue("@destination", destination);
                    command.Parameters.Add("@date", NpgsqlDbType.Date).Value = date.Date;
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Flights.Add(new Flight
                            {
                                Code = reader.GetInt32(0),
                                DeparturePoint = reader.GetString(1),
                                DestinationPoint = reader.GetString(2),
                                DepartureTime = reader.GetTimeSpan(3),
                                ArrivalTime = reader.GetTimeSpan(4),
                                TimeInTransit = reader.GetString(5),
                                TrainNumber = reader.GetString(6),
                                Date = reader.GetDateTime(7),
                                TrainType = reader.GetString(8),
                                Company = reader.GetString(9)
                            });
                        }
                    }
                }
                databaseConnection.CloseConnection();
            }
        }
    }
}
