using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LINQTraining.Utils
{
    public static class LoggerExtensions
    {
        public static void StopWatch(this ITestOutputHelper logger, Expression<Action> action)
        {
            var compile = action.Compile();
            StopWatchCore(logger, action.Body, () =>
            {
                compile();
                return 0;
            });
        }

        public static TResult StopWatch<TResult>(this ITestOutputHelper logger, Expression<Func<TResult>> action)
        {
            var compile = action.Compile();
            return StopWatchCore(logger, action.Body, compile);
        }

        public static Task StopWatchAsync(this ITestOutputHelper logger, Expression<Func<Task>> action)
        {
            var compile = action.Compile();
            return StopWatchAsyncCore(logger, action.Body, () => compile().ContinueWith(antecedent => 0));
        }

        public static Task<TResult> StopWatchAsync<TResult>(this ITestOutputHelper logger,
            Expression<Func<Task<TResult>>> action)
        {
            var compile = action.Compile();
            return StopWatchAsyncCore(logger, action.Body, compile);
        }

        private static TResult StopWatchCore<TResult>(ITestOutputHelper logger, Expression body, Func<TResult> action)
        {
            var sw = new Stopwatch();
            try
            {
                sw.Restart();
                return action.Invoke();
            }
            finally
            {
                sw.Stop();
                logger.WriteLine($"'{body}' = {sw.ElapsedMilliseconds} ms");
            }
        }

        private static async Task<TResult> StopWatchAsyncCore<TResult>(ITestOutputHelper logger, Expression body, Func<Task<TResult>> action)
        {
            var sw = new Stopwatch();
            try
            {
                sw.Restart();
                return await action.Invoke();
            }
            finally
            {
                sw.Stop();
                logger.WriteLine($"'{body}' = {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}