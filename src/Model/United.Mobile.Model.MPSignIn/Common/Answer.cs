using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.Common
{
    [Serializable()]
    public class Answer
    {
        public int AnswerID { get; set; }
        
        public string AnswerKey { get; set; }

        public string QuestionKey { get; set; } 
        
        public int QuestionID { get; set; }
        
        public string AnswerText { get; set; }
       
    }
}
