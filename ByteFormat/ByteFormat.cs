using System.Text;
using Newtonsoft.Json;

namespace DrakeToolbox.Formatting
{
    public static class ByteFormat
    {
        public static byte[] ToByteArray(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.Unicode.GetBytes(json);
        }

        public static object ToObject<ObjectType>(byte[] bytes)
        {
            string json = Encoding.Unicode.GetString(bytes);
            return JsonConvert.DeserializeObject<ObjectType>(json);
        }
    }
}