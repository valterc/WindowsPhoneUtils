using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsPhoneUtils.Tasks
{
    public class Parallel
    {
        private Parallel()
        {
        }

        public static void Foreach(IEnumerable enumerable, Action<object> action)
        {
            List<Task> tasks = new List<Task>();

            foreach (object item in enumerable)
            {
                Task t = new Task(action, item);
                tasks.Add(t);
                t.Start();
            }

            Task.WaitAll(tasks.ToArray());
        }

    }
}
