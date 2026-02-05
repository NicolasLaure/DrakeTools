namespace DrakeTools.Math
{
    public static class RandomGenerator
    {
        private static Random random = new Random();

        public static float Range(float min, float max)
        {
            return min + (max - min) * (float)random.NextDouble();
        }

        public static int Range(int min, int max)
        {
            return random.Next(min, max);
        }
    }
}