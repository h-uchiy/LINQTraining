using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace LINQTraining.Utils
{
    public static class LoggerExtensions
    {
        public static ITestOutputHelper StopWatch(this ITestOutputHelper logger, Expression<Action> action)
        {
            var compile = action.Compile();
            var sw = new Stopwatch();
            try
            {
                sw.Restart();
                compile.Invoke();
            }
            finally
            {
                sw.Stop();
                logger.WriteLine($"'{action.Body}' = {sw.ElapsedMilliseconds} ms");
            }
            return logger;
        }
    }
}