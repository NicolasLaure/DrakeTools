using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace DrakeToolbox.Formatting
{
    public static class ByteFormat
    {
        public static byte[] ToByteArray(object obj)
        {
            Type type = obj.GetType();
            if (!type.IsPrimitive)
            {
                string json = JsonConvert.SerializeObject(obj);
                return Encoding.Unicode.GetBytes(json);
            }

            List<byte> bytes = new List<byte>();

            switch (obj)
            {
                case sbyte sByteObj:
                    bytes.Add(Convert.ToByte(sByteObj));
                    break;
                case byte byteObj:
                    bytes.Add(byteObj);
                    break;
                case short shortObj:
                    bytes.AddRange(BitConverter.GetBytes(shortObj));
                    break;
                case ushort ushortObj:
                    bytes.AddRange(BitConverter.GetBytes(ushortObj));
                    break;
                case int intObj:
                    bytes.AddRange(BitConverter.GetBytes(intObj));
                    break;
                case uint uintObj:
                    bytes.AddRange(BitConverter.GetBytes(uintObj));
                    break;
                case long longObj:
                    bytes.AddRange(BitConverter.GetBytes(longObj));
                    break;
                case ulong ulongObj:
                    bytes.AddRange(BitConverter.GetBytes(ulongObj));
                    break;
                case float floatObj:
                    bytes.AddRange(BitConverter.GetBytes(floatObj));
                    break;
                case double doubleObj:
                    bytes.AddRange(BitConverter.GetBytes(doubleObj));
                    break;
                case decimal decimalObj:
                    int[] bits = decimal.GetBits(decimalObj);
                    for (int i = 0; i < bits.Length; i++)
                        bytes.AddRange(BitConverter.GetBytes(bits[i]));
                    break;
                case bool boolObj:
                    bytes.AddRange(BitConverter.GetBytes(boolObj));
                    break;
                case char charObj:
                    bytes.AddRange(BitConverter.GetBytes(charObj));
                    break;
                case string stringObj:
                    bytes.AddRange(Encoding.Unicode.GetBytes(stringObj));
                    break;
            }

            return bytes.ToArray();
        }

        public static byte[] ToByteArray(object[] objects)
        {
            List<byte> bytes = new List<byte>();
            foreach (object obj in objects)
                bytes.AddRange(ToByteArray(obj));

            return bytes.ToArray();
        }

        public static object ToObject<ObjectType>(byte[] bytes, int offset)
        {
            if (!typeof(ObjectType).IsPrimitive)
            {
                string json = Encoding.Unicode.GetString(bytes);
                return JsonConvert.DeserializeObject<ObjectType>(json);
            }

            return GetPrimitive(bytes, offset, typeof(ObjectType));
        }

        public static object ToObject(byte[] bytes, int offset, Type type)
        {
            if (!type.IsPrimitive)
            {
                string json = Encoding.Unicode.GetString(bytes);
                return JsonConvert.DeserializeObject(json, type);
            }

            return GetPrimitive(bytes, offset, type);
        }

        public static object[] ToObjectArray(byte[] bytes, int offset, Type[] types)
        {
            object[] convertedParameters = new object[types.Length];
            for (int i = 0; i < types.Length; i++)
            {
                int byteCount = Marshal.SizeOf(types[i]);

                convertedParameters[i] = ByteFormat.ToObject(bytes, offset, types[i]);
                offset += byteCount;
            }

            return convertedParameters;
        }

        private static object GetPrimitive(byte[] bytes, int offset, Type type)
        {
            object obj = Activator.CreateInstance(type);
            switch (obj)
            {
                case sbyte:
                    return Convert.ToSByte(bytes[offset]);
                case byte:
                    return bytes[offset];
                case short:
                    return BitConverter.ToInt16(bytes, offset);
                case ushort:
                    return BitConverter.ToUInt16(bytes, offset);
                case int:
                    return BitConverter.ToInt32(bytes, offset);
                case uint:
                    return BitConverter.ToUInt32(bytes, offset);
                case long:
                    return BitConverter.ToInt64(bytes, offset);
                case ulong:
                    return BitConverter.ToUInt64(bytes, offset);
                case float:
                    return BitConverter.ToSingle(bytes, offset);
                case double:
                    return BitConverter.ToDouble(bytes, offset);
                case decimal:
                    List<int> ints = new List<int>();
                    for (int i = 0; i < 4; i++)
                    {
                        ints.Add(BitConverter.ToInt32(bytes, offset));
                        offset += sizeof(int);
                    }

                    return new decimal(ints.ToArray());
                case bool:
                    return BitConverter.ToBoolean(bytes, offset);
                case char:
                    return BitConverter.ToChar(bytes, offset);
                case string:
                    char[] text = Encoding.Unicode.GetChars(bytes, offset, bytes.Length - offset);
                    return new string(text);
            }

            return null;
        }
    }
}