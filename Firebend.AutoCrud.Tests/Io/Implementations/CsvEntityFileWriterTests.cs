using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Firebend.AutoCrud.Io.Implementations;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Firebend.AutoCrud.Tests.Web.Implementations.Swagger;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Io.Implementations;

[TestFixture]
public class CsvEntityFileWriterTests
{
    public class Parent
    {
        public Guid ParentId { get; set; }
        public string Name { get; set; }
        public List<Child> Children { get; set; }
    }

    public class Child
    {
        public Guid ChildId { get; set; }
        public string Name { get; set; }
        public ICollection<Pet> Pets { get; set; }
    }

    public class Pet
    {
        public Guid PetId { get; set; }
        public string Name { get; set; }
    }

    public Fixture Fixture { get; set; }

    [SetUp]
    public void Setup()
    {
        Fixture = new Fixture();
        Fixture.Customize(new AutoMoqCustomization());

        var parentFileFieldWriteFilter = Fixture.Freeze<Mock<IFileFieldWriteFilter<Parent, V1>>>();
        parentFileFieldWriteFilter.Setup(x => x.ShouldExport(It.IsAny<IFileFieldWrite<Parent>>()))
            .Returns(true);
        var childFileFieldWriteFilter = Fixture.Freeze<Mock<IFileFieldWriteFilter<Child, V1>>>();
        childFileFieldWriteFilter.Setup(x => x.ShouldExport(It.IsAny<IFileFieldWrite<Child>>()))
            .Returns(true);
        var fileFieldWriteFilterFactory = Fixture.Freeze<Mock<IFileFieldWriteFilterFactory<V1>>>();

        fileFieldWriteFilterFactory.Setup(x => x.GetFilter<Parent>())
            .Returns(parentFileFieldWriteFilter.Object);
        fileFieldWriteFilterFactory.Setup(x => x.GetFilter<Child>())
            .Returns(childFileFieldWriteFilter.Object);
        fileFieldWriteFilterFactory.Setup(x => x.GetFilter<Pet>())
            .Returns((IFileFieldWriteFilter<Pet, V1>)null);
    }

    [TestCase]
    public async Task WriteRecordsAsync_Should_Work_With_Lists_Of_Lists()
    {
        //arrange
        var fileFieldAutoMapper = Fixture.Create<FileFieldAutoMapper<V1>>();

        var fields = fileFieldAutoMapper.MapOutput<Parent>();
        var parents = Fixture.CreateMany<Parent>(3).ToList();

        var writer = new CsvEntityFileWriter<V1>(fileFieldAutoMapper);

        //act
        var result = await writer.WriteRecordsAsync(fields, parents, default);
        result.Position = 0;
        var reader = new StreamReader(result);
        var exportResult = reader.ReadToEnd();

        //assert
        exportResult.Should().NotBeNullOrEmpty();
        exportResult.Should().ContainAll(parents.Select(x => x.ParentId.ToString()));
        exportResult.Should().ContainAll(parents.Select(x => x.Name));
        exportResult.Should().ContainAll(parents.SelectMany(x => x.Children.Select(y => y.ChildId.ToString())));
        exportResult.Should().ContainAll(parents.SelectMany(x => x.Children.Select(y => y.Name)));
        exportResult.Should().ContainAll(parents.SelectMany(x => x.Children.SelectMany(y => y.Pets.Select(z => z.PetId.ToString()))));
        exportResult.Should().ContainAll(parents.SelectMany(x => x.Children.SelectMany(y => y.Pets.Select(z => z.Name))));
        Regex.Matches(exportResult, "ParentId,Name").Count.Should().Be(3);
        Regex.Matches(exportResult, "\r\n\r\n\r\nParentId,Name").Count.Should().Be(2);
        Regex.Matches(exportResult, "\r\n\r\nChildId,Name").Count.Should().Be(9);
        Regex.Matches(exportResult, "\r\n\r\nPetId,Name").Count.Should().Be(9);
    }

    [TestCase]
    public async Task WriteRecordsAsync_Should_Work_With_Empty_And_Null_And_Single_List()
    {
        //arrange
        var fileFieldAutoMapper = Fixture.Create<FileFieldAutoMapper<V1>>();

        var fields = fileFieldAutoMapper.MapOutput<Parent>();
        var parents = Fixture.CreateMany<Parent>(3).ToList();
        parents[0].Children = new List<Child> { parents[0].Children[0] };
        parents[1].Children = null;
        parents[2].Children = new List<Child>();

        var writer = new CsvEntityFileWriter<V1>(fileFieldAutoMapper);

        //act
        var result = await writer.WriteRecordsAsync(fields, parents, default);
        result.Position = 0;
        var reader = new StreamReader(result);
        var exportResult = reader.ReadToEnd();

        //assert
        exportResult.Should().NotBeNullOrEmpty();
        exportResult.Should().ContainAll(parents.Select(x => x.ParentId.ToString()));
        exportResult.Should().ContainAll(parents.Select(x => x.Name));
        exportResult.Should().Contain(parents[0].Children[0].ChildId.ToString());
        exportResult.Should().Contain(parents[0].Children[0].Name);
        exportResult.Should().ContainAll(parents[0].Children[0].Pets.Select(z => z.PetId.ToString()));
        exportResult.Should().ContainAll(parents[0].Children[0].Pets.Select(z => z.Name));
        Regex.Matches(exportResult, "ParentId,Name").Count.Should().Be(2);
        Regex.Matches(exportResult, "\r\n\r\n\r\nParentId,Name").Count.Should().Be(1);
        Regex.Matches(exportResult, "\r\n\r\nChildId,Name").Count.Should().Be(1);
        Regex.Matches(exportResult, "\r\n\r\nPetId,Name").Count.Should().Be(1);
    }
}
