using System;
using System.Linq;
using System.Reflection;

namespace DSFramework.Logging
{
    internal static class TypeExtensions
    {
        internal static string ReadableName(this Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }
            
            try
            {
                var result = ReadableTypeName(type);
                if (type.IsGenericType)
                {
                    result = $"{result}{GetGenericTypeName(type)}";
                }
                return result;
            }
            catch
            {
                return type.Name;
            }
        }

        internal static string GetGenericTypeName(this Type type)
        {
            var typeName = string.Empty;

            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        private static string ReadableTypeName(Type type)
        {
            var result = type.Name;
            if (type.GetTypeInfo().IsGenericType)
            {
                result = result.Remove(result.IndexOf('`'));
            }

            if (type.IsNested && !type.IsGenericParameter)
            {
                return $"{type.DeclaringType.ReadableName()}.{result}";
            }

            return result;
        }
    }
}