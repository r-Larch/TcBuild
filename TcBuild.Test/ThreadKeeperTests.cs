using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TcBuild.Test;
using TcPluginBase;
using TcPluginBase.FileSystem;
using Xunit;
using Xunit.Abstractions;


// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement
namespace FsAzureStorage.Test {
    public class ThreadKeeperTests {
        private readonly ITestOutputHelper _output;
        [ThreadStatic] private static ThreadKeeper _exec;

        public ThreadKeeperTests(ITestOutputHelper output)
        {
            _output = output;
            Console.SetOut(new Converter(output, ""));
            Console.SetError(new Converter(output, ""));
        }

        private static ThreadKeeper GetCurrentThreadKeeper() => _exec ?? (_exec = new ThreadKeeper());
        //private static ThreadKeeper exec => _exec ?? (_exec = new ThreadKeeper());


        [Fact]
        public void Test_MultiThreading_ThreadStatic()
        {
            Parallel.For(0, 10, new ParallelOptions() {MaxDegreeOfParallelism = 100}, i => {
                Test_MultiThreading();
            });
        }


        [Fact]
        public void Test_MultiThreading()
        {
            // TODO overwrite: public virtual int OnTcPluginEvent(PluginEventArgs e)

            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            _output.WriteLine($"Main ThreadId {mainThreadId}");

            var exec = GetCurrentThreadKeeper();

            try {
                var result = exec.ExecAsync(async (token) => {
                    await Task.Delay(1, token);
                    _output.WriteLine($"   async ThreadId {Thread.CurrentThread.ManagedThreadId}");
                    Assert.NotEqual(mainThreadId, Thread.CurrentThread.ManagedThreadId);

                    Parallel.For(0, 100, new ParallelOptions {
                        MaxDegreeOfParallelism = 100
                    }, i => {
                        var tid = Thread.CurrentThread.ManagedThreadId;
                        _output.WriteLine($"{i}      ThreadId {tid}");
                        Assert.NotEqual(mainThreadId, tid);

                        exec.RunInMainThread(() => {
                            Assert.Equal(mainThreadId, Thread.CurrentThread.ManagedThreadId);
                            if (i == 20) {
                                exec.RunInMainThread(() => _output.WriteLine("hoi"));
                            }
                        });

                        if (i == 20) {
                            exec.RunInMainThread(() => _output.WriteLine("hoi outer"));
                        }

                        if (i == 30) {
                            try {
                                exec.RunInMainThread(() => {
                                    throw new Exception("Test");
                                });
                            }
                            catch (Exception e) {
                                _output.WriteLine($"caught: {e.Message}");
                            }

                            _output.WriteLine($"exec.Cancel();");
                            exec.Cancel();
                        }
                    });

                    return 12;
                });

                _output.WriteLine($"result: {result}");
            }
            catch (TaskCanceledException) {
                return;
            }
            catch (OperationCanceledException) {
                return;
            }
            catch (AggregateException e) {
                if (HasCanceledException(e)) {
                    _output.WriteLine($"{e.Message}");
                    return;
                }
                else {
                    throw;
                }
            }
        }

        private bool HasCanceledException(AggregateException e)
        {
            foreach (var exception in e.InnerExceptions) {
                switch (exception) {
                    case AggregateException agg:
                        return HasCanceledException(agg);
                    case TaskCanceledException _:
                    case OperationCanceledException _:
                        return true;
                }
            }

            return false;
        }
    }
}
