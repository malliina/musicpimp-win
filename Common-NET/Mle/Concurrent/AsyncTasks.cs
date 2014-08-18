using Mle.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Mle.Concurrent {
    /// <summary>
    /// TODO: The API must not take Tasks but functions CancellationTokenSource => Task as parameter.
    /// Then the implementations can create the cts and manage cancellation transparently for the user.
    /// </summary>
    public class AsyncTasks {
        public static Task Noop() {
            return TaskEx.FromResult(0);
        }
        public static Task<T> FirstSuccessfulWithin<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks, TimeSpan timeout) {
            return Within<T>(FirstSuccessful<T>(tasks), timeout);
        }
        public static async Task<T> FirstSuccessful<T>(IEnumerable<Func<CancellationToken, Task<T>>> tasks) {
            // can I use the using keyword with async/await like below?
            using(var cts = new CancellationTokenSource()) {
                var taskList = tasks.Select(f => f(cts.Token)).ToList();
                while(taskList.Count > 0) {
                    Task<T> firstCompleted = await TaskEx.WhenAny<T>(taskList);
                    if(firstCompleted.IsFaulted) {
                        // remove mutates
                        taskList.Remove(firstCompleted);
                    } else {
                        cts.Cancel();
                        return await firstCompleted;
                    }
                }
                throw new NoResultsException("None of the submitted tasks completed successfully.");
            }
        }
        public static Task<T> Within<T>(Task<T> task, TimeSpan timeout) {
            return Within<T>(task, timeout, onTimeout: () => { });
        }
        public static Task Within2(Task task, TimeSpan timeout) {
            return Within(FromTask(task), timeout);
        }
        // hack
        private static async Task<int> FromTask(Task task) {
            await task;
            return 42;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns>the task if it completes within the supplied timeout, otherwise throws TimeoutException</returns>
        /// <exception cref="TimeoutException">if the timeout is hit before the supplied task completes</exception>
        public static async Task<T> Within<T>(Task<T> task, TimeSpan timeout, Action onTimeout) {
            if(await TaskEx.WhenAny(task, TaskEx.Delay(timeout)) == task) {
                return await task;
            } else {
                onTimeout();
                throw new TimeoutException("Unable to complete task within " + timeout + ".");
            }
        }
    }
}
