using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class UpdateManager : MonoBehaviour
{
    private static UpdateManager _instance;
    public static UpdateManager Instance => _instance;

    // 用 label 收集远程资源：给 RemoteGroup 里所有资源打个 label "Remote"
    private readonly List<string> remoteKeys = new List<string>() { "Remote" };

    private void Awake()
    {
        if (_instance == null) _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private IEnumerator Start()
    {
        // 1. 检查远程是否有新 catalog
        var check = Addressables.CheckForCatalogUpdates(false);
        yield return check;
        if (check.Result == null || check.Result.Count == 0)
        {
            Debug.Log("[UpdateManager] 无更新，直接进入游戏");
            yield break;
        }

        // 2. 应用新 catalog
        var update = Addressables.UpdateCatalogs(check.Result);
        yield return update;

        // 3. 计算下载大小
        var size = Addressables.GetDownloadSizeAsync(remoteKeys);
        yield return size;
        if (size.Result <= 0) { Debug.Log("[UpdateManager] 有更新但无需下载"); yield break; }

        // 4. 下载（进度可接 UI 进度条）
        var dl = Addressables.DownloadDependenciesAsync(remoteKeys, Addressables.MergeMode.Union);
        while (!dl.IsDone)
        {
            Debug.Log($"[UpdateManager] 下载 {dl.PercentComplete * 100:0.0}%");//0.0第一个0表示整数位前至少有一个0，第二个0表示小数位后至少有一个0
            yield return null;
        }
        Debug.Log(dl.Status == AsyncOperationStatus.Succeeded
            ? "[UpdateManager] 下载完成，进入游戏"
            : "[UpdateManager] 下载失败，回退本地版本");
    }
}
