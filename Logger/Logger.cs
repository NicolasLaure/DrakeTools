namespace Utils
{
    public static class Logger
    {
        public static Action<string> onLog;
        public static Action<string> onLogError;
        public static Action<string> onLogWarning;

        public static void Log(string txt)
        {
            onLog?.Invoke(txt);
        }

        public static void LogError(string txt)
        {
            onLogError?.Invoke(txt);
        }

        public static void LogWarning(string txt)
        {
            onLogWarning?.Invoke(txt);
        }
    }
}