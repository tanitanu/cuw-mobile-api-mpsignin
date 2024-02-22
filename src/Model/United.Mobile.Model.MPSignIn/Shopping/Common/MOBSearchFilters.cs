using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBSearchFilters
    {
        private string aircraftTypes = string.Empty;
        
        private string airportsStop = string.Empty;        
        private string airportsStopToAvoid = string.Empty;        
        private string bookingCodes = string.Empty;        
        private string equipmentCodes = string.Empty;
        private string fareFamily = "";    
        private string timeArrivalMax = string.Empty;
        private string timeArrivalMin = string.Empty;
        private string timeDepartMax = string.Empty;
        private string timeDepartMin = string.Empty;      
        private int pageNumber = 1;
        private string sortType1 = string.Empty;        

        public string AircraftTypes { get { return this.aircraftTypes; } set { this.aircraftTypes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public string AirportsDestination { get; set; }
        public List<MOBSearchFilterItem> AirportsDestinationList { get; set; }
        public string AirportsOrigin { get; set; }
        public List<MOBSearchFilterItem> AirportsOriginList { get; set; }

        public string AirportsStop { get { return this.airportsStop; } set { this.airportsStop = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }

        public List<MOBSearchFilterItem> AirportsStopList { get; set; }
        public string AirportsStopToAvoid { get { return this.airportsStopToAvoid; } set { this.airportsStopToAvoid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }

        public List<MOBSearchFilterItem> AirportsStopToAvoidList { get; set; }
        public string BookingCodes { get { return this.bookingCodes; } set { this.bookingCodes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public int CabinCountMax { get; set; } = -1;
        public int CabinCountMin { get; set; } = -1;
        public bool CarrierDefault { get; set; } = true;
        public bool CarrierExpress { get; set; } = true;
        public bool CarrierPartners { get; set; } = true;
        public string CarriersMarketing { get; set; }
        public List<MOBSearchFilterItem> CarriersMarketingList { get; set; }
        public string CarriersOperating { get; set; }
        public List<MOBSearchFilterItem> CarriersOperatingList { get; set; }
        public bool CarrierStar { get; set; } = true;
        public int DurationMax { get; set; } = -1;
        public int DurationMin { get; set; } = -1;
        public int DurationStopMax { get; set; } = -1;
        public int DurationStopMin { get; set; } = -1;
        public string EquipmentCodes { get { return this.equipmentCodes; } set { this.equipmentCodes = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public List<MOBSearchFilterItem> EquipmentList { get; set; }
        public string EquipmentTypes { get; set; }
        public List<MOBSHOPFareFamily> FareFamilies { get; set; }
        public string FareFamily { get { return this.fareFamily; } set { this.fareFamily = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public decimal PriceMax { get; set; } = new Decimal(-1.0);
        public decimal PriceMin { get; set; } = new Decimal(-1.0);
        public string PriceMaxDisplayValue { get; set; }
        public string PriceMinDisplayValue { get; set; }
        public int StopCountExcl { get; set; } = -1;
        public int StopCountMax { get; set; } = -1;
        public int StopCountMin { get; set; } = -1;
        public string TimeArrivalMax { get { return this.timeArrivalMax; } set { this.timeArrivalMax = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public string TimeArrivalMin { get { return this.timeArrivalMin; } set { this.timeArrivalMin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public string TimeDepartMax { get { return this.timeDepartMax; } set { this.timeDepartMax = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public string TimeDepartMin { get { return this.timeDepartMin; } set { this.timeDepartMin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public List<string> Warnings { get; set; }
        public List<MOBSearchFilterItem> WarningsFilter { get; set; }
        public int PageNumber { get; set; } = 1;
        public string SortType1 { get { return this.sortType1; } set { this.sortType1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }

        public List<MOBSearchFilterItem> SortTypes { get; set; }
        public List<MOBSearchFilterItem> NumberofStops { get; set; }
        public List<MOBSearchFilterItem> AmenityTypes { get; set; }

        public List<MOBSearchFilterItem> AircraftCabinTypes { get; set; }
        public List<MOBSearchFilterItem> CarrierTypes { get; set; }
        public bool ShowPriceFilters { get; set; } = false;
        public bool ShowDepartureFilters { get; set; } = false;
        public bool ShowArrivalFilters { get; set; } = false;
        public bool ShowDurationFilters { get; set; } = false;
        public bool ShowLayOverFilters { get; set; } = false;
        public bool ShowSortingandFilters { get; set; } = false;
        public bool FilterSortPaging { get; set; } = false;
        public string MaxArrivalDate { get; set; }
        public string MinArrivalDate { get; set; }
        public bool ShowRefundableFaresToggle { get; set; } = false;
        public MOBSearchFilterItem RefundableFaresToggle { get; set; }
        public List<MOBSearchFilterItem> AdditionalToggles { get; set; }

    }
}
