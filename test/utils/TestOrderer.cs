namespace FinalApi.Test.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    /*
     * Microsoft plumbing to get tests to run in the order with which I present them
     * https://learn.microsoft.com/en-us/dotnet/core/testing/order-unit-tests?pivots=xunit
     */
    public class TestOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
                where TTestCase : ITestCase
        {
            string assemblyName = typeof(TestOrderAttribute).AssemblyQualifiedName!;
            var sortedMethods = new SortedDictionary<int, List<TTestCase>>();
            foreach (TTestCase testCase in testCases)
            {
                int testOrder = testCase.TestMethod.Method
                    .GetCustomAttributes(assemblyName)
                    .FirstOrDefault()
                    ?.GetNamedArgument<int>(nameof(TestOrderAttribute.TestOrder)) ?? 0;

                GetOrCreate(sortedMethods, testOrder).Add(testCase);
            }

            foreach (TTestCase testCase in
                sortedMethods.Keys.SelectMany(
                    priority => sortedMethods[priority].OrderBy(
                        testCase => testCase.TestMethod.Method.Name)))
            {
                yield return testCase;
            }
        }

        private static TValue GetOrCreate<TKey, TValue>(
            IDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : struct
            where TValue : new() =>
            dictionary.TryGetValue(key, out TValue result)
                ? result
                : (dictionary[key] = new TValue());
    }
}
