namespace BirthdayPokemonCore.Data
{
    public class PokemonDateFormat
    {
        public static bool IsDayMonthFormat(FormatType formatType)
        {
            return formatType == FormatType.DayMonth;
        }
    }

    public enum FormatType
    {
        MonthDay,
        DayMonth
    }
}
