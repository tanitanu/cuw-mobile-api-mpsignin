using System;
using System.Collections.Generic;
using System.Text;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.MPAuthentication
{
    [Serializable]

    public class DependentInfo
    {
        public MOBName DependentName { get; set; }

        public string DependentId { get; set; }

        public string DateOfBirth { get; set; }

        public MOBEmpRelationship Relationship { get; set; }

        public int Age { get; set; }

    }
}
