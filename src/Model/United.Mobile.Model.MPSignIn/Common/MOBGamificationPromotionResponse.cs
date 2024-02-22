using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace United.Mobile.Model.MPSignIn
{
        [Serializable]
        public class Promotion
        {
            [DataMember]
            public string PromotionId { get; set; }

            [DataMember]
            public DateTime EffectiveDate { get; set; }

            [DataMember]
            public DateTime ExpirationDate { get; set; }

            [DataMember]
            public DateTime RegistrationEffectiveDate { get; set; }

            [DataMember]
            public DateTime RegistrationExpirationDate { get; set; }

            [DataMember]
            public DateTime QualificationEffectiveDate { get; set; }

            [DataMember]
            public DateTime QualificationExpirationDate { get; set; }

            [DataMember]
            public DateTime DisplayEffectiveDate { get; set; }

            [DataMember]
            public DateTime DisplayExpirationDate { get; set; }

            [DataMember]
            public bool IsTargeted { get; set; }

            public string Url { get; set; }

            [DataMember]
            public string VanityUrl { get; set; }

            [DataMember]
            public string Description { get; set; }

            [DataMember]
            public string XmlMetaData { get; set; }

            [DataMember]
            public dynamic MetaData { get; set; }
    }
        [Serializable]
        public class MemberPromotion<TMetaData>
        {

            [DataMember]
            public Guid MemberPromotionId { get; set; }

            [DataMember]
            public string MpId { get; set; }

            [DataMember]
            public string AltRefId1 { get; set; }

            [DataMember]
            public string AltRefId2 { get; set; }

            [DataMember]
            public string PromotionId { get; set; }

            [DataMember]
            public Promotion Promotion { get; set; }

            [DataMember]
            public string DisplayName { get; set; }

            [DataMember]
            public DateTime EffectiveDate { get; set; }

            [DataMember]
            public DateTime ExpirationDate { get; set; }

            [DataMember]
            public bool IsActive { get; set; }

            [DataMember]
            public string EmailId { get; set; }

            [DataMember]
            public bool IsRegistered { get; set; }

            [DataMember]
            public DateTime? RegistrationTimestamp { get; set; }

            [DataMember]
            public bool IsRegisteredInIms { get; set; }

            [DataMember]
            public DateTime? ImsRegistrationTimestamp { get; set; }

            [DataMember]
            public DateTime InsertTimestamp { get; set; }

            [DataMember]
            public string InsertId { get; set; }

            [DataMember]
            public DateTime? UpdateTimestamp { get; set; }

            [DataMember]
            public string UpdateId { get; set; }

            [DataMember]
            public TMetaData MetaData { get; set; }

            [DataMember]
            public string XmlMetaData { get; set; }

            [DataMember]
            public string State { get; set; }

            [DataMember]
            public string ImsPromotionId { get; set; }

            [DataMember]
            public string ChannelRegistrationId { get; set; }
    }
    
}
