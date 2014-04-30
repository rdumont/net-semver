using System;

namespace SemanticVersioning
{
    public static class SemVer
    {
        public static bool Compare(Version a, string op, Version b)
        {
            switch (op)
            {
                case "===":
                    return ReferenceEquals(a, b);
                case "!==":
                    return !ReferenceEquals(a, b);
                case "!=":
                    return a != b;
                case ">":
                    return a > b;
                case ">=":
                    return a >= b;
                case "<":
                    return a < b;
                case "<=":
                    return a <= b;
                case "":
                case "=":
                case "==":
                    return a == b;
                default:
                    throw new ArgumentException("Invalid operator: " + op, "op");
            }
        }

        public static bool Satisfies(Version version, string rangeString, bool loose = false)
        {
            Range range;
            try
            {
                range = Range.Parse(rangeString, loose);
            }
            catch (FormatException)
            {
                return false;
            }
            return range.Matches(version);
        }
    }
}
