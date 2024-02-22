using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class MPPINPWSecurityQuestionsValidation : IPersist
    {
        #region IPersist Members

        public string ObjectName { get; set; } = "United.Persist.Definition.Profile.MPPINPWSecurityQuestionsValidation";

        #endregion

        public string SessionId { get; set; }

        public int RetryCount { get; set; }

        public List<Securityquestion> SecurityQuestionsFromCSL { get; set; }

        public List<Securityquestion> SecurityQuestionsSentToClient { get; set; }

        public bool AllSecurityQuestionsAnsweredCorrect { get; set; }
    }
}
