using System.Diagnostics;
using System.Reflection;

namespace NeoModLoader.Tests;

/// <summary>
///     Marks a public static test method. It may return <see cref="Task" />.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class TestAttribute : Attribute
{
}

internal static class Program
{
    private static readonly TimeSpan test_timeout = TimeSpan.FromSeconds(30);

    /// <param name="args">Optional name filters; a test runs if its name contains any of them.</param>
    private static int Main(string[] args)
    {
        var tests = typeof(Program).Assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method => method.GetCustomAttribute<TestAttribute>() != null)
                .Select(method => (type, method)))
            .OrderBy(test => test.type.Name).ThenBy(test => test.method.Name)
            .ToList();

        int passed = 0;
        var failures = new List<string>();
        foreach ((Type type, MethodInfo method) in tests)
        {
            string name = $"{type.Name}.{method.Name}";
            if (args.Length > 0 && !args.Any(filter => name.Contains(filter))) continue;

            Stopwatch watch = Stopwatch.StartNew();
            try
            {
                if (method.Invoke(null, null) is Task task)
                {
                    if (!task.Wait(test_timeout)) throw new TimeoutException($"timed out after {test_timeout}");
                    task.GetAwaiter().GetResult();
                }

                passed++;
                Console.WriteLine($"PASS {name} ({watch.ElapsedMilliseconds} ms)");
            }
            catch (Exception e)
            {
                Exception error = e is TargetInvocationException or AggregateException
                    ? e.InnerException ?? e
                    : e;
                failures.Add(name);
                Console.WriteLine($"FAIL {name}: {error}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{passed} passed, {failures.Count} failed");
        foreach (string failure in failures) Console.WriteLine($"  failed: {failure}");
        return failures.Count == 0 ? 0 : 1;
    }
}

internal sealed class AssertionException : Exception
{
    public AssertionException(string pMessage) : base(pMessage)
    {
    }
}

internal static class Assert
{
    public static void True(bool pCondition, string pMessage = "expected true")
    {
        if (!pCondition) throw new AssertionException(pMessage);
    }

    public static void False(bool pCondition, string pMessage = "expected false")
    {
        if (pCondition) throw new AssertionException(pMessage);
    }

    public static void Equal<T>(T pExpected, T pActual, string pMessage = null)
    {
        if (!EqualityComparer<T>.Default.Equals(pExpected, pActual))
        {
            throw new AssertionException($"{pMessage ?? "values differ"}: expected <{pExpected}>, got <{pActual}>");
        }
    }

    public static void SequenceEqual<T>(IEnumerable<T> pExpected, IEnumerable<T> pActual, string pMessage = null)
    {
        List<T> expected = pExpected.ToList();
        List<T> actual = pActual.ToList();
        if (!expected.SequenceEqual(actual))
        {
            throw new AssertionException(
                $"{pMessage ?? "sequences differ"}: expected [{string.Join(", ", expected)}], got [{string.Join(", ", actual)}]");
        }
    }

    public static void Contains(string pExpectedPart, string pActual)
    {
        if (pActual == null || !pActual.Contains(pExpectedPart))
        {
            throw new AssertionException($"expected <{pActual}> to contain <{pExpectedPart}>");
        }
    }

    public static void Null(object pValue, string pMessage = "expected null")
    {
        if (pValue != null) throw new AssertionException($"{pMessage}: got <{pValue}>");
    }

    public static void NotNull(object pValue, string pMessage = "expected a value")
    {
        if (pValue == null) throw new AssertionException(pMessage);
    }

    /// <summary>
    ///     Poll until <paramref name="pCondition" /> holds, for code that finishes on another thread.
    /// </summary>
    public static async Task Eventually(Func<bool> pCondition, string pMessage, int pTimeoutMs = 5000)
    {
        Stopwatch watch = Stopwatch.StartNew();
        while (!pCondition())
        {
            if (watch.ElapsedMilliseconds > pTimeoutMs) throw new AssertionException($"timed out: {pMessage}");
            await Task.Delay(5);
        }
    }
}
