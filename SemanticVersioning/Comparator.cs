using System;
using System.Text.RegularExpressions;

namespace SemanticVersioning
{
    public class Comparator
    {
        private static readonly Version Any = new Version(0, 0, 0);
        private readonly Version _semver;

        public string Operator { get; private set; }

        public string Version { get; private set; }

        public Comparator(string op, Version version)
        {
            this.Operator = op;
            this.Version = ReferenceEquals(version, Any) ? string.Empty : version.ToString();
            _semver = version;
        }

        public static Comparator Parse(string source, bool loose = false)
        {
            Comparator comparator;
            if (TryParse(source, out comparator, loose))
                return comparator;

            throw new FormatException("Invalid comparator: " + source);
        }

        public static bool TryParse(string source, out Comparator comparator, bool loose = false)
        {
            var regex = loose ? Re.ComparatorLoose : Re.Comparator;
            var match = regex.Match(source);

            if (!match.Success)
            {
                comparator = null;
                return false;
            }
            comparator = Parse(match, source, loose);
            return true;
        }

        internal static Comparator Parse(Match match, string source, bool loose = false)
        {
            if(!match.Success)
                throw new FormatException("Invalid comparator: " + source);

            var op = match.Groups[1].Value;
            Version version;

            // if it literally is just '>' or '' then allow anything
            if (!match.Groups[2].Success)
                version = Any;
            else
            {
                version = SemanticVersioning.Version.Parse(match.Groups[2].Value, loose);

                // <1.2.3-rc DOES allow 1.2.3-beta (has prerelease)
                // >=1.2.3 DOES NOT allow 1.2.3-beta
                // <=1.2.3 DOES allow 1.2.3-beta
                // However, <1.2.3 does NOT allow 1.2.3-beta,
                // even though `1.2.3-beta < 1.2.3`
                // The assumption is that the 1.2.3 version has something you
                // *don't* want, so we push the prerelease down to the minimum.
                if (op == "<" && version.Prerelease.Length == 0)
                {
                    version.Prerelease = new VersionIdentifier[] {0};
                }
            }

            return new Comparator(op, version);
        }

        public bool Matches(Version version)
        {
            return ReferenceEquals(_semver, Any) || SemVer.Compare(version, Operator, _semver);
        }
        
        public override string ToString()
        {
            return Version;
        }
    }
}
