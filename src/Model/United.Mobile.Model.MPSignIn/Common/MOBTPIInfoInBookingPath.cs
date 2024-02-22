using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBTPIInfoInBookingPath
    {
        private string quoteId = string.Empty; // TPI price $14.12
        private string title; // Travel Guard® Insurance
        private string header; // Purchase travel insurance to <b>cover the unexpected:</b>
        private string tnc;
        private string coverCostText = string.Empty; // Travel insurance coverage is based on total cost of trip
        private string coverCost = string.Empty; // Covers total trip cost of $134.40
        private string coverCostStatus = string.Empty;// (currently added to trip)
        private string img;
        private string buttonTextInProdPage;
        private string buttonTextInRTIPage;
        private string legalInformation;
        private string legalInformationText;
        private string popUpMessage = string.Empty;
        private string oldQuoteId = string.Empty;
        private string tncSecondaryFOPPage;
        private string paymentContent; // travel insurance 
        private string paymentContentHeader;
        private string paymentContentBody; // DB content 
        private string displayAmount;
        private string confirmationMsg;
        private string confirmationEmailForTPIPurcahse;
        private string tileTitle1;
        private string tileTitle2;
        private string tileImage;
        private string tileLinkText;

        
        public List<MOBItem> TPIAIGReturnedMessageContentList { get; set; } //Covid-19 Emergency WHO TPI content

        public string QuoteId
        {
            get => this.quoteId;
            set => this.quoteId = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public double Amount { get; set; } // 14.12
        public string Title
        {
            get => this.title;
            set => this.title = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Header
        {
            get => this.header;
            set => this.header = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public List<string> Content { get; set; } // - Trip cancellations, - Missed flight connections, - Lost baggage
        public string Tnc
        {
            get => this.tnc;
            set => this.tnc = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string CoverCostText
        {
            get => this.coverCostText;
            set => this.coverCostText = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string CoverCost
        {
            get => this.coverCost;
            set => this.coverCost = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string CoverCostStatus
        {
            get => this.coverCostStatus;
            set => this.coverCostStatus = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Img
        {
            get => this.img;
            set => this.img = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string ButtonTextInProdPage
        {
            get => this.buttonTextInProdPage;
            set => this.buttonTextInProdPage = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string ButtonTextInRTIPage
        {
            get => this.buttonTextInRTIPage;
            set => this.buttonTextInRTIPage = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public bool IsRegistered { get; set; } // current status, show is in your cart or not.
        public string LegalInformation
        {
            get => this.legalInformation;
            set => this.legalInformation = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string LegalInformationText
        {
            get => this.legalInformationText;
            set => this.legalInformationText = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string PopUpMessage
        {
            get => this.popUpMessage;
            set => this.popUpMessage = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public double OldAmount { get; set; }
        public string OldQuoteId
        {
            get => this.oldQuoteId;
            set => this.oldQuoteId = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string TncSecondaryFOPPage
        {
            get => this.tncSecondaryFOPPage;
            set => this.tncSecondaryFOPPage = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string PaymentContent
        {
            get => this.paymentContent;
            set => this.paymentContent = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string PaymentContentHeader
        {
            get => this.paymentContentHeader;
            set => this.paymentContentHeader = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string PaymentContentBody
        {
            get => this.paymentContentBody;
            set => this.paymentContentBody = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string DisplayAmount
        {
            get => this.displayAmount;
            set => this.displayAmount = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string ConfirmationMsg
        {
            get => this.confirmationMsg;
            set => this.confirmationMsg = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string ConfirmationEmailForTPIPurcahse
        {
            get => this.confirmationEmailForTPIPurcahse;
            set => this.confirmationEmailForTPIPurcahse = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public bool IsTPIIncludedInCart { get; set; } = false;


        public string TileTitle1
        {
            get => this.tileTitle1;
            set => this.tileTitle1 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string TileTitle2
        {
            get => this.tileTitle2;
            set => this.tileTitle2 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string TileImage
        {
            get => this.tileImage;
            set => this.tileImage = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string TileLinkText
        {
            get => this.tileLinkText;
            set => this.tileLinkText = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

    }
}
