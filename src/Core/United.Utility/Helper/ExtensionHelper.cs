using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace United.Utility.Helper
{
    public static class ExtensionHelper
    {
        public static string GetDisplayName(this System.Enum enumValue)
        {
            return enumValue.GetType()?
                            .GetMember(enumValue.ToString())?
                            .First()?
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name;
        }
    }
}
