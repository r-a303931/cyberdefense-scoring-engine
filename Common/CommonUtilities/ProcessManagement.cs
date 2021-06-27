using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Common.Exceptions;

namespace Common
{
    public static class ProcessManagement
    {
        public static Task<int> RunProcessAsync(Process process)
        {
            var tcs = new TaskCompletionSource<int>();

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();

            return tcs.Task;
        }

        public static void Forget(this Task task, IExceptionHandler exceptionHandler)
        {
            if (!task.IsCompleted || task.IsFaulted)
            {
                _ = ForgetAwaited(task, exceptionHandler);
            }

            async static Task ForgetAwaited(Task task, IExceptionHandler exceptionHandler)
            {
                try
                {
                    await task;
                }
                catch (Exception e)
                {
                    exceptionHandler.HandleException(e);
                }
            }
        }
    }
}
