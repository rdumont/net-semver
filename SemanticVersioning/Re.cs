using System.Text.RegularExpressions;

namespace SemanticVersioning
{
    public static class Re
    {
        public static Regex Full = new Regex(ReSrc.Full, RegexOptions.Compiled);
        public static Regex Loose = new Regex(ReSrc.Loose, RegexOptions.Compiled);
        public static Regex Integer = new Regex("^[0-9]+$", RegexOptions.Compiled);
    }
}
