using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

public static class SetAddressTools
{
    // 把所有 ResLocal / ResRemote 下的 Addressable 的 Address 设为原 Resources 路径
    [MenuItem("Tools/Addressables/Set Address From Path")]
    public static void Reset()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { Debug.LogError("Addressables 未初始化"); return; }

        int count = 0;
        foreach (var g in settings.groups)
        {
            if (g == null || g.entries == null) continue;
            foreach (var e in g.entries)
            {
                var path = e.AssetPath;
                if (string.IsNullOrEmpty(path)) continue;

                string prefix = null;
                if (path.StartsWith("Assets/ResLocal/")) prefix = "Assets/ResLocal/";
                else if (path.StartsWith("Assets/ResRemote/")) prefix = "Assets/ResRemote/";
                if (prefix == null) continue;

                var rel = path.Substring(prefix.Length);            // "UI/BeginPanel.prefab"
                int dot = rel.LastIndexOf('.');
                if (dot > 0) rel = rel.Substring(0, dot);          // "UI/BeginPanel"
                e.SetAddress(rel);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"已批量设置 {count} 个 Addressable 的 Address");
    }
}
