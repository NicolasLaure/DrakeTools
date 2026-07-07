using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using DrakeToolbox.Formatting;

namespace DrakeToolbox.Reflection
{
    public static class ReflectionUtilities
    {
        public static bool IsCollection(this FieldInfo field)
        {
            return field.FieldType != typeof(string) && (field.FieldType.IsArray || typeof(ICollection).IsAssignableFrom(field.FieldType));
        }

        public static object[] GetConstructorObjects(ConstructorInfo constructorInfo, byte[] parameters)
        {
            ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
            object[] convertedParameters = new object[parameterInfos.Length];
            int offset = 0;
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                int byteCount = Marshal.SizeOf(parameterInfos[i].ParameterType);

                convertedParameters[i] = ByteFormat.ToObject(parameters, offset, parameterInfos[i].ParameterType);
                offset += byteCount;
            }

            return convertedParameters;
        }
    }
}