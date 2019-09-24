using System;
using System.Text;

namespace DSFramework.Caching
{
    public class CacheTag : IEquatable<CacheTag>
    {
        readonly string _tag;

        public CacheTag(string name, params object[] values)
        {
            var txt = new StringBuilder(name);

            if (values != null)
            {
                txt.Append('[');
                foreach (var value in values)
                {
                    var o = value?.ToString().Trim();
                    txt.Append(o);
                    txt.Append(',');
                }
                txt.Append(']');
            }

            _tag = txt.ToString();
        }

        public bool Equals(CacheTag other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_tag, other._tag);
        }

        public override string ToString()
        {
            return _tag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((CacheTag)obj);
        }

        public override int GetHashCode()
        {
            return _tag?.GetHashCode() ?? 0;
        }

        public static bool operator ==(CacheTag left, CacheTag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CacheTag left, CacheTag right)
        {
            return !Equals(left, right);
        }
    }
}