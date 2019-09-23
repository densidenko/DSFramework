using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DSFramework.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> _aliasedTypes = new Dictionary<Type, string>
        {
            { typeof(byte), "int" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(decimal), "decimal" }
        };

        public static string ReadableName(this Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            var typeInfo = type.GetTypeInfo();

            try
            {
                if (_aliasedTypes.TryGetValue(type, out var alias))
                {
                    return alias;
                }

                var result = ReadableTypeName(type);
                if (typeInfo.IsGenericType)
                {
                    result = $"{result}{ReadableNameForGeneric(typeInfo.GenericTypeArguments)}";
                }
                return result;
            }
            catch
            {
                return type.Name;
            }
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

        private static string ReadableNameForGeneric(Type[] types)
        {
            return $"<{string.Join(",", types.Select(ReadableName))}>";
        }
    }
}