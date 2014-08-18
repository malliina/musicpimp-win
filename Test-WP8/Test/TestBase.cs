using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Mle.Test {
    public abstract class TestBase {
        public Task<T> ThrowsExceptionAsync<T>(Func<Task> action)
        where T : Exception {
            return ThrowsExceptionAsync<T>(action, string.Empty, null);
        }

        public Task<T> ThrowsExceptionAsync<T>(Func<Task> action, string message)
            where T : Exception {
            return ThrowsExceptionAsync<T>(action, message, null);
        }

        public async Task<T> ThrowsExceptionAsync<T>(Func<Task> action, string message, object[] parameters)
        where T : Exception {
            try {
                await action();
            } catch (Exception ex) {
                if (ex.GetType() == typeof(T)) {
                    return ex as T;
                }

                var objArray = new object[] { 
                "AssertExtensions.ThrowsExceptionAsync",
                string.Format(CultureInfo.CurrentCulture, FrameworkMessages.WrongExceptionThrown, message, typeof(T).Name, ex.GetType().Name, ex.Message, ex.StackTrace) 
            };

                throw new AssertFailedException(string.Format(CultureInfo.CurrentCulture, FrameworkMessages.AssertionFailed, objArray));
            }

            var objArray2 = new object[] { 
            "AssertExtensions.ThrowsExceptionAsync", 
            string.Format(CultureInfo.CurrentCulture, FrameworkMessages.NoExceptionThrown, message, typeof(T).Name)
        };

            throw new AssertFailedException(string.Format(CultureInfo.CurrentCulture, FrameworkMessages.AssertionFailed, objArray2));
        }
    }
}
