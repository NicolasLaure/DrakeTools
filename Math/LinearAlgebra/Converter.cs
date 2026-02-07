using System;

namespace DrakeToolbox.Mathematics
{
    public static class Converter
    {
        public static float Rad2Deg => 180f / MathF.PI;
        public static float Deg2Rad => MathF.PI / 180f;
    }
}