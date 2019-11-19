﻿using System;

namespace DSFramework.Functional
{
    public static class MaybeExtensions
    {
        public static T GetValueOrDefault<T>(this Maybe<T> maybe, T defaultValue = default) where T : class
            => maybe.GetValueOrDefault(x => x, defaultValue);

        public static TK GetValueOrDefault<T, TK>(this Maybe<T> maybe, Func<T, TK> selector, TK defaultValue = default) where T : class
            => maybe.HasValue ? selector(maybe.Value) : defaultValue;

        public static Maybe<T> Where<T>(this Maybe<T> maybe, Func<T, bool> predicate) where T : class
        {
            if (!maybe.HasValue) return default(T);

            return predicate(maybe.Value) ? maybe : default(T);
        }

        public static Maybe<TK> Select<T, TK>(this Maybe<T> maybe, Func<T, TK> selector) where T : class where TK : class
            => !maybe.HasValue ? default : selector(maybe.Value);

        public static Maybe<TK> Select<T, TK>(this Maybe<T> maybe, Func<T, Maybe<TK>> selector) where T : class where TK : class
            => !maybe.HasValue ? default(TK) : selector(maybe.Value);

        public static void Execute<T>(this Maybe<T> maybe, Action<T> action) where T : class
        {
            if (!maybe.HasValue) return;

            action(maybe.Value);
        }
    }
}