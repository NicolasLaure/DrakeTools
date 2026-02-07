using DrakeToolbox.Events;

namespace DrakeToolbox.Console
{
    public struct OnLogErrorNotificationEvent : IEvent
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