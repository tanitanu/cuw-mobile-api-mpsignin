using System.Collections.Generic;
using United.Mobile.Model.Common;

namespace United.Mobile.MemberSignIn.Domain
{
    public class SecurityquestionEqualityComparer : IEqualityComparer<Securityquestion>
    {
        public bool Equals(Securityquestion x, Securityquestion y)
        {
            return x.QuestionKey.Equals(y.QuestionKey);
        }

        public int GetHashCode(Securityquestion obj)
        {
            return obj.QuestionKey.GetHashCode();
        }
    }
}
