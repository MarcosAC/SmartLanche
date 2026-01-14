using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace SmartLanche.Helpers
{
    public static class EnumDisplayHelper
    {
        public static string GetDisplayName(this Enum enumValue)
        {            
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            
            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>(false);
            
            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}