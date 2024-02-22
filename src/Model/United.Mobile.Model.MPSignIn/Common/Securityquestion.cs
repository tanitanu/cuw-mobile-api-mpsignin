using System;
using System.Collections.Generic;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class Securityquestion
    {
        public int QuestionID { get; set; } 

        public string QuestionKey { get; set; }

        public string QuestionText { get; set; }

        public bool Used { get; set; }

        public List<Answer> Answers { get; set; }

    }
}
