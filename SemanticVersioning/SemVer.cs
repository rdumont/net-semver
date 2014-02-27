using System;
using System.Linq;

namespace SemanticVersioning
{
    public class SemVer : ICloneable
    {
        private readonly bool _loose;
        private readonly string _raw;
        private string _version;

        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public object[] Prerelease { get; set; }
        public string[] Build { get; set; }

        protected SemVer()
        {
        }

        public SemVer(SemVer version)
        {
            _loose = version._loose;
            _raw = version._raw;
            _version = version._version;
            Major = version.Major;
            Minor = version.Minor;
            Patch = version.Patch;
            Prerelease = (object[]) version.Prerelease.Clone();
            Build = (string[]) version.Build.Clone();
        }

        public SemVer(string version, bool loose = false)
        {
            _loose = loose;
            var match = (loose ? Re.Loose : Re.Full).Match(version.Trim());

            if (!match.Success)
                throw new Exception("Invalid Version: " + version);

            _raw = version;

            Major = int.Parse(match.Groups[1].Value);
            Minor = int.Parse(match.Groups[2].Value);
            Patch = int.Parse(match.Groups[3].Value);

            if (!match.Groups[4].Success)
                Prerelease = new object[] {};
            else
                Prerelease = match.Groups[4].Value.Split('.').Select(id =>
                    Re.Integer.IsMatch(id) ? int.Parse(id) : (object) id).ToArray();

            Build = match.Groups[5].Success
                ? match.Groups[5].Value.Split('.').ToArray()
                : new string[] {};

            Format();
        }

        public static SemVer Parse(string version, bool loose = false)
        {
            var regex = loose ? Re.Loose : Re.Full;
            return regex.IsMatch(version) ? new SemVer(version, loose) : null;
        }

        public static string Valid(string version, bool loose = false)
        {
            var semver = Parse(version, loose);
            return semver != null ? semver._version : null;
        }

        public static string Clean(string version, bool loose = false)
        {
            var semver = Parse(version, loose);
            return semver != null ? semver._version : null;
        }

        public string Format()
        {
            _version = Major + "." + Minor + "." + Patch;
            if (Prerelease.Length > 0)
                _version += "-" + string.Join(".", Prerelease);
            return _version;
        }

        public string Inspect()
        {
            return string.Format("SemVer \"{0}\">", this);
        }

        public override string ToString()
        {
            return _version;
        }

        public object Clone()
        {
            return new SemVer(this);
        }

        public int Compare(SemVer other)
        {
            var main = CompareMain(other);
            return main != 0 ? main : ComparePre(other);
        }

        protected int CompareMain(SemVer other)
        {
            var major = CompareIdentifiers(Major, other.Major);
            if (major != 0) return major;
            
            var minor = CompareIdentifiers(Minor, other.Minor);
            if (minor != 0) return minor;

            return CompareIdentifiers(Patch, other.Patch);
        }

        protected int ComparePre(SemVer other)
        {
            // NOT having a prerelease is > having one
            if (this.Prerelease.Length > 0 && other.Prerelease.Length == 0)
                return -1;

            if (this.Prerelease.Length == 0 && other.Prerelease.Length > 0)
                return 1;

            if (this.Prerelease.Length == 0 && other.Prerelease.Length == 0)
                return 0;

            for (var i = 0; i < Math.Max(this.Prerelease.Length, other.Prerelease.Length); i++)
            {
                if (other.Prerelease.Length == i)
                    return 1;

                if (this.Prerelease.Length == i)
                    return -1;

                var compare = CompareIdentifiers(this.Prerelease[i], other.Prerelease[i]);
                if (compare != 0)
                    return compare;
            }
            return 0;
        }

        protected static int CompareIdentifiers(object a, object b)
        {
            var anum = a as int?;
            var bnum = b as int?;
            var astr = a.ToString();
            var bstr = b.ToString();

            if (anum != null && bnum != null)
                return anum == bnum ? 0 : (anum < bnum ? -1 : 1);

            if (anum == null && bnum == null)
            {
                var stringCompare = string.CompareOrdinal(astr, bstr);
                return stringCompare > 0 ? 1 : stringCompare < 0 ? -1 : 0;
            }

            if (anum != null)
                return -1;

            return 1;
        }
    }
}
