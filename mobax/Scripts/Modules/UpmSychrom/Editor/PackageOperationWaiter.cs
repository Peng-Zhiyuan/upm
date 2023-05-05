using UnityEditor.PackageManager.Requests;
using UnityEditor;
using System;
using System.Threading.Tasks;

class PackageOperationWaiter
{
    Request request;
    Action onComplete;

    public static Task Await(Request request)
    {
        var tcs = new TaskCompletionSource<bool>();
        var w = new PackageOperationWaiter(request, () =>
        {
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    PackageOperationWaiter(Request request, Action onComplete)
    {
        this.request = request;
        this.onComplete = onComplete;
        EditorApplication.update += OnUpdate;
    }

    void OnUpdate()
    {
        if (this.request.IsCompleted)
        {
            EditorApplication.update -= OnUpdate;
            onComplete.Invoke();
        }
    }
}
