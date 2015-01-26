using System;
using System.Threading.Tasks;

namespace CommonTestingFramework
{
    public static class TaskExtensionMethods
    {
        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)))
                await task;
            else
                throw new TimeoutException("Task didn't complete in the allocated time");
        }
    }
}
