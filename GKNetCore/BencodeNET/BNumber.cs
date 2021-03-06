﻿using System;

namespace BencodeNET
{
    /// <summary>
    /// Represents a bencoded number (integer).
    /// </summary>
    /// <remarks>
    /// The underlying value is a <see cref="long"/>.
    /// </remarks>
    public sealed class BNumber : BObject<long>, IComparable<BNumber>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly long fValue;

        /// <summary>
        /// The string-length of long.MaxValue. Longer strings cannot be parsed.
        /// </summary>
        internal const int MaxDigits = 19;

        /// <summary>
        /// The underlying value.
        /// </summary>
        public override long Value
        {
            get { return fValue; }
        }

        /// <summary>
        /// Create a <see cref="BNumber"/> from a <see cref="long"/>.
        /// </summary>
        public BNumber(long value)
        {
            fValue = value;
        }

        /// <summary>
        /// Create a <see cref="BNumber"/> from a <see cref="DateTime"/>.
        /// </summary>
        /// <remarks>
        /// Bencode dates are stored in unix format (seconds since epoch).
        /// </remarks>
        public BNumber(DateTime? datetime)
        {
            fValue = (datetime.HasValue) ? datetime.Value.Subtract(Epoch).Ticks / TimeSpan.TicksPerSecond : 0;
        }

        protected override void EncodeObject(BencodeStream stream)
        {
            stream.Write('i');
            stream.Write(fValue);
            stream.Write('e');
        }

        public static implicit operator int(BNumber bint)
        {
            return (int)bint.fValue;
        }

        public static implicit operator long(BNumber bint)
        {
            return bint.fValue;
        }

        public static implicit operator bool(BNumber bint)
        {
            return bint.fValue > 0;
        }

        public static implicit operator DateTime? (BNumber number)
        {
            if (number == null) return null;

            if (number.fValue > int.MaxValue) {
                try {
                    return Epoch.AddMilliseconds(number);
                } catch (ArgumentOutOfRangeException) {
                    return Epoch;
                }
            }

            return Epoch.AddSeconds(number);
        }

        public static implicit operator BNumber(int value)
        {
            return new BNumber(value);
        }

        public static implicit operator BNumber(long value)
        {
            return new BNumber(value);
        }

        public static implicit operator BNumber(bool value)
        {
            return new BNumber(value ? 1 : 0);
        }

        public static implicit operator BNumber(DateTime? datetime)
        {
            return new BNumber(datetime);
        }

        public static bool operator ==(BNumber bnumber, BNumber other)
        {
            if (ReferenceEquals(bnumber, null) && ReferenceEquals(other, null)) return true;
            if (ReferenceEquals(bnumber, null) || ReferenceEquals(other, null)) return false;
            return bnumber.fValue == other.fValue;
        }

        public static bool operator !=(BNumber bnumber, BNumber other)
        {
            return !(bnumber == other);
        }

        public override bool Equals(object obj)
        {
            var bnumber = obj as BNumber;
            return fValue == (bnumber != null ? bnumber.fValue : 0);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            return fValue.GetHashCode();
        }

        public int CompareTo(BNumber other)
        {
            if (other == null)
                return 1;

            return fValue.CompareTo(other.fValue);
        }

        public override string ToString()
        {
            return fValue.ToString();
        }

        public string ToString(string format)
        {
            return fValue.ToString(format);
        }

        public string ToString(IFormatProvider formatProvider)
        {
            return fValue.ToString(formatProvider);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return fValue.ToString(format, formatProvider);
        }
    }
}
