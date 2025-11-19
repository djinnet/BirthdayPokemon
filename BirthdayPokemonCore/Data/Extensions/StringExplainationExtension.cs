using BirthdayPokemonCore.Data.Enums;
using BirthdayPokemonCore.Models;
using System.Text;

namespace BirthdayPokemonCore.Data.Extensions
{
    public static class StringExplainationExtension
    {
        public static string ToExplaniationString(this PokemonInfo pokemonInfo, DateOnly birthday, EFormatType formatType, EPokemonBirthdayCalculationType calculationType, string output, int normalizedDex)
        {
            StringBuilder explanation = new();
            explanation.AppendLine($"Parsed date: {birthday:dd/MM/yyyy}");
            explanation.AppendLine($"Using specified format: {formatType.ToString()}");
            explanation.AppendLine($"Using calculation type: {calculationType}");
            explanation.AppendLine(output);
            explanation.AppendLine($"Result -> #{normalizedDex}: {pokemonInfo?.Name}");
            return explanation.ToString();
        }

        public static string ToExplainationString(this DateOnly birthday, int MaxDexNumber, int result)
        {
            DateOnly date = birthday;
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;
            int baseYear = year % 2 == 0 ? year - 2 : year - 1;
            DateOnly baseDate = new(baseYear, month, day);
            int[] yearDigits = year.ToString().Select(c => int.Parse(c.ToString())).ToArray();
            int seed = yearDigits[3] * 100 + yearDigits[2];
            int datediff = date.DayOfYear - baseDate.DayOfYear;

            //write explanation log with whole calculation steps
            StringBuilder explanation = new();
            //How we calculated baseyear
            explanation.AppendLine($"Base Year Calculation: {(year % 2 == 0 ? $"{year} is even, so base year is {year - 2}" : $"{year} is odd, so base year is {year - 1}")}");

            //how we calculated seed
            explanation.AppendLine($"Seed Calculation: Last two digits of year {year} are {yearDigits[2]} and {yearDigits[3]}, so seed is ({yearDigits[3]} * 100) + {yearDigits[2]} = {seed}");
            explanation.AppendLine($"Date Difference Calculation: Day of year for {date:dd/MM/yyyy} is {date.DayOfYear}, Day of year for base date {baseDate:dd/MM/yyyy} is {baseDate.DayOfYear}, so difference is {date.DayOfYear} - {baseDate.DayOfYear} = {datediff}");
            explanation.AppendLine($"Final Calculation: (((( {datediff} ) / 86400000) + {seed}) % {MaxDexNumber}) + 1 = {result}");
            return explanation.ToString();
        }

        public static ColorStringBuilder ToExplainationString(this DateOnly birthday, EFormatType formatType, int normalized, string name)
        {
            ColorStringBuilder colorSB = new();
            colorSB.AppendLine("Explanation:", ConsoleColor.Yellow);
            colorSB.AppendLine("-------------------------", ConsoleColor.Yellow);
            colorSB.AppendLine($"Parsed date: {birthday:dd/MM/yyyy}", ConsoleColor.Cyan);
            colorSB.AppendLine($"Using specified format: {formatType.ToString()}", ConsoleColor.Cyan);
            colorSB.AppendLine($"Result -> #{normalized}: {name}", ConsoleColor.Green);
            return colorSB;

            //StringBuilder explanation = new();
            //explanation.AppendLine($"Parsed date: {birthday:dd/MM/yyyy}");
            //explanation.AppendLine($"Using specified format: {formatType.ToString()}");
            //explanation.AppendLine($"Result -> #{normalized}: {name}");
            //return explanation.ToString();
        }
    }
}
