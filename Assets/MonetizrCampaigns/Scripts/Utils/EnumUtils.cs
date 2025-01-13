using Monetizr.SDK.Debug;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Monetizr.SDK.Utils
{
    public static class EnumUtils
    {
        public static string GetEnumDescription (Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        public static bool IsEnumError (MessageEnum messageEnum)
        {
            if (messageEnum >= MessageEnum.M400 && messageEnum < MessageEnum.M600) return true;
            return false;
        }
    }
}