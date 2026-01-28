using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows.Markup;

namespace SmartLanche.Helpers
{
    public class EnumValue
    {
        public string DisplayName { get; set; } = string.Empty;
        public object Value { get; set; } = new object();
    }

    public class EnumValuesExtension : MarkupExtension
    {
        private Type _enumType;

        public EnumValuesExtension(Type enumType)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                throw new ArgumentException("enumType deve ser um tipo Enum.");
            }

            _enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var values = Enum.GetValues(_enumType);
            var result = new List<EnumValue>();

            foreach (var value in values)
            {
                var fieldInfo = _enumType.GetField(value.ToString()!);
                var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>(false);
                
                result.Add(new EnumValue
                {
                    DisplayName = displayAttribute?.Name ?? value.ToString()!,
                    Value = value
                });
            }

            return result; ;
        }

        public static string GetDisplayName(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            var displayAttribute = fieldInfo?.GetCustomAttribute<DisplayAttribute>(false);

            return displayAttribute?.Name ?? value.ToString();
        }

        public static class FilterOptions
        {
            public const string All = "ALL_STATUS";
        }
    }
}
