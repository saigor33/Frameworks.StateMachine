using System;
using System.Linq;

namespace Frameworks.StateMachine.StateGraphVisualizer
{
    public static class TypesHelpers
    {
        public static Type[] GetInheritGenericTypes(Type[] types, Type type)
        {
            return types
               .Where(t => TypesHelpers.IsInheritGenericType(t, type))
               .ToArray();
        }

        public static Type[] GetInheritTypes(Type[] types, Type baseType)
        {
            return types
               .Where(type => type.BaseType != null && type.BaseType == baseType)
               .ToArray();
        }

        public static bool IsInheritGenericType(Type type, Type baseStateType)
        {
            return type.BaseType != null
                && type.BaseType.IsGenericType
                && type.BaseType.GetGenericTypeDefinition() == baseStateType;
        }
    }
}