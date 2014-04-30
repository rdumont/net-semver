using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SemanticVersioning
{
    public class Version : ICloneable, IEquatable<Version>, IComparable<Version>
    {
        private int _major;
        private int _minor;
        private int _patch;
        private VersionIdentifier[] _prerelease;
        private string[] _build;

        private readonly bool _loose;
        private readonly string _raw;
        private string _version;

        public int Major
        {
            get { return _major; }
            set
            {
                _major = value;
                Format();
            }
        }

        public int Minor
        {
            get { return _minor; }
            set
            {
                _minor = value;
                Format();
            }
        }

        public int Patch
        {
            get { return _patch; }
            set
            {
                _patch = value;
                Format();
            }
        }

        public VersionIdentifier[] Prerelease
        {
            get { return _prerelease; }
            set
            {
                _prerelease = value ?? new VersionIdentifier[0];
                Format();
            }
        }

        public string[] Build
        {
            get { return _build; }
            set
            {
                _build = value ?? new string[0];
                Format();
            }
        }

        public Version(int major, int minor, int patch)
            : this(major, minor, patch, null, null)
        {
        }

        public Version(int major, int minor, int patch, VersionIdentifier[] prerelease)
            : this(major, minor, patch, prerelease, null)
        {
        }

        public Version(int major, int minor, int patch, VersionIdentifier[] prerelease, string[] build)
            : this(major, minor, patch, prerelease, build, false, string.Empty)
        {
        }

        private Version(int major, int minor, int patch, VersionIdentifier[] prerelease, string[] build, bool loose,
            string raw)
        {
            _major = major;
            _minor = minor;
            _patch = patch;
            _prerelease = prerelease ?? new VersionIdentifier[0];
            _build = build ?? new string[0];
            _loose = loose;
            _raw = raw;
            Format();
        }

        protected Version()
            : this(0, 0, 0, null, null)
        {
        }

        public static Version Parse(string source, bool loose = false)
        {
            Version version;
            if(TryParse(source, out version, loose))
                return version;

            throw new FormatException("Invalid Version: " + source);
        }

        public static bool TryParse(string source, out Version version, bool loose = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var match = (loose ? Re.Loose : Re.Full).Match(source.Trim());
            if (!match.Success)
            {
                version = null;
                return false;
            }
            version = Parse(match, source, loose);
            return true;
        }

        private static Version Parse(Match match, string source, bool loose)
        {
            var major = int.Parse(match.Groups[1].Value);
            var minor = int.Parse(match.Groups[2].Value);
            var patch = int.Parse(match.Groups[3].Value);

            var preGroup = match.Groups[4];
            var prerelease = preGroup.Success
                ? preGroup.Value.Split('.').Select(VersionIdentifier.Parse).ToArray()
                : new VersionIdentifier[0];

            var buildGroup = match.Groups[5];
            var build = buildGroup.Success
                ? buildGroup.Value.Split('.').ToArray()
                : new string[] { };

            return new Version(major, minor, patch, prerelease, build, loose, source);
        }

        public bool Satisfies(string range)
        {
            try
            {
                Range actualRange;
                return Range.TryParse(range, out actualRange, _loose) && actualRange.Matches(this);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public int CompareTo(Version other)
        {
            var main = CompareMain(other);
            return main != 0 ? main : ComparePre(other);
        }

        protected int CompareMain(Version other)
        {
            var major = _major.CompareTo(other._major);
            if (major != 0) return major;

            var minor = _minor.CompareTo(other._minor);
            if (minor != 0) return minor;

            return _patch.CompareTo(other._patch);
        }

        protected int ComparePre(Version other)
        {
            // NOT having a prerelease is > having one
            if (this._prerelease.Length > 0 && other._prerelease.Length == 0)
                return -1;

            if (this._prerelease.Length == 0 && other._prerelease.Length > 0)
                return 1;

            if (this._prerelease.Length == 0 && other._prerelease.Length == 0)
                return 0;

            for (var i = 0; i < Math.Max(this._prerelease.Length, other._prerelease.Length); i++)
            {
                if (other._prerelease.Length == i)
                    return 1;

                if (this._prerelease.Length == i)
                    return -1;

                var compare = this._prerelease[i].CompareTo(other._prerelease[i]);
                if (compare != 0)
                    return compare;
            }
            return 0;
        }

        public void Increment(IncrementType type)
        {
            switch (type)
            {
                case IncrementType.Major:
                    _major++;
                    _minor = -1;
                    goto case IncrementType.Minor;
                case IncrementType.Minor:
                    _minor++;
                    _patch = -1;
                    goto case IncrementType.Patch;
                case IncrementType.Patch:
                    _patch++;
                    _prerelease = new VersionIdentifier[0];
                    break;
                case IncrementType.Prerelease:
                    if (_prerelease.Length == 0)
                        _prerelease = new VersionIdentifier[] { 0 };
                    else
                    {
                        var incrementedSomething = false;
                        for (var i = _prerelease.Length - 1; i >= 0; i--)
                        {
                            var integerValue = _prerelease[i].IntegerValue;
                            if (integerValue != null)
                            {
                                _prerelease[i] = integerValue + 1;
                                incrementedSomething = true;
                                break;
                            }
                        }
                        if (!incrementedSomething)
                            _prerelease = new List<VersionIdentifier>(_prerelease) {0}.ToArray();
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid increment: " + type, "type");
            }
            this.Format();
        }

        public object Clone()
        {
            return new Version(_major, _minor, _patch, _prerelease, _build, _loose, _raw);
        }

        public bool Equals(Version other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.CompareTo(other) == 0;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Version)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _loose.GetHashCode();
                hashCode = (hashCode * 397) ^ _major;
                hashCode = (hashCode * 397) ^ _minor;
                hashCode = (hashCode * 397) ^ _patch;
                hashCode = (hashCode * 397) ^ _prerelease.GetHashCode();
                hashCode = (hashCode * 397) ^ _build.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return _version;
        }

        private void Format()
        {
            _version = _major + "." + _minor + "." + _patch;
            if (_prerelease.Length > 0)
                _version += "-" + string.Join(".", _prerelease.Cast<object>());
        }

        #region SemVer Operator Overloads

        public static bool operator ==(Version v1, Version v2)
        {
            if (ReferenceEquals(v1, null))
                return ReferenceEquals(v2, null);
            return v1.CompareTo(v2) == 0;
        }

        public static bool operator !=(Version v1, Version v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(Version v1, Version v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.CompareTo(v2) < 0;
        }

        public static bool operator >(Version v1, Version v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.CompareTo(v2) > 0;
        }

        public static bool operator <=(Version v1, Version v2)
        {
            return !(v1 > v2);
        }

        public static bool operator >=(Version v1, Version v2)
        {
            return !(v1 < v2);
        }

        #endregion

        #region SemVer to String Operator Overloads

        public static bool operator ==(Version v1, string v2)
        {
            if (ReferenceEquals(v1, null))
                return false;
            return v1.CompareTo(Parse(v2, v1._loose)) == 0;
        }

        public static bool operator !=(Version v1, string v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(Version v1, string v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (string.IsNullOrWhiteSpace(v2))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.CompareTo(Parse(v2, v1._loose)) < 0;
        }

        public static bool operator >(Version v1, string v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (string.IsNullOrWhiteSpace(v2))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.CompareTo(Parse(v2, v1._loose)) > 0;
        }

        public static bool operator <=(Version v1, string v2)
        {
            return !(v1 > v2);
        }

        public static bool operator >=(Version v1, string v2)
        {
            return !(v1 < v2);
        }

        #endregion

        #region String to SemVer Operator Overloads

        public static bool operator ==(string v1, Version v2)
        {
            if (ReferenceEquals(v2, null))
                return false;
            return v2.CompareTo(Parse(v1, v2._loose)) == 0;
        }

        public static bool operator !=(string v1, Version v2)
        {
            return !(v1 == v2);
        }

        public static bool operator <(string v1, Version v2)
        {
            if (string.IsNullOrWhiteSpace(v1))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            return v2.CompareTo(Parse(v1, v2._loose)) > 0;
        }

        public static bool operator >(string v1, Version v2)
        {
            if (string.IsNullOrWhiteSpace(v1))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            return v2.CompareTo(Parse(v1, v2._loose)) < 0;
        }

        public static bool operator <=(string v1, Version v2)
        {
            return !(v1 > v2);
        }

        public static bool operator >=(string v1, Version v2)
        {
            return !(v1 < v2);
        }

        #endregion
    }
}
