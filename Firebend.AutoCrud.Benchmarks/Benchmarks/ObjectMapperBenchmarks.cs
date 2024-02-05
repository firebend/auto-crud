using BenchmarkDotNet.Attributes;
using Firebend.AutoCrud.Core.Extensions;
using Firebend.AutoCrud.Core.Models.CustomFields;
using Firebend.JsonPatch.Extensions;

namespace Firebend.AutoCrud.Benchmarks.Benchmarks;

[RPlotExporter]
[MemoryDiagnoser]
public class ObjectMapperBenchmarks
{
    [Benchmark]
    public void Map_Using_Object_Mapper() => Map(true);

    [Benchmark]
    public void Map_Using_Object_Mapper_Without_Memoization() => Map(false);

    [Benchmark]
    public void Map_Using_Reflection()
    {
        var person = CreateSampleModel();

        for (var i = 0; i < 50; i++)
        {
            var copyTo = new SampleModel();
            person.CopyPropertiesToReflection(copyTo);
        }
    }

    [Benchmark]
    public void Map_Using_Clone()
    {
        var model = CreateSampleModel();

        for (var i = 0; i < 50; i++)
        {
            _ = model.Clone();
        }
    }

    [Benchmark]
    public void Map_Using_By_Hand()
    {
        var model = CreateSampleModel();

        for (var i = 0; i < 50; i++)
        {
            _ = new SampleModel
            {
                Id = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                DateOfBirth = model.DateOfBirth,
                CustomFields = model.CustomFields?.Select(x => new CustomFieldsEntity<Guid>
                {
                    Id = x.Id,
                    EntityId = x.EntityId,
                    Key = x.Key,
                    Value = x.Value
                }).ToList()
            };
        }
    }

    private static void Map(bool useMemorizer)
    {
        var person = CreateSampleModel();

        for (var i = 0; i < 50; i++)
        {
            var copyTo = new SampleModel();
            person.CopyPropertiesToObjectMapper(copyTo, useMemoizer: useMemorizer);
        }
    }

    private static SampleModel CreateSampleModel()
    {
        var person = new SampleModel
        {
            FirstName = "Fox",
            LastName = "Mulder",
            DateOfBirth = DateTimeOffset.UtcNow,
            CustomFields = new List<CustomFieldsEntity<Guid>>
            {
                new()
                {
                    Key = "Favorite Color",
                    Value = "Red"
                },
                new()
                {
                    Key = "Favorite Number",
                    Value = "42"
                }
            }
        };
        return person;
    }
}
