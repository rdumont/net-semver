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
    }
}
