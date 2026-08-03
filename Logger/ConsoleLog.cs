using System;
using DrakeToolbox.Events;
using DrakeToolbox.Services;

namespace DrakeToolbox.Logging
{
    public class ConsoleLog
    {
        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public ConsoleLog()
        {
            EventBus.AddListener<OnLogNotificationEvent>(Log);
            EventBus.AddListener<OnLogErrorNotificationEvent>(LogError);
            EventBus.AddListener<OnLogWarningNotificationEvent>(LogWarning);
        }

        private void Log(in OnLogNotificationEvent text)
        {
            Console.WriteLine(text.message);
        }

        private void LogError(in OnLogErrorNotificationEvent text)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(text.message);
            Console.ForegroundColor = color;
        }

        private void LogWarning(in OnLogWarningNotificationEvent text)
        {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(text.message);
            Console.ForegroundColor = color;
        }
    }
}