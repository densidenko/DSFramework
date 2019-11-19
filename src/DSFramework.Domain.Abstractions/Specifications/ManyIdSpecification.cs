using System;
using System.Linq;
using DSFramework.Domain.Abstractions.Entities;

namespace DSFramework.Domain.Abstractions.Specifications
{
    public class ManyIdSpecification<TAggregateRoot> : DomainSpecification<TAggregateRoot>, IManyIdSpecification<TAggregateRoot>
        where TAggregateRoot : IHasId
    {
        public string[] Ids { get; }

        public ManyIdSpecification(params string[] ids)
        {
            Ids = ids;
        }

        public override bool IsSatisfied(TAggregateRoot obj) => obj != null && Ids.Contains(obj.Id);
    }

    public class ManyIdSpecification<TKey, TAggregateRoot> : DomainSpecification<TAggregateRoot>, IManyIdSpecification<TKey, TAggregateRoot>
    {
        public TKey[] Ids { get; }
        public Func<TAggregateRoot, TKey> KeySelector { get; }

        public ManyIdSpecification(Func<TAggregateRoot, TKey> keySelector, params TKey[] ids)
        {
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            Ids = ids;
        }

        public override bool IsSatisfied(TAggregateRoot obj) => obj != null && Ids.Contains(KeySelector(obj));
    }
}