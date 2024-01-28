
using BenchmarkDotNet.Running;
using Firebend.AutoCrud.Benchmarks.Benchmarks;

var summary = BenchmarkRunner.Run<ObjectMapperBenchmarks>();
