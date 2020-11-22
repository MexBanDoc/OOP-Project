using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Tests.TestInfrastucture
{
    public class TestLinq
    {
        private static IEnumerable<(string, int)[]> Tuples
        {
            get
            {
                yield return new[] { ("peace", 2), ("mafia", 2) };
            }
        }
        
        private static IEnumerable<(string, int)[]> EmptyTuples
        {
            get
            {
                yield return new (string, int)[0];
            }
        }
        
        private static IEnumerable<(string, int)[]> UniqueTuple
        {
            get
            {
                yield return new[] { ("peace", 1)};
            }
        }
        
        private static IEnumerable<(string, int)[]> UniqueEmptyTuple
        {
            get
            {
                yield return new[] { ("peace", 0)};
            }
        }
        
        
        [TestCase(null, null)]
        [TestCase(nameof(EmptyTuples), new string[0])]
        [TestCase(nameof(UniqueTuple), new []{"peace"})]
        [TestCase(nameof(UniqueEmptyTuple), new string[0])]
        [TestCase(nameof(Tuples), new []{"peace", "peace", "mafia", "mafia"})]
        public void Multiply<T>(IEnumerable<Tuple<T, int>> input, T[] result)
        {
            
            var output = Mafia.Infrastructure.MoreLinq.Multiply(input).ToArray();
            output.Should().HaveCount(result.Length, "because we should put all objects" +
                                                    "which is the first element in tuple as many times as second value" +
                                                    "in tuple says");
            output.Should().Equal(result);
        }
    }
}