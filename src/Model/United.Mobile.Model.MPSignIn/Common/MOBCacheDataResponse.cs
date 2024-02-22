using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MPSignIn
{
    [Serializable]
    public class MOBCacheDataResponse
    {
        public MOBCacheDataResponse()
            : base()
        {
        }

        private string cacheData;

        public int Id { get; set; }

        public bool BlnRefresh { get; set; }

        public DateTime LastUpdateDateTime { get; set; }

        public string CacheData
        {
            get
            {
                return this.cacheData;
            }
            set
            {
                this.cacheData = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
