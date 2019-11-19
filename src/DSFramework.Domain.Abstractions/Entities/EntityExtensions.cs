﻿namespace DSFramework.Domain.Abstractions.Entities
{
    /// <summary>
    ///     Some useful extension methods for Entities.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        ///     Check if this Entity is null of marked as deleted.
        /// </summary>
        public static bool IsNullOrDeleted(this ISoftDelete entity) => entity == null || entity.IsDeleted;
    }
}