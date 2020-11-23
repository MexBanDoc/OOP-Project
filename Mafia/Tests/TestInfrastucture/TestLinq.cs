using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Tests.TestInfrastucture
{
    [TestFixture]
    public class TestLinq
    {
        public static IEnumerable<TestCaseData> TestLinqCases
        {
            get
            {
                yield return new TestCaseData(null, null);
                yield return new TestCaseData(new (string, int)[0], new string[0]);
                yield return new TestCaseData(new[] { ("peace", 1)}, new []{"peace"});
                yield return new TestCaseData(new[] { ("peace", 2), ("mafia", 2) },
                    new []{"peace", "peace", "mafia", "mafia"});
                yield return new TestCaseData(new[] {("peace", 0), ("mafia", 1), ("doctor", 3)},
                    new[] {"mafia", "doctor", "doctor", "doctor"});
            }
        }

        [TestCaseSource("TestLinqCases")]
        public void Multiply(IEnumerable<Tuple<string, int>> input, string[] result)
        {
            
            var output = Mafia.Infrastructure.MoreLinq.Multiply(input).ToArray();
            output.Should().HaveCount(result.Length, "because we should put all objects" +
                                                    "which is the first element in tuple as many times as second value" +
                                                    "in tuple says");
            output.Should().Equal(result);
        }
    }
}