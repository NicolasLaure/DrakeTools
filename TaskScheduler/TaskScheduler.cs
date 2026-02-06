using DrakeToolbox.Services;
using DrakeTools.Flow;
using DrakeTools.TaskScheduler.DrakeTools.Tasks;

namespace DrakeTools.Tasks
{
    public sealed class TaskScheduler : IService, ITickable
    {
        public bool IsPersistent => false;

        private readonly List<ScheduleCall> scheduledTasks;

        public TaskScheduler()
        {
            scheduledTasks = new List<ScheduleCall>();
        }

        public void Schedule(Action action, float time)
        {
            scheduledTasks.Add(new ScheduleCall(action, time));
        }

        public void Tick(float deltaTime)
        {
            for (int i = scheduledTasks.Count; i >= 0; i--)
            {
                ScheduleCall task = scheduledTasks[i];
                task.remainingTime -= deltaTime;

                if (task.remainingTime <= 0)
                {
                    scheduledTasks.RemoveAt(i);
                    task.callback?.Invoke();
                }
            }
        }
    }
}