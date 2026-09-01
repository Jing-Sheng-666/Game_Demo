using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UpdateMgr: MonoBehaviour
{
    private static UpdateMgr _instance;
    public static UpdateMgr Instance => _instance;

    // 新增：进度事件（0~1）和流程结束事件
    public static event Action<float> OnUpdateProgress;
    public static event Action<bool> OnUpdateFinished;   // true=成功，false=失败

    private readonly List<string> remoteKeys = new List<string>() { "Remote" };

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        // 阶段一：检查
        OnUpdateProgress?.Invoke(0.05f);
        var check = Addressables.CheckForCatalogUpdates(false);
        yield return check;
        if (check.Result == null || check.Result.Count == 0)
        {
            Debug.Log("[UpdateManager] 无更新");
            OnUpdateProgress?.Invoke(1f);
            OnUpdateFinished?.Invoke(true);        // ← 无更新 = 成功
            yield break;
        }

        // 阶段二：应用 catalog（失败回退）
        var update = Addressables.UpdateCatalogs(check.Result);
        yield return update;
        if (update.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning("[UpdateManager] catalog 失败，回退本地");
            OnUpdateFinished?.Invoke(false);       // ← catalog 失败
            yield break;
        }

        // 阶段三：大小
        var size = Addressables.GetDownloadSizeAsync(remoteKeys);
        yield return size;
        if (size.Result <= 0) { OnUpdateFinished?.Invoke(true); yield break; }

        // 阶段四：下载（进度上报给面板）
        var dl = Addressables.DownloadDependenciesAsync(remoteKeys, Addressables.MergeMode.Union);
        while (!dl.IsDone)
        {
            OnUpdateProgress?.Invoke(dl.PercentComplete);
            yield return null;
        }

        Debug.Log(dl.Status == AsyncOperationStatus.Succeeded
            ? "[UpdateManager] 更新完成"
            : "[UpdateManager] 下载失败，回退本地");

        OnUpdateFinished?.Invoke(dl.Status == AsyncOperationStatus.Succeeded);
    }
}
