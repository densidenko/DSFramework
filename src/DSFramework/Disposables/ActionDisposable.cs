using System;

namespace DSFramework.Disposables
{
    public class ActionDisposable : IDisposable
    {
        private readonly Action _action;


        public ActionDisposable(Action action)
        {
            _action = action;
        }

        public void Dispose() => _action();
    }

}