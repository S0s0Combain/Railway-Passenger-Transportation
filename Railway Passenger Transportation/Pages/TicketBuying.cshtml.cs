using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Railway_Passenger_Transportation.Pages
{
    public class TicketBuyingModel : PageModel
    {
        public Ticket ticket;

        public void OnGet(int ticketId, string wagonNumber, int seatNumber, string seatType, string price)
        {
            ticket = new Ticket
            {
                Id = ticketId,
                WagonNumber = wagonNumber,
                SeatNumber = seatNumber,
                SeatType = seatType,
                Price = decimal.Parse(price)
            };
            HttpContext.Session.SetString("SelectedTicket", JsonConvert.SerializeObject(ticket));
        }
    }
}
