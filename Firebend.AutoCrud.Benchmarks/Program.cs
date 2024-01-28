
using BenchmarkDotNet.Running;
using Firebend.AutoCrud.Benchmarks.Benchmarks;
using Firebend.AutoCrud.Core.Implementations.Concurrency;

var summary = BenchmarkRunner.Run<ObjectMapperBenchmarks>();
