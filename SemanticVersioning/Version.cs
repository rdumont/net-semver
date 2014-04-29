using System;
using System.Collections.Generic;
using System.Linq;

namespace SemanticVersioning
{
    public class Version : ICloneable, IEquatable<Version>
    {
        private int _major;
        private int _minor;
        private int _patch;
        private object[] _prerelease;
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

        public object[] Prerelease
        {
            get { return _prerelease; }
            set
            {
                _prerelease = value ?? new object[0];
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

        public Version(int major, int minor, int patch, object[] prerelease)
            : this(major, minor, patch, prerelease, null)
        {
        }

        public Version(int major, int minor, int patch, object[] prerelease, string[] build)
            : this(major, minor, patch, prerelease, build, false, string.Empty)
        {
        }

        private Version(int major, int minor, int patch, object[] prerelease, string[] build, bool loose, string raw)
        {
            _major = major;
            _minor = minor;
            _patch = patch;
            _prerelease = prerelease ?? new object[0];
            _build = build ?? new string[0];
            _loose = loose;
            _raw = raw;
            Format();
        }

        protected Version()
            : this(0, 0, 0, null, null)
        {
        }

        public Version(string version, bool loose = false)
        {
            _loose = loose;
            var match = (loose ? Re.Loose : Re.Full).Match(version.Trim());

            if (!match.Success)
                throw new FormatException("Invalid Version: " + version);

            _raw = version;

            _major = int.Parse(match.Groups[1].Value);
            _minor = int.Parse(match.Groups[2].Value);
            _patch = int.Parse(match.Groups[3].Value);

            if (!match.Groups[4].Success)
                _prerelease = new object[] {};
            else
                _prerelease = match.Groups[4].Value.Split('.').Select(id =>
                    Re.Integer.IsMatch(id) ? int.Parse(id) : (object) id).ToArray();

            _build = match.Groups[5].Success
                ? match.Groups[5].Value.Split('.').ToArray()
                : new string[] {};

            Format();
        }

        public static Version Parse(string version, bool loose = false)
        {
            var regex = loose ? Re.Loose : Re.Full;
            return regex.IsMatch(version) ? new Version(version, loose) : null;
        }

        public static string Valid(string version, bool loose = false)
        {
            var semver = Parse(version, loose);
            return !ReferenceEquals(semver, null) ? semver._version : null;
        }

        public static string Clean(string version, bool loose = false)
        {
            var semver = Parse(version, loose);
            return !ReferenceEquals(semver, null) ? semver._version : null;
        }

        private void Format()
        {
            _version = _major + "." + _minor + "." + _patch;
            if (_prerelease.Length > 0)
                _version += "-" + string.Join(".", _prerelease);
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
            return new Version(_major, _minor, _patch, _prerelease, _build, _loose, _raw);
        }

        public int Compare(Version other)
        {
            var main = CompareMain(other);
            return main != 0 ? main : ComparePre(other);
        }

        public int Compare(string other, bool loose = false)
        {
            if (string.IsNullOrWhiteSpace(other))
                throw new ArgumentNullException("other");

            Version otherVersion;
            try
            {
                otherVersion = new Version(other, loose);
            }
            catch (FormatException exception)
            {
                throw new ArgumentException(exception.Message, "other", exception);
            }
            return Compare(otherVersion);
        }

        protected int CompareMain(Version other)
        {
            var major = CompareIdentifiers(_major, other._major);
            if (major != 0) return major;
            
            var minor = CompareIdentifiers(_minor, other._minor);
            if (minor != 0) return minor;

            return CompareIdentifiers(_patch, other._patch);
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

                var compare = CompareIdentifiers(this._prerelease[i], other._prerelease[i]);
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

        public Version Increment(IncrementType type)
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
                    _prerelease = new object[0];
                    break;
                case IncrementType.Prerelease:
                    if (_prerelease.Length == 0)
                        _prerelease = new object[] {0};
                    else
                    {
                        var incrementedSomething = false;
                        for (var i = _prerelease.Length - 1; i >= 0; i--)
                        {
                            if (_prerelease[i] is int)
                            {
                                _prerelease[i] = (int) _prerelease[i] + 1;
                                incrementedSomething = true;
                                break;
                            }
                        }
                        if (!incrementedSomething)
                            _prerelease = new List<object>(_prerelease) {0}.ToArray();
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid increment: " + type, "type");
            }
            this.Format();
            return this;
        }

        public bool Equals(Version other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Compare(other) == 0;
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

        #region SemVer Operator Overloads

        public static bool operator ==(Version v1, Version v2)
        {
            if (ReferenceEquals(v1, null))
                return ReferenceEquals(v2, null);
            return v1.Compare(v2) == 0;
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
            return v1.Compare(v2) < 0;
        }

        public static bool operator >(Version v1, Version v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.Compare(v2) > 0;
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
            return v1.Compare(v2) == 0;
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
            return v1.Compare(v2) < 0;
        }

        public static bool operator >(Version v1, string v2)
        {
            if (ReferenceEquals(v1, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            if (string.IsNullOrWhiteSpace(v2))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            return v1.Compare(v2) > 0;
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
            return v2.Compare(v1) == 0;
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
            return v2.Compare(v1) > 0;
        }

        public static bool operator >(string v1, Version v2)
        {
            if (string.IsNullOrWhiteSpace(v1))
                throw new ArgumentNullException("v2", "Cannot compare null versions");
            if (ReferenceEquals(v2, null))
                throw new ArgumentNullException("v1", "Cannot compare null versions");
            return v2.Compare(v1) < 0;
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
