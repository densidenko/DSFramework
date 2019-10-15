using System;
using System.Collections.Generic;
using System.Linq;

namespace DSFramework.Disposables
{
    public class CompositeDisposable : IDisposable
    {
        private readonly IReadOnlyList<IDisposable> _disposables;

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables?.ToList();
        }

        public bool IsEmpty => !_disposables.Any();

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}