namespace MathUtils
{
    public static class Random
    {
        private static System.Random _ran;
        public static System.Random ran
        {
            get
            {
                if (_ran == null)
                {
                    int seed = (int)System.DateTime.Now.Ticks;
                    _ran = new System.Random(seed);
                }
                return _ran;
            }
        }

        public static int RandomInt(int maxValue)
        {
            return ran.Next(maxValue);
        }
        public static int RandomInt(int minValue, int maxValue)
        {
            return ran.Next(minValue, maxValue);
        }
        public static double RandomDouble()
        {
            return ran.NextDouble();
        }
        public static double RandomDouble(double minValue, double maxValue)
        {
            return minValue + (maxValue - minValue) * ran.NextDouble();
        }
        public static bool RandomBool()
        {
            return RandomInt(2) == 0;
        }
        public static void RandomSeed(int seed)
        {
            _ran = new System.Random(seed);
        }
        public static void RandomSeed()
        {
            _ran = null;
        }

        public static bool TossCoin(int probability)
        {
            return ran.Next(100) < probability;
        }
    }
}