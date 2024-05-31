using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.qrcode;
using iTextSharp.tool.xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting.Internal;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Mail;


namespace Railway_Passenger_Transportation.Pages
{
    public class PaymentModel : PageModel
    {
        private readonly DatabaseConnection databaseConnection;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public Ticket ticket;
        public PaymentModel(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            databaseConnection = new DatabaseConnection(connectionString);
        }
        public void OnGet(string surname, string name, string patronymic, string documentNumber, int category, string phoneNumber, string email)
        {
            Passenger passenger = new Passenger()
            {
                Surname = surname,
                Name = name,
                Patronymic = patronymic,
                DocumentNumber = documentNumber,
                CategoryCode = category,
                PhoneNumber = phoneNumber,
                Email = email
            };
            HttpContext.Session.SetString("Passenger", JsonConvert.SerializeObject(passenger));
            ticket = JsonConvert.DeserializeObject<Ticket>(HttpContext.Session.GetString("SelectedTicket"));
            if (passenger.CategoryCode == 2)
            {
                decimal discount = ticket.Price * 0.3m;
                ticket.Price = ticket.Price - discount;
            }
            else if (passenger.CategoryCode == 3)
            {
                decimal discount = ticket.Price * 1m;
                ticket.Price = ticket.Price - discount;
            }
            HttpContext.Session.SetString("SelectedTicket", JsonConvert.SerializeObject(ticket));
        }
        public IActionResult OnPostPay()
        {
            Flight selectedFlight = JsonConvert.DeserializeObject<Flight>(HttpContext.Session.GetString("SelectedFlight"));
            Ticket selectedTicket = JsonConvert.DeserializeObject<Ticket>(HttpContext.Session.GetString("SelectedTicket"));
            Passenger passenger = JsonConvert.DeserializeObject<Passenger>(HttpContext.Session.GetString("Passenger"));

            using (NpgsqlConnection connection = databaseConnection.GetConnection())
            {
                int newPassengerId=0;
                using (NpgsqlCommand command = new NpgsqlCommand("SELECT MAX(код) FROM public.пассажиры;", connection)) 
                {
                    using(NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            try
                            {
                                newPassengerId = reader.GetInt32(0) + 1;
                            }
                            catch
                            {
                                newPassengerId = 1;
                            }
                        }
                    }
                }
                using(NpgsqlCommand command = new NpgsqlCommand("INSERT INTO public.пассажиры (код, фамилия, имя, отчество, номер_документа, код_категории_пассажира, телефон, электронная_почта) VALUES(@passengerId, @passengerSurname, @passengerName, @passengerPatronymic, @documentNumber, @categoryCode, @phoneNumber, @email);", connection))
                {
                    command.Parameters.AddWithValue("@passengerId", newPassengerId);
                    command.Parameters.AddWithValue("@passengerSurname", passenger.Surname);
                    command.Parameters.AddWithValue("@passengerName", passenger.Name);
                    command.Parameters.AddWithValue("@passengerPatronymic", passenger.Patronymic);
                    command.Parameters.AddWithValue("@documentNumber", passenger.DocumentNumber);
                    command.Parameters.AddWithValue("@categoryCode", passenger.CategoryCode);
                    command.Parameters.AddWithValue("@phoneNumber", passenger.PhoneNumber);
                    command.Parameters.AddWithValue("@email", passenger.Email);
                    command.ExecuteNonQuery();
                }
                using (NpgsqlCommand command = new NpgsqlCommand("CALL public.buy_ticket(@ticketCode, @passengerCode, @buyDate)", connection))
                {
                    command.Parameters.AddWithValue("@ticketCode", selectedTicket.Id);
                    command.Parameters.AddWithValue("@passengerCode", newPassengerId);
                    command.Parameters.Add("@buyDate", NpgsqlDbType.Date).Value = DateTime.Now;
                    command.ExecuteNonQuery();
                }
            }
            
            byte[] pdfBytes = CreateTicketPdf(selectedFlight, selectedTicket, passenger);
            string pdfName = "ticket.pdf";

            SendTicketEmail(passenger, pdfBytes, pdfName);

            return RedirectToPage("SuccessfulPurchase");
        }

        public byte[] CreateTicketPdf(Flight selectedFlight, Ticket ticket, Passenger passenger)
        {
            string logoRelativePath = "/Images/logotype.png";
            IFileInfo logoFileInfo = _webHostEnvironment.WebRootFileProvider.GetFileInfo(logoRelativePath);
            using (FileStream logoStream = (FileStream)logoFileInfo.CreateReadStream())
            {
                Bitmap bitmap = new Bitmap(800, 530);
                Graphics graphics = Graphics.FromImage(bitmap);

                System.Drawing.Image logo = System.Drawing.Image.FromStream(logoStream);

                System.Drawing.Font headerFont = new System.Drawing.Font("Arial", 24, FontStyle.Bold);
                System.Drawing.Font dataFont = new System.Drawing.Font("Arial", 18);

                Color backgroundColor = Color.LightGray;
                Color textColor = Color.Black;

                graphics.Clear(Color.White);

                graphics.DrawImage(logo, new RectangleF(5, 10, 377, 87));

                graphics.DrawString("Пассажир:", headerFont, Brushes.Black, new PointF(50, 150));
                graphics.DrawString($"Фамилия: {passenger.Surname}", dataFont, Brushes.Black, new PointF(50, 190));
                graphics.DrawString($"Имя: {passenger.Name}", dataFont, Brushes.Black, new PointF(50, 215));
                graphics.DrawString($"Отчество: {passenger.Patronymic}", dataFont, Brushes.Black, new PointF(50, 240));
                graphics.DrawString($"Паспорт РФ {passenger.DocumentNumber}", dataFont, Brushes.Black, new PointF(50, 265));

                graphics.DrawString("Рейс:", headerFont, Brushes.Black, new PointF(50, 310));
                graphics.DrawString($"Код рейса: {selectedFlight.Code}", dataFont, Brushes.Black, new PointF(50, 350));
                graphics.DrawString($"Отправление: {selectedFlight.DeparturePoint}", dataFont, Brushes.Black, new PointF(50, 375));
                graphics.DrawString($"Прибытие: {selectedFlight.DestinationPoint}", dataFont, Brushes.Black, new PointF(50, 400));
                graphics.DrawString($"Время отправления: {selectedFlight.DepartureTime}", dataFont, Brushes.Black, new PointF(50, 425));
                graphics.DrawString($"Время прибытия: {selectedFlight.ArrivalTime}", dataFont, Brushes.Black, new PointF(50, 450));

                graphics.DrawString("Место:", headerFont, Brushes.Black, new PointF(450, 150));
                graphics.DrawString($"Номер поезда: {selectedFlight.TrainNumber}", dataFont, Brushes.Black, new PointF(450, 190));
                graphics.DrawString($"Номер вагона: {ticket.WagonNumber}", dataFont, Brushes.Black, new PointF(450, 215));
                graphics.DrawString($"Номер места: {ticket.SeatType}", dataFont, Brushes.Black, new PointF(450, 240));
                graphics.DrawString($"Тип места: {ticket.SeatNumber}", dataFont, Brushes.Black, new PointF(450, 265));

                graphics.DrawString($"Стоимость: {ticket.Price}", headerFont, Brushes.Black, new PointF(450, 500));

                MemoryStream imageStream = new MemoryStream();
                bitmap.Save(imageStream, ImageFormat.Png);

                graphics.Dispose();
                bitmap.Dispose();

                Document document = new Document(PageSize.A4);
                MemoryStream pdfStream = new MemoryStream();
                PdfWriter writer = PdfWriter.GetInstance(document, pdfStream);

                document.Open();

                iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imageStream.ToArray());
                image.RotationDegrees = 90;
                document.Add(image);
                document.Close();
                return pdfStream.ToArray();
            }
        }

        public void SendTicketEmail(Passenger passenger, byte[] pdfBytes, string pdfName)
        {
            string smtpServer = "smtp.mail.ru";
            int smtpPort = 587; 
            string smtpUsername = "traintrekercomp@mail.ru"; 
            string smtpPassword = "aarFJ3qaXUmTwwawcUzH";
            
            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(smtpUsername);
                    mailMessage.To.Add(passenger.Email); 
                    mailMessage.Subject = "Билет на рейс";
                    mailMessage.Body = $"Добрый день, {passenger.Surname} {passenger.Name} {passenger.Patronymic}! <br/><br/>Прикреплен билет на рейс.";
                    mailMessage.IsBodyHtml = true;
                    mailMessage.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), pdfName));

                    try
                    {
                       
                        smtpClient.Send(mailMessage);
                        Console.WriteLine("Сообщение успешно отправлено.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка отправки сообщения: {ex.Message}");
                    }
                }
            }
        }
    }
}
