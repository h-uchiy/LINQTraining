using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace LINQTraining.Utils
{
    public static class LoggerExtensions
    {
        public static void StopWatch(this ITestOutputHelper logger, Expression<Action> action)
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
        }
        
        public static TResult StopWatch<TResult>(this ITestOutputHelper logger, Expression<Func<TResult>> action)
        {
            var compile = action.Compile();
            var sw = new Stopwatch();
            try
            {
                sw.Restart();
                return compile.Invoke();
            }
            finally
            {
                sw.Stop();
                logger.WriteLine($"'{action.Body}' = {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}