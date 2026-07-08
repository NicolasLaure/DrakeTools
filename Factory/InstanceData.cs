using System;
using System.Collections.Generic;
using System.Text;

namespace DrakeToolbox.Factory
{
    public struct InstanceData
    {
        public uint instanceID;
        public uint originalClientID;
        public int blueprintIdLength;
        public string blueprintId;
        public int parametersByteCount;
        public byte[] constructorParameters;
        public int routeLength;
        public int[] route;
        public int instanceTypeLength;
        public string instanceType;

        public static bool operator ==(InstanceData left, InstanceData right)
        {
            return left.instanceID == right.instanceID && left.originalClientID == right.originalClientID &&
                   left.blueprintIdLength == right.blueprintIdLength && left.blueprintId == right.blueprintId &&
                   left.parametersByteCount == right.parametersByteCount && left.constructorParameters == right.constructorParameters &&
                   left.routeLength == right.routeLength && left.route == right.route &&
                   left.instanceTypeLength == right.instanceTypeLength && left.instanceType == right.instanceType;
        }

        public static bool operator !=(InstanceData left, InstanceData right)
        {
            return !(left == right);
        }

        public byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(instanceID));
            data.AddRange(BitConverter.GetBytes(originalClientID));
            data.AddRange(BitConverter.GetBytes(Encoding.Unicode.GetByteCount(blueprintId)));
            data.AddRange(Encoding.Unicode.GetBytes(blueprintId));

            data.AddRange(BitConverter.GetBytes(constructorParameters.Length));
            data.AddRange(constructorParameters);

            data.AddRange(BitConverter.GetBytes(routeLength));
            for (int i = 0; i < routeLength; i++)
                data.AddRange(BitConverter.GetBytes(route[i]));

            data.AddRange(BitConverter.GetBytes(Encoding.Unicode.GetByteCount(instanceType)));
            data.AddRange(Encoding.Unicode.GetBytes(instanceType));
            return data.ToArray();
        }

        public static InstanceData Deserialize(byte[] data)
        {
            InstanceData instanceData = new InstanceData();

            int offset = 0;
            instanceData.instanceID = BitConverter.ToUInt32(data);
            offset += sizeof(int);
            instanceData.originalClientID = BitConverter.ToUInt32(data, offset);
            offset += sizeof(int);
            instanceData.blueprintIdLength = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            instanceData.blueprintId = Encoding.Unicode.GetString(data, offset, instanceData.blueprintIdLength);
            offset += instanceData.blueprintIdLength;

            instanceData.parametersByteCount = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            instanceData.constructorParameters = data[offset..instanceData.parametersByteCount];
            offset += instanceData.parametersByteCount;

            instanceData.routeLength = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            List<int> routeList = new List<int>();
            for (int i = 0; i < instanceData.routeLength; i++)
            {
                routeList.Add(BitConverter.ToInt32(data, offset));
                offset += sizeof(int);
            }

            instanceData.route = routeList.ToArray();

            instanceData.instanceTypeLength = BitConverter.ToInt32(data, offset);
            offset += sizeof(int);
            instanceData.instanceType = Encoding.Unicode.GetString(data, offset, instanceData.instanceTypeLength);

            return instanceData;
        }

        public static List<InstanceData> DeserealizeMultiple(byte[] data, int count)
        {
            List<InstanceData> instanceDatas = new List<InstanceData>();
            int offset = 0;
            for (int i = 0; i < count; i++)
            {
                InstanceData instanceData;
                instanceData.instanceID = BitConverter.ToUInt32(data);
                offset += sizeof(int);
                instanceData.originalClientID = BitConverter.ToUInt32(data, offset);
                offset += sizeof(int);
                instanceData.blueprintIdLength = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                instanceData.blueprintId = Encoding.Unicode.GetString(data, offset, instanceData.blueprintIdLength);
                offset += instanceData.blueprintIdLength;

                instanceData.parametersByteCount = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                instanceData.constructorParameters = data[offset..instanceData.parametersByteCount];
                offset += instanceData.parametersByteCount;

                instanceData.routeLength = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                List<int> routeList = new List<int>();
                for (int j = 0; j < instanceData.routeLength; j++)
                {
                    routeList.Add(BitConverter.ToInt32(data, offset));
                    offset += sizeof(int);
                }

                instanceData.route = routeList.ToArray();

                instanceData.instanceTypeLength = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                instanceData.instanceType = new string(Encoding.Unicode.GetChars(data, offset, instanceData.instanceTypeLength));

                instanceDatas.Add(instanceData);
            }

            return instanceDatas;
        }
    }
}