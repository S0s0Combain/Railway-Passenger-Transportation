namespace Railway_Passenger_Transportation
{
    public class Flight
    {
        public int Code { get; set; }
        public string DeparturePoint { get; set; }
        public string DestinationPoint { get; set;}
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public string TimeInTransit { get; set; }
        public string TrainNumber { get; set; }
        public DateTime Date {  get; set; }
        public string TrainType { get; set; }
        public string Company { get; set; }
    }
}
