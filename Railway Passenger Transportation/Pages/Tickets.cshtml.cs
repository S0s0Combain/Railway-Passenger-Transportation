using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Npgsql;

namespace Railway_Passenger_Transportation.Pages
{
    public class TicketsModel : PageModel
    {
        private readonly DatabaseConnection databaseConnection;
        public List<Ticket> Tickets { get; set; }
        public TicketsModel(IConfiguration configuration)
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            databaseConnection = new DatabaseConnection(connectionString);
            Tickets = new List<Ticket>();
        }
        public void OnGet(int flightId, string departurePoint, string destinationPoint, DateTime date, TimeSpan departureTime, TimeSpan arrivalTime, string timeInTransit, string trainType, string trainNumber, string company)
        {
            using (NpgsqlConnection connection = databaseConnection.GetConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM public.get_tickets(@flightId)", connection))
                {
                    command.Parameters.AddWithValue("@flightId", flightId);
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read()) 
                        {
                            Tickets.Add(new Ticket
                            {
                                Id = reader.GetInt32(0),
                                WagonNumber = reader.GetString(1),
                                SeatNumber = reader.GetInt32(2),
                                SeatType = reader.GetString(3),
                                Price = reader.GetDecimal(4)
                            });
                        }
                    }
                }
                databaseConnection.CloseConnection();
            }
            Flight selectedFlight = new Flight
            {
                Code = flightId,
                DeparturePoint = departurePoint,
                DestinationPoint = destinationPoint,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                TimeInTransit = timeInTransit,
                TrainNumber = trainNumber,
                Date = date,
                TrainType = trainType,
                Company = company
            };
            HttpContext.Session.SetString("SelectedFlight", JsonConvert.SerializeObject(selectedFlight));
        }
    }
}
