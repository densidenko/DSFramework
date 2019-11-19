using System;

namespace DSFramework.Functional
{
    public struct Maybe<T> : IEquatable<Maybe<T>> where T : class
    {
        private readonly T _value;

        public bool HasValue => _value != null;
        public T Value => _value ?? throw new InvalidOperationException();
        public static Maybe<T> None => new Maybe<T>();

        private Maybe(T value)
        {
            _value = value;
        }

        public override string ToString() => HasValue ? _value.ToString() : "NO VALUE";

        public static implicit operator Maybe<T>(T value) => new Maybe<T>(value);

        #region IEquatable Support

        /// <inheritdoc />
        /// <summary> Avoid boxing and Give type safety </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Maybe<T> other)
        {
            if (!HasValue && !other.HasValue)
            {
                return true;
            }

            if (!HasValue || !other.HasValue)
            {
                return false;
            }

            return _value.Equals(other.Value);
        }

        /// <summary> Avoid reflection </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is T typed)
            {
                obj = new Maybe<T>(typed);
            }

            if (!(obj is Maybe<T> other))
            {
                return false;
            }

            return Equals(other);
        }

        /// <summary>
        ///     Good practice when overriding Equals method.
        ///     If x.Equals(y) then we must have x.GetHashCode()==y.GetHashCode()
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => HasValue ? _value.GetHashCode() : default;

        public static bool operator ==(Maybe<T> maybe, T value) => maybe.HasValue && maybe.Value.Equals(value);

        public static bool operator !=(Maybe<T> maybe, T value) => !(maybe == value);

        public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

        public static bool operator !=(Maybe<T> left, Maybe<T> right) => !(left == right);

        #endregion
    }
}