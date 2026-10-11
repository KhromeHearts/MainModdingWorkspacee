using System.Collections.Concurrent;
using NeoModLoader.services;
using UnityEngine;

namespace NeoModLoader.utils;

/// <summary>
///     Runs callbacks on Unity's main thread. Before <see cref="Init" /> (e.g. in unit tests) callbacks run inline.
/// </summary>
internal static class MainThreadDispatcher
{
    private const int max_actions_per_frame = 256;
    private static readonly ConcurrentQueue<Action> queue = new();
    private static int main_thread_id = -1;

    public static bool IsMainThread => main_thread_id == -1 || Thread.CurrentThread.ManagedThreadId == main_thread_id;

    public static void Init(GameObject pHost)
    {
        main_thread_id = Thread.CurrentThread.ManagedThreadId;
        pHost.AddComponent<DispatcherBehaviour>();
    }

    /// <summary>
    ///     Run now if already on the main thread, otherwise on the next frame.
    /// </summary>
    public static void Run(Action pAction)
    {
        if (IsMainThread)
        {
            SafeInvoke(pAction);
            return;
        }

        queue.Enqueue(pAction);
    }

    /// <summary>
    ///     Run on the main thread and return its result. Continuations of the returned task do not run on the main
    ///     thread.
    /// </summary>
    public static Task<T> RunAsync<T>(Func<T> pFunc)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

        void execute()
        {
            try
            {
                completion.SetResult(pFunc());
            }
            catch (Exception e)
            {
                completion.SetException(e);
            }
        }

        if (IsMainThread)
        {
            execute();
        }
        else
        {
            queue.Enqueue(execute);
        }

        return completion.Task;
    }

    private static void SafeInvoke(Action pAction)
    {
        try
        {
            pAction();
        }
        catch (Exception e)
        {
            LogService.LogError($"Exception in a main thread callback: {e.Message}");
            LogService.LogException(e);
        }
    }

    private class DispatcherBehaviour : MonoBehaviour
    {
        private void Update()
        {
            for (int i = 0; i < max_actions_per_frame && queue.TryDequeue(out Action action); i++)
            {
                SafeInvoke(action);
            }
        }
    }
}
