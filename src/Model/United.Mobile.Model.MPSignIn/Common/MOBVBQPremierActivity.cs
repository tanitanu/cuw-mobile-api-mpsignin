using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MOBVBQPremierActivity
    {
        //private string vBQPremierActivityTitle;
        //private string vBQPpremierActivityYear;
        //private string vBQPremierActivityStatus;
        //private MOBPremierQualifierTracker pQF;
        //private MOBPremierQualifierTracker pQP;
        public string vBQPremierActivityTitle { get; set; }
        //{
        //    get { return this.vBQPremierActivityTitle; }
        //    set { this.vBQPremierActivityTitle = value; }
        //}
        public string vBQPpremierActivityYear { get; set; }
        //{
        //    get { return this.vBQPpremierActivityYear; }
        //    set { this.vBQPpremierActivityYear = value; }
        //}
        public string vBQPremierActivityStatus { get; set; }
        //{
        //    get { return this.vBQPremierActivityStatus; }
        //    set { this.vBQPremierActivityStatus = value; }
        //}
        public MOBPremierQualifierTracker pQF { get; set; }
        //{
        //    get { return this.pQF; }
        //    set { this.pQF = value; }
        //}
        public MOBPremierQualifierTracker pQP { get; set; }
        //{
        //    get { return this.pQP; }
        //    set { this.pQP = value; }
        //}
        public MOBPremierQualifierTracker OutrightPQP { get; set; }
        public List<MOBKVP> KeyValueList { get; set; }
    }
}
