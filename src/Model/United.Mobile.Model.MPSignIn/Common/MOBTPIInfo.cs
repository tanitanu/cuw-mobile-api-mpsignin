using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBTPIInfo
    {
        private string displayAmount = string.Empty; // TPI price $14.12
        private string formattedDisplayAmount = string.Empty; // $15, removed 
        private string coverCost = string.Empty; // TPI cover price $566.40
        private string pageTitle = string.Empty; // Travel Guard® Insurance 
        private string title1 = string.Empty; // Cover your trip with a 
        private string title2 = string.Empty; // For
        private string title3 = string.Empty; // per person, removed 
        private string quoteTitle = string.Empty; // Travel Guard® Insurance by AIG Travel
        private string headline1 = string.Empty; // Travel Guard Insurance from AIG 
        private string headline2 = string.Empty; // Add Travel Guard Insurance 
        private string body1 = string.Empty; // Are you prepard?
        private string body2 = string.Empty; // From millions of travelers every year...
        private string body3 = string.Empty; // Coverage is covered by...
        private string lineItemText = string.Empty; // Covers total trip cost 
        public string tNC { get; set; } = string.Empty; // By clicking on purchase...
        private string image = string.Empty;
        private string productId = string.Empty; // QuoteId 
        private string paymentContent = string.Empty; // Travel Guard® Insurance by AIG Travel
        private string pkDispenserPublicKey = string.Empty;
        private string confirmation1 = string.Empty;
        private string confirmation2 = string.Empty;
        private string tileImage = string.Empty;
        private string tileTitle1 = string.Empty;
        private string tileTitle2 = string.Empty;
        private string tileQuoteTitle = string.Empty;
        public List<MOBItem> tpiAIGReturnedMessageContentList { get; set; } //Covid-19 Emergency WHO TPI content

        public string ProductCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string DisplayAmount
        {
            get => this.displayAmount;
            set => this.displayAmount = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string FormattedDisplayAmount
        {
            get => this.formattedDisplayAmount;
            set => this.formattedDisplayAmount = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public double Amount { get; set; } // 14.12
        public string CoverCost
        {
            get => this.coverCost;
            set => this.coverCost = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string PageTitle
        {
            get => this.pageTitle;
            set => this.pageTitle = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Title1
        {
            get => this.title1;
            set => this.title1 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Title2
        {
            get => this.title2;
            set => this.title2 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string Title3
        {
            get => this.title3;
            set => this.title3 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }

        public string QuoteTitle
        {
            get => this.quoteTitle;
            set => this.quoteTitle = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Headline1
        {
            get => this.headline1;
            set => this.headline1 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Headline2
        {
            get => this.headline2;
            set => this.headline2 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Body1
        {
            get => this.body1;
            set => this.body1 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Body2
        {
            get => this.body2;
            set => this.body2 = string.IsNullOrEmpty(value) ? string.Empty : value;
        }
        public string Body3
        {
            get => this.body3;
            set => this.body3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string LineItemText
        {
            get => this.lineItemText;
            set => this.lineItemText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        //public string TNC
        //{
        //    get
        //    {
        //        return this.tNC;
        //    }
        //    set
        //    {
        //        this.tNC = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        //    }
        //}

        public string Image
        {
            get => this.image;
            set => this.image = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string ProductId
        {
            get => this.productId;
            set => this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }

        public string PaymentContent
        {
            get => this.paymentContent;
            set => this.paymentContent = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string PkDispenserPublicKey
        {
            get => this.pkDispenserPublicKey;
            set => this.pkDispenserPublicKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string Confirmation1
        {
            get => this.confirmation1;
            set => this.confirmation1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string Confirmation2
        {
            get => this.confirmation2;
            set => this.confirmation2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string TileImage
        {
            get => this.tileImage;
            set => this.tileImage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string TileTitle1
        {
            get => this.tileTitle1;
            set => this.tileTitle1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string TileTitle2
        {
            get => this.tileTitle2;
            set => this.tileTitle2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
        public string TileQuoteTitle
        {
            get => this.tileQuoteTitle;
            set => this.tileQuoteTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
        }
    }
}
