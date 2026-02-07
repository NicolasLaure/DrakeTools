using DrakeToolbox.Events;
using DrakeToolbox.Services;

namespace DrakeToolbox.Console
{
    public class Logger : IService
    {
        public bool IsPersistent => true;

        private EventBus EventBus => ServiceProvider.Instance.GetService<EventBus>();

        public void Log(string txt)
        {
            EventBus.Raise<OnLogNotificationEvent>(txt);
        }

        public void LogError(string txt)
        {
            EventBus.Raise<OnLogErrorNotificationEvent>(txt);
        }

        public void LogWarning(string txt)
        {
            EventBus.Raise<OnLogWarningNotificationEvent>(txt);
        }
    }
}