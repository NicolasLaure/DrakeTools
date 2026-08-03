using DrakeToolbox.Events;

namespace DrakeToolbox.Logging
{
    public struct OnLogWarningNotificationEvent : IEvent
    {
        public string message;

        public void Assign(params object[] parameters)
        {
            message = (string)parameters[0];
        }

        public void Reset()
        {
            message = "";
        }
    }
}