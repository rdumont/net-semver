using System;

namespace SemanticVersioning
{
    public class VersionIdentifier : IEquatable<VersionIdentifier>, IComparable<VersionIdentifier>
    {
        private readonly string _stringValue;
        private readonly int? _integerValue;

        public int? IntegerValue
        {
            get { return _integerValue; }
        }

        public string StringValue
        {
            get { return _stringValue; }
        }

        public VersionIdentifier(int integerValue)
        {
            _integerValue = integerValue;
        }

        public VersionIdentifier(string stringValue)
        {
            _stringValue = stringValue;
        }

        public static VersionIdentifier Parse(string source)
        {
            int integerValue;
            return int.TryParse(source, out integerValue)
                ? new VersionIdentifier(integerValue)
                : new VersionIdentifier(source.Trim());
        }

        public static implicit operator VersionIdentifier(int integerValue)
        {
            return new VersionIdentifier(integerValue);
        }

        public static implicit operator VersionIdentifier(string stringValue)
        {
            return new VersionIdentifier(stringValue);
        }

        public bool Equals(VersionIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_stringValue, other._stringValue) && _integerValue == other._integerValue;
        }

        public int CompareTo(VersionIdentifier other)
        {
            var anum = _integerValue;
            var bnum = other._integerValue;
            var astr = _stringValue;
            var bstr = other._stringValue;

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VersionIdentifier) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_stringValue != null ? _stringValue.GetHashCode() : 0)*397) ^ _integerValue.GetHashCode();
            }
        }

        public override string ToString()
        {
            return _integerValue != null
                ? _integerValue.ToString()
                : _stringValue;
        }
    }
}
