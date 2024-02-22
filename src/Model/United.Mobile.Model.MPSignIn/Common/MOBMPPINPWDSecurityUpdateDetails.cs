using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBMPPINPWDSecurityUpdateDetails
    {
        public MOBMPPINPWDSecurityUpdateDetails()
            : base()
        {
        }
        public bool ForceSignOut { get; set; } // This is to force sign out when update later is disabled as its time to update the Security Data and will be forced to update data to move forward (As of now here too other than Revenue Booking)
        
        //The "daysToCompleteSecurityUpdate" property communicates how many days the user has to complete the security updates
        //The "passwordOnlyAllowed" property communicates that only their password can be used to sign-in.  If this property is true then check the value entered by the user in the PIN/Password input field.  If the user entered a 4 digit value then log them out and display an error message.
        //The "updateLaterAllowed" property communicates whether the user can perform the security updates later or not.


        public MOBMPPINPWDSecurityItems SecurityItems { get; set; }

        public MOBMPSecurityUpdatePath MPSecurityPath { get; set; }

        public int DaysToCompleteSecurityUpdate { get; set; }

        public bool PasswordOnlyAllowed { get; set; } // passwordOnlyAllowed says 

        public bool UpdateLaterAllowed { get; set; }

        public List<MOBMPSecurityUpdatePath> MPSecurityPathList { get; set; }

        public List<MOBItem> SecurityUpdateMessages { get; set; }

        //TFS Backlog Defect #27502 - PINPWD AutoSignIn
        public bool IsPinPwdAutoSignIn { get; set; }


    }
}
