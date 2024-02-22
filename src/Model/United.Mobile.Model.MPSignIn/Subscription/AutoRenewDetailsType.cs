using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn.Subscription
{
    public class AutoRenewDetailsType
    {

        private string autoRenewField;

        private PaymentDetailType autoRenewFopField;

        public string AutoRenew
        {
            get
            {
                return this.autoRenewField;
            }
            set
            {
                this.autoRenewField = value;
            }
        }

        public PaymentDetailType AutoRenewFop
        {
            get
            {
                return this.autoRenewFopField;
            }
            set
            {
                this.autoRenewFopField = value;
            }
        }
    }
}
