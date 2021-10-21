using System;
using System.Collections.Generic;

namespace com.cratesmith.widgets
{
    public enum OptionalUsage
    {
        Never=0,
        AsDefault=1,
        Always=2,
    }

    public struct Optional<T> : IEquatable<Optional<T>>
    {
        public T value;
        public OptionalUsage usage;
        
        public static implicit operator Optional<T>(in T _value) => new Optional<T>() {value = _value, usage = OptionalUsage.Always};
        public static implicit operator T(in Optional<T> _this) => _this.value;
        
        public T GetValue(T _valueIfUnused, bool _useAsDefault)
        {
            switch (usage)
            {
                case OptionalUsage.Always:
                    return value;
                    
                case OptionalUsage.AsDefault:
                    return _useAsDefault ? value:_valueIfUnused; 
                
                default:
                    return _valueIfUnused;
            }
        }

        public Optional<T> AsDefault()
        {
            return new Optional<T>()
            {
                value = this.value,
                usage = this.usage!=OptionalUsage.Never 
                    ? OptionalUsage.AsDefault
                    : OptionalUsage.Never
            };
        }

        public bool Equals(Optional<T> other)
        {
            return EqualityComparer<T>.Default.Equals(value, other.value) && usage == other.usage;
        }

        public override bool Equals(object obj)
        {
            return obj is Optional<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(value) * 397) ^ usage.GetHashCode();
            }
        }

        public static bool operator ==(Optional<T> left, Optional<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Optional<T> left, Optional<T> right)
        {
            return !left.Equals(right);
        }
    }

    public static class OptionalExtensions
    {
        public static Optional<T> AsDefault<T>(this T _this)
        {
            return new Optional<T>
            {
                value = _this,
                usage = OptionalUsage.AsDefault
            };
        }
    }
}