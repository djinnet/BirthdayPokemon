using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirthdayPokemonCore
{
    /// <summary>
    /// A helper class for building colored strings for console output by using StringBuilder.
    /// </summary>
    public class ColorStringBuilder
    {
        public ColorStringBuilder() { }
        private StringBuilder _builder = new StringBuilder();
        private List<(int Start, int Length, ConsoleColor Color)> _colorSpans = new List<(int, int, ConsoleColor)>();
        public ColorStringBuilder Append(string text, ConsoleColor color)
        {
            int start = _builder.Length;
            _builder.Append(text);
            _colorSpans.Add((start, text.Length, color));
            return this;
        }

        public ColorStringBuilder AppendLine(string text, ConsoleColor color)
        {
            return Append(text + Environment.NewLine, color);
        }

        public void WriteToConsole()
        {
            int currentIndex = 0;
            foreach (var (Start, Length, Color) in _colorSpans.OrderBy(span => span.Start))
            {
                // Write any text before the colored span
                if (currentIndex < Start)
                {
                    Console.ResetColor();
                    Console.Write(_builder.ToString(currentIndex, Start - currentIndex));
                    currentIndex = Start;
                }
                // Write the colored span
                Console.ForegroundColor = Color;
                Console.Write(_builder.ToString(Start, Length));
                currentIndex = Start + Length;
            }
            // Write any remaining text after the last colored span
            if (currentIndex < _builder.Length)
            {
                Console.ResetColor();
                Console.Write(_builder.ToString(currentIndex, _builder.Length - currentIndex));
            }
            // Reset color at the end
            Console.ResetColor();
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
