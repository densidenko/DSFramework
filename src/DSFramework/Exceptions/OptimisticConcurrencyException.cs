using System;

namespace DSFramework.Exceptions
{
    public class OptimisticConcurrencyException : Exception
    {
        public OptimisticConcurrencyException()
            : base("OPTIMISTIC_CONCURRENCY")
        { }
    }
}