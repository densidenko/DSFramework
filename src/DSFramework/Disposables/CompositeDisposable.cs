using System;
using System.Collections.Generic;
using System.Linq;

namespace DSFramework.Disposables
{
    public class CompositeDisposable : IDisposable
    {
        private readonly IReadOnlyList<IDisposable> _disposables;

        public bool IsEmpty => !_disposables.Any();

        public CompositeDisposable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables?.ToList();
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}