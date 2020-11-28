using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.TestInfrastucture
{
    [TestFixture]
    public class TestLinq
    {
        public static IEnumerable<TestCaseData> TestLinqCases
        {
            get
            {
                yield return new TestCaseData(new Tuple<string, int>[0], new string[0]);
                yield return new TestCaseData(new[] { Tuple.Create("peace", 1)}, new []{"peace"});
                yield return new TestCaseData(new[] { Tuple.Create("peace", 2), Tuple.Create("mafia", 2) },
                    new []{"peace", "peace", "mafia", "mafia"});
                yield return new TestCaseData(new[]
                        {Tuple.Create("peace", 0), Tuple.Create("mafia", 1), Tuple.Create("doctor", 3)},
                    new[] {"mafia", "doctor", "doctor", "doctor"});
            }
        }

        [TestCaseSource("TestLinqCases")]
        public void MultiplySuccess(IEnumerable<Tuple<string, int>> input, string[] result)
        {
            
            var output = Mafia.Infrastructure.MoreLinq.Multiply(input).ToArray();
            output.Should().HaveCount(result.Length, "because we should put all objects" +
                                                    "which is the first element in tuple as many times as second value" +
                                                    "in tuple says");
            output.Should().Equal(result);
        }

        [Test]
        public void MultiplyErrors()
        {
            ((Action)(() =>
                {
                    foreach (var s in Mafia.Infrastructure.MoreLinq.Multiply<string>(null)) {}
                }))
                .Should().Throw<NullReferenceException>();
        }
    }
}