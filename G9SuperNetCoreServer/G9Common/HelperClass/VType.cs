using System;
using System.Collections.Generic;
using System.Reflection;

namespace G9Common.HelperClass
{
    /// <summary>
    ///     Helper class for derived types
    /// </summary>
    public static class VType
    {
        /// <summary>
        ///     Get derived types
        /// </summary>
        /// <param name="baseType">Specify base type</param>
        /// <param name="assemblies">Specify assemblies for search</param>
        /// <returns>List of types</returns>

        #region GetDerivedTypes

        public static List<Type> GetDerivedTypes(Type baseType, Assembly[] assemblies)
        {
            var derivedTypes = new List<Type>();
            foreach (var assembly in assemblies)
            {
                // Get all types from the given assembly
                var types = assembly.GetTypes();

                for (int i = 0, count = types.Length; i < count; i++)
                {
                    var type = types[i];
                    if (IsSubclassOf(type, baseType)) derivedTypes.Add(type);
                }
            }
            return derivedTypes;
        }

        #endregion

        /// <summary>
        ///     Specify type is sub class
        /// </summary>
        /// <param name="type">Specify type</param>
        /// <param name="baseType">Specify base ty[e</param>
        /// <returns></returns>

        #region IsSubclassOf

        public static bool IsSubclassOf(Type type, Type baseType)
        {
            if (type == null || baseType == null || type == baseType)
                return false;

            if (baseType.IsGenericType == false)
            {
                if (type.IsGenericType == false)
                    return type.IsSubclassOf(baseType);
            }
            else
            {
                baseType = baseType.GetGenericTypeDefinition();
            }

            type = type.BaseType;
            var objectType = typeof(object);

            while (type != objectType && type != null)
            {
                var curentType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (curentType == baseType)
                    return true;

                type = type.BaseType;
            }

            return false;
        }

        #endregion
    }
}