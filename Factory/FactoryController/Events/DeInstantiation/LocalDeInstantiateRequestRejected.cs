using DrakeToolbox.Events;

namespace DrakeToolbox.Factory
{
    public struct LocalDeInstantiateRequestRejected : IEvent
    {
        public string message;

        public void Assign(params object[] parameters)
        {
            message = (string)parameters[0];
        }

        public void Reset()
        {
            message = string.Empty;
        }
    }
}