namespace DrakeTools.TaskScheduler.DrakeTools.Tasks
{
    public sealed class ScheduleCall
    {
        public readonly Action callback;
        public float remainingTime;

        public ScheduleCall(Action callback, float duration)
        {
            this.callback = callback;
            remainingTime = duration;
        }
    }
}