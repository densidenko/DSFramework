using System;
using System.Linq;
using DS.Domain.Abstractions.Aggregates;

namespace DS.Domain.Abstractions.Specifications
{
    public class ManyIdSpecification<TAggregateRoot> : DomainSpecification<TAggregateRoot>, IManyIdSpecification<TAggregateRoot> where TAggregateRoot : IHasId
    {
        public ManyIdSpecification(params string[] ids)
        {
            Ids = ids;
        }

        public string[] Ids { get; }

        public override bool IsSatisfied(TAggregateRoot obj)
        {
            return obj != null && Ids.Contains(obj.Id);
        }
    }

    public class ManyIdSpecification<TKey, TAggregateRoot> : DomainSpecification<TAggregateRoot>, IManyIdSpecification<TKey, TAggregateRoot>
    {
        public Func<TAggregateRoot, TKey> KeySelector { get; }

        public ManyIdSpecification(Func<TAggregateRoot, TKey> keySelector, params TKey[] ids)
        {
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            Ids = ids;
        }

        public TKey[] Ids { get; }

        public override bool IsSatisfied(TAggregateRoot obj)
        {
            return obj != null && Ids.Contains(KeySelector(obj));
        }
    }
}