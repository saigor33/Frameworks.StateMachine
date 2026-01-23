using System;
using System.Linq;

namespace Frameworks.StateMachine.StateGraphVisualizer
{
    public static class TypesHelpers
    {
        public static Type[] GetInheritGenericTypes(Type[] types, Type genericType)
        {
            return types
               .Where(t => IsInheritGenericType(t, genericType))
               .ToArray();
        }

        public static Type[] GetInheritTypes(Type[] types, Type baseType)
        {
            return types
               .Where(type => type.BaseType != null && type.BaseType == baseType)
               .ToArray();
        }

        public static bool IsInheritGenericType(Type type, Type genericType)
        {
            return type.BaseType != null
                && type.BaseType.IsGenericType
                && type.BaseType.GetGenericTypeDefinition() == genericType;
        }
    }
}