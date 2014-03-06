using System;

namespace SemanticVersioning
{
    public static class SemVer
    {
        public static bool Compare(string a, string op, string b, bool loose = false)
        {
            var v1 = new Version(a, loose);
            var v2 = new Version(b, loose);
            return Compare(v1, op, v2);
        }

        public static bool Compare(Version a, string op, string b, bool loose = false)
        {
            var v2 = new Version(b, loose);
            return Compare(a, op, v2);
        }

        public static bool Compare(string a, string op, Version b, bool loose = false)
        {
            var v1 = new Version(a, loose);
            return Compare(v1, op, b);
        }

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
                default:
                    throw new ArgumentException("Invalid operator: " + op, "op");
            }
        }
    }
}
