using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.Subscription
{
    public class FeatureType
    {

        private string displayNameField;

        private string descriptionField;

        private FeatureTypeType typeField;

        private string valueField;

        private string hirerachyField;

        private string nameField;

        public string DisplayName
        {
            get
            {
                return this.displayNameField;
            }
            set
            {
                this.displayNameField = value;
            }
        }

        public string Description
        {
            get
            {
                return this.descriptionField;
            }
            set
            {
                this.descriptionField = value;
            }
        }

        public FeatureTypeType Type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }

        public string Hirerachy
        {
            get
            {
                return this.hirerachyField;
            }
            set
            {
                this.hirerachyField = value;
            }
        }

        public string Name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }
    public enum FeatureTypeType
    {

        /// <remarks/>
        PRIORITY_CHECKIN,

        /// <remarks/>
        PRIORITY_BOARDING,

        /// <remarks/>
        PRIORITY_SECURITY,

        /// <remarks/>
        REGION,

        /// <remarks/>
        BAGGAGE_FEE_WAIVER,

        /// <remarks/>
        BENEFICIARY,

        /// <remarks/>
        HANDLING,

        /// <remarks/>
        DURATION,

        /// <remarks/>
        CATEGORY,

        /// <remarks/>
        BONUS_MILES,

        /// <remarks/>
        NONE,
    }
}
