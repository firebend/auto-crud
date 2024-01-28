using BenchmarkDotNet.Attributes;
using Firebend.AutoCrud.Core.Implementations.Concurrency;

namespace Firebend.AutoCrud.Benchmarks.Benchmarks;

[RPlotExporter]
[MemoryDiagnoser]
public class MemoizerBenchmarks
{
    [Benchmark]
    public async Task Memoize_Standard()
    {
        var memoizer = new Memoizer();
        for(var i = 0; i < 10; i++)
        {
            var result = await memoizer.MemoizeAsync("standard", async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                return true;
            }, default);

            if (result == false)
            {
                throw new Exception("Should not be false");
            }
        }
    }

    [Benchmark]
    public async Task Memoize_Memcache()
    {
        for (var i = 0; i < 10; i++)
        {
            var result = await MemoryCacheMemoizer.Instance.MemoizeAsync("standard", async () =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                return true;
            }, default);

            if (result == false)
            {
                throw new Exception("Should not be false");
            }
        }
    }
}
