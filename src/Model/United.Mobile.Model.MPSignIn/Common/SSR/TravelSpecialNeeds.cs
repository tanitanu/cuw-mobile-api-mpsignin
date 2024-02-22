using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace United.Mobile.Model.Common.SSR
{
    [Serializable]
    public class TravelSpecialNeeds
    {
        private List<MOBTravelSpecialNeed> specialMeals=new List<MOBTravelSpecialNeed>();

        public string MealUnavailable { get; set; }
        public string AccommodationsUnavailable { get; set; }

        public List<MOBTravelSpecialNeed> HighTouchItems { get; set; }

        public List<MOBTravelSpecialNeed> SpecialMeals
        {
            get { return specialMeals; }
            set
            {
                specialMeals = value ?? new List<MOBTravelSpecialNeed>();
            }
        }

        public List<MOBItem> SpecialMealsMessages { get; set; }

        public List<MOBTravelSpecialNeed> SpecialRequests { get; set; }

        public List<MOBItem> SpecialRequestsMessages { get; set; }

        public List<MOBTravelSpecialNeed> ServiceAnimals { get; set; }

        public List<MOBItem> ServiceAnimalsMessages { get; set; }
    }
}
