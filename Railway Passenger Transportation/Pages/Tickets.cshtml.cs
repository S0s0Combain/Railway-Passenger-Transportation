using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public void OnGet(int flightId)
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
        }
    }
}
