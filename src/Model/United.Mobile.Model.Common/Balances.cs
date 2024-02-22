
using System;
using System.Collections.Generic;
using United.Mobile.Model.CSLModels;

namespace United.Mobile.Model.Common
{
    public class Balances
    {
        //
        // Summary:
        //     Balance amount.       
        public decimal Amount { get; set; }
        //
        // Summary:
        //     Expiry date for balance     
        public DateTime? ExpirationDate { get; set; }
        //
        // Summary:
        //     Program currecy  
        public  United.Mobile.Model.CSLModels.Constants.ProgramCurrencyType Currency { get; set; }
        //
        // Summary:
        //     Subbalances     
        public List<SubBalances> SubBalances { get; set; }
    }
}
