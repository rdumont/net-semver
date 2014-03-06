using System;

namespace SemanticVersioning
{
    public class Comparator
    {
        private static readonly Version Any = new Version("0.0.0");
        private readonly Version _semver;

        public string Operator { get; private set; }

        public bool Loose { get; private set; }
        
        public string Value { get; private set; }

        public Comparator(string comp, bool loose = false)
        {
            Loose = loose;
            var regex = loose ? Re.ComparatorLoose : Re.Comparator;
            var match = regex.Match(comp);

            if (!match.Success)
                throw new FormatException("Invalid comparator: " + comp);

            Operator = match.Groups[1].Value;
            // if it literally is just '>' or '' then allow anything
            if (!match.Groups[2].Success)
                _semver = Any;
            else
            {
                _semver = new Version(match.Groups[2].Value, this.Loose);

                // <1.2.3-rc DOES allow 1.2.3-beta (has prerelease)
                // >=1.2.3 DOES NOT allow 1.2.3-beta
                // <=1.2.3 DOES allow 1.2.3-beta
                // However, <1.2.3 does NOT allow 1.2.3-beta,
                // even though `1.2.3-beta < 1.2.3`
                // The assumption is that the 1.2.3 version has something you
                // *don't* want, so we push the prerelease down to the minimum.
                if (this.Operator == "<" && _semver.Prerelease.Length == 0)
                {
                    _semver.Prerelease = new object[] {0};
                    _semver.Format();
                }
            }

            if (ReferenceEquals(_semver, Any))
                Value = "";
            else
                Value = Operator + _semver;
        }

        public string Inspect()
        {
            return string.Format("<SemVer Comparator \"{0}\"", this);
        }

        public override string ToString()
        {
            return Value;
        }

        public bool Test(string version)
        {
            return ReferenceEquals(_semver, Any) || SemVer.Compare(version, Operator, _semver, Loose);
        }

        public bool Test(Version version)
        {
            return ReferenceEquals(_semver, Any) || SemVer.Compare(version, Operator, _semver);
        }
    }
}
