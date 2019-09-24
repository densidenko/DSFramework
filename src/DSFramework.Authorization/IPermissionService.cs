using System.Collections.Generic;
using DSFramework.Functional;

namespace DSFramework.Authorization
{
    public interface IPermissionService
    {
        Maybe<Permission> Find(string name);
        IReadOnlyList<Permission> ReadList();
    }
}