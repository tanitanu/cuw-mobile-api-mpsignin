using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Shopping
{
    [Serializable()]
    public class ULTripInfo
    {
        public List<MOBULAirReservation> AirReservations { get; set; }

        public List<MOBULTraveler> Travelers { get; set; }

        public List<MOBULOrderLine> OrderLines { get; set; }

        public int OrderAmount { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
    }

    [Serializable()]
    public class MOBULAirReservation
    {
        public string Pnr { get; set; }

        public string ReservationType { get; set; }

        public string TripType { get; set; }

        public List<MOBULItinerary> Itineraries { get; set; }

        public int Price { get; set; }

        public string Origin { get; set; }

        public string Destination { get; set; }
    }

    [Serializable()]
    public class MOBULItinerary
    {
        public string Origin { get; set; }

        public string OriginDescription { get; set; }

        public string Destination { get; set; }

        public string DestinationDescription { get; set; }

        public string DepartureTime { get; set; }

        public string ArrivalTime { get; set; }

        public string FareClass { get; set; }

        public string CarrierCode { get; set; }
    }

    [Serializable()]
    public class MOBULTraveler
    {
        public int Index { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string DateOfBirth { get; set; }
    }

    [Serializable()]
    public class MOBULOrderLine
    {
        public string Name { get; set; }

        public int Price { get; set; }
    }
}
