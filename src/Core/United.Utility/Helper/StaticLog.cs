using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Utility.Helper
{
    public static class StaticLog
    {
        public static bool Information(ICacheLog logger,string messageTemplate, params string[] Values)
        {
            if (logger != null && !string.IsNullOrEmpty(messageTemplate))
            {
                messageTemplate = GeneralHelper.RemoveCarriageReturn(messageTemplate);

                for(int i=0;i< Values.Length; i++)
                {
                    Values[i] = GeneralHelper.RemoveCarriageReturn(Values[i]); 
                }

                logger.LogInformation(messageTemplate, Values);
                return true;
            }
            return false;
        }
        public static bool Error(ICacheLog logger, string messageTemplate, params string[] Values)
        {
            if (logger != null && !string.IsNullOrEmpty(messageTemplate))
            {
                messageTemplate = GeneralHelper.RemoveCarriageReturn(messageTemplate);

                for (int i = 0; i < Values.Length; i++)
                {
                    Values[i] = GeneralHelper.RemoveCarriageReturn(Values[i]);
                }

                logger.LogError(messageTemplate, Values);
                return true;
            }
            return false;
        }
        public static bool Warning(ICacheLog logger, string messageTemplate, params string[] Values)
        {

            if ( logger != null &&  !string.IsNullOrEmpty(messageTemplate) )
            {
                messageTemplate = GeneralHelper.RemoveCarriageReturn(messageTemplate);

                for (int i = 0; i < Values.Length; i++)
                {
                    Values[i] = GeneralHelper.RemoveCarriageReturn(Values[i]);
                }

                logger.LogWarning(messageTemplate, Values);
                return true;
            }
            return false;
        }
    }
}
