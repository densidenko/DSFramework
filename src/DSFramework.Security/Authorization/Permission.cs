using System.Collections.Generic;
using System.Collections.Immutable;
using DSFramework.GuardToolkit;
using DSFramework.Hosting.MultiTenancy;

namespace DSFramework.Security.Authorization
{
    /// <summary>
    ///     Represents a permission.
    ///     A permission is used to restrict functionalities of the application from unauthorized users.
    /// </summary>
    public class Permission
    {
        private readonly List<Permission> _children;

        /// <summary>
        ///     List of child permissions. A child permission can be granted only if parent is granted.
        /// </summary>
        public IReadOnlyList<Permission> Children => _children.ToImmutableList();

        /// <summary>
        ///     Parent of this permission if one exists.
        ///     If set, this permission can be granted only if parent is granted.
        /// </summary>
        public Permission Parent { get; private set; }

        /// <summary>
        ///     Unique name of the permission.
        ///     This is the key name to grant permissions.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Display name of the permission.
        ///     This can be used to show permission to the user.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        ///     A brief description for this permission.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Which side can use this permission.
        /// </summary>
        public MultiTenancySides MultiTenancySides { get; set; }

        /// <summary>
        ///     Creates a new Permission.
        /// </summary>
        /// <param name="name">Unique name of the permission</param>
        /// <param name="displayName">Display name of the permission</param>
        /// <param name="description">A brief description for this permission</param>
        /// <param name="multiTenancySides">Which side can use this permission</param>
        public Permission(string name,
                          string displayName = null,
                          string description = null,
                          MultiTenancySides multiTenancySides = MultiTenancySides.Host | MultiTenancySides.Tenant)
        {
            Check.NotNull(name, nameof(name));

            Name = name;
            DisplayName = displayName;
            Description = description;
            MultiTenancySides = multiTenancySides;

            _children = new List<Permission>();
        }

        public override string ToString() => $"[Permission: {Name}]";

        /// <summary>
        ///     Adds a child permission.
        ///     A child permission can be granted only if parent is granted.
        /// </summary>
        /// <returns>Returns newly created child permission</returns>
        public Permission CreateChildPermission(string name,
                                                string displayName = null,
                                                string description = null,
                                                MultiTenancySides multiTenancySides = MultiTenancySides.Host | MultiTenancySides.Tenant)
        {
            var permission = new Permission(name, displayName, description, multiTenancySides) { Parent = this };
            _children.Add(permission);
            return permission;
        }

        public static Permission CreatePermission(string name,
                                                  string displayName = null,
                                                  string description = null,
                                                  MultiTenancySides multiTenancySides = MultiTenancySides.Host | MultiTenancySides.Tenant)
        {
            var permission = new Permission(name, displayName, description, multiTenancySides);

            return permission;
        }
    }
}