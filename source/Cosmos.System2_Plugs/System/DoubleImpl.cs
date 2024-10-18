using Cosmos.Common;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(double))]
    public static class DoubleImpl
    {
        public static string ToString(ref double aThis)
        {
            return StringHelper.GetNumberString(aThis);
        }

        public static string ToString(ref double aThis, string format, IFormatProvider provider) => aThis.ToString();

        public static double Parse(string s)
        {
            //Format of Double string: [whitespace][sign][integral-digits[,]]integral-digits[.[fractional-digits]][E[sign]exponential-digits][whitespace]

            //Validate input
            if (s is null)
            {
                throw new ArgumentNullException(nameof(s), "Value can not be null");
            }

            //Remove leading whitespaces
            while (s.Length != 0 && (s[0] == ' ' || s[0] == '\n' || s[0] == '\t'))
            {
                s = s[1..];
            }

            //Check that string is not finished too early
            if (s.Length == 0)
            {
                throw new FormatException();
            }

            //Check for sign
            short sign = 1;
            if (s[0] == '-')
            {
                s = s[1..];
                sign = -1;
            }
            else if (s[0] == '+')
            {
                s = s[1..];
            }

            //Check that string is not finished too early
            if (s.Length == 0)
            {
                throw new FormatException();
            }

            //Read in number

            List<int> internalDigits = new();
            List<int> fractionalDigits = new();

            bool foundDecimal = false;

            //Iterate until fully parsed or an E/Whitespace is found
            //Assume english standard, so . == decimal seperator and , == thousands seperator
            while (s.Length != 0)
            {
                char active = s[0];
                if (active == 'E' || active == 'e' || active == ' ' || active == '\n' || active == '\t')
                {
                    break;
                }

                s = s[1..];
                if (active == '.')
                {
                    foundDecimal = true;
                }
                else if (active == ',')
                {
                    continue;
                }
                else if (active >= '0' && active <= '9')
                {
                    if (foundDecimal)
                    {
                        fractionalDigits.Add(int.Parse(active.ToString()));
                    }
                    else
                    {
                        internalDigits.Add(int.Parse(active.ToString()));
                    }
                }
                else
                {
                    throw new FormatException();
                }
            }

            //Iterate through rest of string
            double multiplier = 0;
            while (s.Length != 0)
            {
                //Check for exponential notation i.e. 8.1E10 = 8.1 * 10^10 + Whitespaces
                //E can only be followed by integers
                if (s[0] == 'E' || s[0] == 'e')
                {
                    multiplier = double.Parse(s[1..]);
                    break;
                }
                else if (s[0] == ' ' || s[0] == '\n' || s[0] == '\t')
                {
                    s = s[1..];
                }
                else
                {
                    throw new FormatException();
                }
            }

            //Create double
            double parsed = 0;
            for (int i = 0; i < internalDigits.Count; i++)
            {
                parsed += internalDigits[i] * Math.Pow(10, internalDigits.Count - (i + 1));
            }
            for (int i = 0; i < fractionalDigits.Count; i++)
            {
                parsed += fractionalDigits[i] * Math.Pow(10, -1 * (i + 1));
            }

            parsed *= Math.Pow(10, multiplier);
            parsed *= sign;

            return parsed;
        }

        public static bool TryParse(string s, out double result)
        {
            try
            {
                result = Parse(s);
                return true;
            }
            catch (Exception)
            {
                result = 0;
                return false;
            }
        }
    }
}