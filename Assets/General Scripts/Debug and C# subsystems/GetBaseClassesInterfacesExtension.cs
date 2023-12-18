using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GetBaseClassesInterfacesExtensions
{
    public static IEnumerable<Type> GetBaseClassesAndInterfaces(this Type type, bool includeSelf = false)
    {
        //https://stackoverflow.com/questions/1823655/given-a-c-sharp-type-get-its-base-classes-and-implemented-interfaces
        List<Type> allTypes = new List<Type>();

        if (includeSelf) allTypes.Add(type);

        if (type.BaseType == typeof(object))
        {
            allTypes.AddRange(type.GetInterfaces());
        }
        else
        {
            allTypes.AddRange(
                    Enumerable
                    .Repeat(type.BaseType, 1)
                    .Concat(type.GetInterfaces())
                    .Concat(type.BaseType.GetBaseClassesAndInterfaces())
                    .Distinct());
        }

        return allTypes;
    }
}
