using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Csl.Ms.Common.Models;

namespace United.Mobile.Model.OneClickEnrollment
{
    public class CslResponse<T>
    {
        public dynamic Meta { get; set; }
        public T Data { get; set; }
        public dynamic Link { get; set; }
        public IEnumerable<Error> Errors { get; set; }
    }
    public class CslUcbResponse<T>
    {
        public dynamic Meta { get; set; }
        public T Data { get; set; }
        public dynamic Link { get; set; }
        public IEnumerable<UCBError> Errors { get; set; }
    }
    public class UCBError
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }       
    }
}
