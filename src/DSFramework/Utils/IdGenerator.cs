﻿using System;

namespace DSFramework.Utils
{
    /// <summary>
    ///     The IdGenerator instance, used to generate Ids of different types.
    /// </summary>
    public static class IdGenerator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        ///     Generates a random value of a given type.
        /// </summary>
        /// <typeparam name="TKey">The type of the value to generate.</typeparam>
        /// <returns>A value of type TKey.</returns>
        public static TKey GetId<TKey>()
        {
            var idTypeName = typeof(TKey).Name;
            switch (idTypeName)
            {
                case "Guid":
                    return (TKey)(object)Guid.NewGuid();
                case "Int16":
                    return (TKey)(object)_random.Next(minValue: 1, short.MaxValue);
                case "Int32":
                    return (TKey)(object)_random.Next(minValue: 1, int.MaxValue);
                case "Int64":
                    return (TKey)(object)_random.NextLong(min: 1, long.MaxValue);
                case "String":
                    return (TKey)(object)Guid.NewGuid().ToString("N");
            }

            throw new ArgumentException($"{idTypeName} is not a supported Id type");
        }
    }
}