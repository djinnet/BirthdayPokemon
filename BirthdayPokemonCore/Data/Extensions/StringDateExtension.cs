using BirthdayPokemonCore.Data.Enums;

namespace BirthdayPokemonCore.Data.Extensions
{
    public static class StringDateExtension
    {
        /// <summary>
        /// Validates if the input string is formatted correctly as a date.
        /// </summary>
        /// <param name="input">The input. The format should be in any date format</param>
        /// <returns>True if the input is formatted correctly; otherwise, false.</returns>
        /// <remarks>
        /// The method checks if the input string can be split into at least two numeric parts.
        /// </remarks>
        public static bool isInputStringFormattedCorrectly(this string input)
        {
            string[] parts = input.Split('/', '-', '.', ' ');
            if (parts.Length < 2)
                return false;
            bool isFirstNumeric = int.TryParse(parts[0], out int first);
            bool isSecondNumeric = int.TryParse(parts[1], out int second);
            return isFirstNumeric && isSecondNumeric;
        }

        /// <summary>
        /// Parses a flexible date string in either DD/MM or MM/DD format.
        /// </summary>
        /// <param name="input">The input. The format should be in any date format</param>
        /// <returns>
        /// A DateOnly object representing the parsed date.
        /// </returns>
        /// <exception cref="FormatException">
        /// Thrown when the input format is invalid.
        /// </exception>
        /// <remarks>
        /// If the first part is greater than 12, it is treated as DD/MM; otherwise, MM/DD.
        /// </remarks>
        public static DateOnly ParseFlexibleDate(this string input)
        {
            string[] parts = input.Split('/', '-', '.', ' ');
            if (parts.Length < 2)
                throw new FormatException("Invalid date format.");

            int first = int.Parse(parts[0]);
            int second = int.Parse(parts[1]);
            int year = parts.Length >= 3 ? int.Parse(parts[2]) : DateTime.Now.Year;

            if (first > 12 && second <= 12)
                return new DateOnly(year, second, first); // DD/MM
            else
                return new DateOnly(year, first, second); // MM/DD
        }

        public static DateOnly ParseDateWithFormat(this string input, bool dayFirst)
        {
            string[] parts = input.Split('/', '-', '.', ' ');
            if (parts.Length < 2)
                throw new FormatException("Invalid date format.");

            int first = int.Parse(parts[0]);
            int second = int.Parse(parts[1]);
            int year = parts.Length >= 3 ? int.Parse(parts[2]) : DateTime.Now.Year;

            if (dayFirst)
                return new DateOnly(year, second, first);
            else
                return new DateOnly(year, first, second);
        }

        public static DateOnly ParseDateWithFormat(this string input, EFormatType formatType)
        {
            string[] parts = input.Split('/', '-', '.', ' ');
            if (parts.Length < 2)
                throw new FormatException("Invalid date format.");

            int first = int.Parse(parts[0]);
            int second = int.Parse(parts[1]);
            int year = parts.Length >= 3 ? int.Parse(parts[2]) : DateTime.Now.Year;

            if (PokemonDateFormatExtension.IsDayMonthFormat(formatType))
                return new DateOnly(year, second, first);
            else
                return new DateOnly(year, first, second);
        }

        public static EFormatType ToFormatType(this string input)
        {
            try
            {
                string[] parts = input.Split('/', '-', '.', ' ');
                int first = int.Parse(parts[0]);
                int second = int.Parse(parts[1]);
                EFormatType formatUsed = first > 12 ? EFormatType.DayMonth : EFormatType.MonthDay;
                return formatUsed;
            }
            catch (Exception)
            {
                return EFormatType.MonthDay;
            }
        }

        /// <summary>
        /// Determines if the input date string is in DD/MM format or MM/DD format.
        /// </summary>
        /// <param name="input">The input. The format should be in any date format</param>
        /// <returns>The boolean value if input is in DD/mm format or MM/DD format.</returns>
        /// <remarks>
        /// Returns true if the input is in DD/MM format, false if in MM/DD format.
        /// </remarks>
        public static bool ParseFlexibleDateAsBoolean(this string input)
        {
            string[] parts = input.Split('/', '-', '.', ' ');
            if (parts.Length < 2)
                return false;

            int first = int.Parse(parts[0]);
            int second = int.Parse(parts[1]);

            return first > 12 && second <= 12;
        }


    }
}
