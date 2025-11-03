using BirthdayPokemonCore.Data.Enums;

namespace BirthdayPokemonCore.Data.Extensions
{
    public class PokemonDateFormatExtension
    {
        public static bool IsDayMonthFormat(EFormatType formatType)
        {
            return formatType == EFormatType.DayMonth;
        }
    }
}
