using System;
using System.Collections.Generic;
using Firebend.AutoCrud.Core.ObjectMapping;
using FluentAssertions;
using NUnit.Framework;

namespace Firebend.AutoCrud.Tests.Core
{
    [TestFixture]
    public class MapperTests
    {
        [TestCase]
        public void
            Mapper_Should_Not_Copy_Of_Ignored_Properties_After_Full_Copy_Operation()
        {
            // given
            var givenSourceObj = new MapperTestObjClass
            {
                Name = "Prime Jedi",
                Age = 4500,
                Birthdate = DateTime.Now.AddYears(1000),
                IsJedi = true
            };
            var givenTargetObj = new MapperTestObjClass();
            var givenTargetObjWithIgnoredProperties = new MapperTestObjClass();
            var givenTargetObjWithDifferentIgnoredProperties = new MapperTestObjClass();

            // when
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObj);
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObjWithIgnoredProperties, new[] {"Age", "Name"});
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObjWithDifferentIgnoredProperties, new[] {"Age"});

            // then
            var expectedAge = 0;

            givenTargetObjWithIgnoredProperties.Should().NotBeNull();
            givenTargetObjWithIgnoredProperties.Name.Should().BeNull();
            givenTargetObjWithIgnoredProperties.Age.Should().Be(expectedAge);
            givenTargetObjWithIgnoredProperties.Birthdate.Should().Be(givenSourceObj.Birthdate);
            givenTargetObjWithIgnoredProperties.IsJedi.Should().Be(givenSourceObj.IsJedi);

            givenTargetObjWithDifferentIgnoredProperties.Name.Should().Be(givenSourceObj.Name);
            givenTargetObjWithDifferentIgnoredProperties.Age.Should().Be(expectedAge);
        }

        [TestCase]
        public void Mapper_Should_Map_Primitive_Type_Props_Without_Ignored_Props()
        {
            // given
            var givenSourceObj = new MapperTestObjClass
            {
                Name = "Prime Jedi",
                Age = 4500,
                Birthdate = DateTime.Now.AddYears(1000),
                IsJedi = true
            };
            var givenTargetObj = new MapperTestObjClass();

            // when
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObj, new [] {"Age", "Name"});

            // then
            var expectedAge = 0;

            givenTargetObj.Should().NotBeNull();
            givenTargetObj.Name.Should().BeNull();
            givenTargetObj.Age.Should().Be(expectedAge);
            givenTargetObj.Birthdate.Should().Be(givenSourceObj.Birthdate);
            givenTargetObj.IsJedi.Should().Be(givenSourceObj.IsJedi);
        }

        [TestCase]
        public void Mapper_Should_Map_Primitive_Type_Props()
        {
            // given
            var givenSourceObj = new MapperTestObjClass
            {
                Name = "Prime Jedi",
                Age = 4500,
                Birthdate = DateTime.Now.AddYears(1000),
                IsJedi = true
            };
            var givenTargetObj = new MapperTestObjClass();

            // when
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObj);

            // then
            givenTargetObj.Should().NotBeNull();
            givenTargetObj.Name.Should().Be(givenSourceObj.Name);
            givenTargetObj.Age.Should().Be(givenSourceObj.Age);
            givenTargetObj.Birthdate.Should().Be(givenSourceObj.Birthdate);
            givenTargetObj.IsJedi.Should().Be(givenSourceObj.IsJedi);
        }

        [TestCase]
        public void Mapper_Should_Map_Objects_With_Lists()
        {
            // given
            var givenSourceObj = new MapperTestObjWithListClass
            {
                Name = "Ahsoka Tano",
                Enemies = new List<MapperTestObjClass>
                {
                    new MapperTestObjClass
                    {
                        Name = "Darth Maul",
                        Age = 54,
                        Birthdate = DateTime.Now.AddYears(1000),
                        IsJedi = false
                    },
                    new MapperTestObjClass
                    {
                        Name = "Darth Sidious",
                        Age = 84,
                        Birthdate = DateTime.Now.AddYears(5000),
                        IsJedi = false
                    },
                }
            };
            var givenTargetObj = new MapperTestObjWithListClass();

            // when
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObj);

            // then
            givenTargetObj.Should().NotBeNull();

            givenTargetObj.Name.Should().Be(givenSourceObj.Name);

            givenTargetObj.Enemies.Should().NotBeNull();
            givenTargetObj.Enemies.Count.Should()
                .Be(givenSourceObj.Enemies.Count);
            givenTargetObj.Enemies[0].Name.Should()
                .Be(givenSourceObj.Enemies[0].Name);
        }

        [TestCase]
        public void Mapper_Should_Map_Objects_By_Different_Types()
        {
            // given
            var givenSourceObj = new MapperTestObjClass
            {
                Name = "Prime Jedi",
                Age = 4500,
                Birthdate = DateTime.Now.AddYears(1000),
                IsJedi = true
            };
            var givenTargetObj = new MapperTestAnotherObjClass();

            // when
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObj);

            // then
            givenTargetObj.Should().NotBeNull();
            givenTargetObj.Name.Should().Be(givenSourceObj.Name);
            givenTargetObj.Age.Should().Be(givenSourceObj.Age);
            givenTargetObj.Birthdate.Should().Be(givenSourceObj.Birthdate);
            givenTargetObj.IsJedi.Should().Be(givenSourceObj.IsJedi);
        }

        [TestCase]
        public void Mapper_Should_Map_Objects_By_Different_Types_With_Lists()
        {
            // given
            var givenSourceObj = new MapperTestObjWithListClass
            {
                Name = "Ahsoka Tano",
                Enemies = new List<MapperTestObjClass>
                {
                    new MapperTestObjClass
                    {
                        Name = "Darth Maul",
                        Age = 54,
                        Birthdate = DateTime.Now.AddYears(1000),
                        IsJedi = false
                    },
                    new MapperTestObjClass
                    {
                        Name = "Darth Sidious",
                        Age = 84,
                        Birthdate = DateTime.Now.AddYears(5000),
                        IsJedi = false
                    },
                }
            };
            var givenTargetObj = new MapperTestAnotherObjWithListClass();

            // when
            ObjectMapper.Instance.Copy(givenSourceObj, givenTargetObj);

            // then
            givenTargetObj.Should().NotBeNull();

            givenTargetObj.Name.Should().Be(givenSourceObj.Name);

            givenTargetObj.Enemies.Should().NotBeNull();
            givenTargetObj.Enemies.Count.Should()
                .Be(givenSourceObj.Enemies.Count);
            givenTargetObj.Enemies[0].Name.Should()
                .Be(givenSourceObj.Enemies[0].Name);
        }
    }

    internal class MapperTestObjClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Birthdate { get; set; }
        public bool IsJedi { get; set; }
    }

    internal class MapperTestObjWithListClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Birthdate { get; set; }
        public List<MapperTestObjClass> Enemies { get; set; }
    }

    internal class MapperTestAnotherObjClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Birthdate { get; set; }
        public bool IsJedi { get; set; }
    }

    internal class MapperTestAnotherObjWithListClass
    {
        public string Name { get; set; }
        public List<MapperTestAnotherObjClass> Enemies { get; set; }
    }
}
