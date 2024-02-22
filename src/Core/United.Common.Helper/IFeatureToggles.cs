using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace United.Common.Helper
{
    public interface IFeatureToggles
    {
        Task<bool> IsEnableU4BForMultipax(int applicationId, string appVersion);
    }
}
