using System;

namespace FridgeManagementSystem.Helpers
{
    public static class StringExtensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        // You can add more string extension methods here
        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string FormatPhoneNumber(this string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber)) return phoneNumber;
            // Simple phone number formatting
            return phoneNumber.Length == 10 ?
                $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3, 3)}-{phoneNumber.Substring(6)}" :
                phoneNumber;
        }
    }
}
