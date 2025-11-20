using BirthdayPokemonCore.Data.Enums;
using Microsoft.Extensions.Logging;

namespace BirthdayPokemonCore.Data.Extensions
{
    public static class PokemonDateFormatExtension
    {
        public static bool IsDayMonthFormat(this EFormatType formatType)
        {
            return formatType == EFormatType.DayMonth;
        }

        public static bool DateValidation(this DateOnly birthday)
        {
            try
            {
                //validate every edge cases with day, month and year
                if (birthday.Day == 0)
                {
                    throw new ArgumentException("Day cannot be zero.");
                }

                if (birthday.Month == 0)
                {
                    throw new ArgumentException("Month cannot be zero.");
                }

                if (birthday.Year == 0)
                {
                    throw new ArgumentException("Year cannot be zero.");
                }

                if (birthday.Day > 31)
                {
                    throw new ArgumentException("Day cannot be greater than 31.");
                }

                if (birthday.Month > 12)
                {
                    throw new ArgumentException("Month cannot be greater than 12.");
                }

                if (birthday.Year < 1850 && birthday.Year > 2100)
                {
                    throw new ArgumentException("Year must be between 1850 and 2100.");
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
