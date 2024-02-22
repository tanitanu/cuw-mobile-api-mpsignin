using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    public class MOBMPStatusLiftBanner
    {
        [System.Text.Json.Serialization.JsonPropertyName("ImageSrcURL")]
        [JsonProperty("ImageSrcURL")]
        public string ImageSrcURL { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("PremierStatusURL")]
        [JsonProperty("PremierStatusURL")]
        public string PremierStatusURL { get; set; }

        //public string imageSrcURL
        //{
        //    get
        //    {
        //        return this.ImageSrcURL;
        //    }
        //    set
        //    {
        //        this.ImageSrcURL = value;
        //    }
        //}

        //public string premierStatusURL
        //{
        //    get
        //    {
        //        return this.PremierStatusURL;
        //    }
        //    set
        //    {
        //        this.PremierStatusURL = value;
        //    }
        //}

    }
}
