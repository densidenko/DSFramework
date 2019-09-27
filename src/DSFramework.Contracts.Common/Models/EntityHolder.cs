using System;
using System.Collections.Generic;
using DSFramework.Domain.Abstractions.Aggregates;

namespace DSFramework.Contracts.Common.Models
{
    public class EntityHolder<T> : EntityHolder<string, T>, IEntityHolder, IHasVersion, IHasId
    {
    }

    public class EntityHolder<TIdentity, TData> : IEntityHolder<TIdentity>, IEquatable<EntityHolder<TIdentity, TData>>
    {
        public TIdentity Id { get; set; }
        public int DataVersion { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string Application { get; set; }

        public TData Data { get; set; }

        public bool Equals(EntityHolder<TIdentity, TData> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return EqualityComparer<TIdentity>.Default.Equals(Id, other.Id) && DataVersion == other.DataVersion && CreatedDate.Equals(other.CreatedDate) && ModifiedDate.Equals(other.ModifiedDate) && string.Equals(CreatedBy, other.CreatedBy) && string.Equals(ModifiedBy, other.ModifiedBy) && string.Equals(Application, other.Application) && EqualityComparer<TData>.Default.Equals(Data, other.Data);
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((EntityHolder<TIdentity, TData>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<TIdentity>.Default.GetHashCode(Id);
                hashCode = (hashCode * 397) ^ DataVersion;
                hashCode = (hashCode * 397) ^ CreatedDate.GetHashCode();
                hashCode = (hashCode * 397) ^ ModifiedDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (CreatedBy != null ? CreatedBy.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ModifiedBy != null ? ModifiedBy.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Application != null ? Application.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ EqualityComparer<TData>.Default.GetHashCode(Data);
                return hashCode;
            }
        }

        public static bool operator ==(EntityHolder<TIdentity, TData> left, EntityHolder<TIdentity, TData> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityHolder<TIdentity, TData> left, EntityHolder<TIdentity, TData> right)
        {
            return !Equals(left, right);
        }
    }
}