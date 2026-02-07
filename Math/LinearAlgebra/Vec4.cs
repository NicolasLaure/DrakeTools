using System;

namespace DrakeToolbox.Mathematics
{
    public class Vec4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vec4()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }

        public Vec4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vec4(float x, float y)
        {
            this.x = x;
            this.y = y;
            this.z = 0.0f;
            this.w = 0.0f;
        }

        public Vec4(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = 0.0f;
        }

        public float SqrMagnitude => x * x + y * y + z * z + w * w;
        public float magnitude => MathF.Sqrt(SqrMagnitude);

        public static Vec4 zero => new(0.0f, 0.0f, 0.0f, 0.0f);
        public static Vec4 one => new(1.0f, 1.0f, 1.0f, 1.0f);

        public static Vec4 operator *(Vec4 v4, float scalar)
        {
            return new Vec4(v4.x * scalar, v4.y * scalar, v4.z * scalar, v4.w * scalar);
        }

        public static Vec4 operator *(float scalar, Vec4 v4)
        {
            return new Vec4(v4.x * scalar, v4.y * scalar, v4.z * scalar, v4.w * scalar);
        }

        public static Vec4 operator /(Vec4 v4, float scalar)
        {
            return new Vec4(v4.x / scalar, v4.y / scalar, v4.z / scalar, v4.w / scalar);
        }
    }
}