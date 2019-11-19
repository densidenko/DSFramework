using System.Collections.Generic;
using DSFramework.Functional;

namespace DSFramework.Security.Authorization
{
    public interface IPermissionService
    {
        Maybe<Permission> Find(string name);
        IReadOnlyList<Permission> ReadList();
    }
}