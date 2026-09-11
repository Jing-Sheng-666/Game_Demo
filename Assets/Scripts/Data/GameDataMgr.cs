using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// 专门用来管理数据的类
/// </summary>
public class GameDataMgr : BaseManager<GameDataMgr>
{
    // 是否已完成初始化（配置表加载完）
    public bool isInited = false;
    // 初始化完成事件：热更阶段就创建的对象（BKMusic 等）可以等它
    public event Action OnInitFinished;

    //记录选择的角色数据 用于之后在游戏场景中创建
    public RoleInfo nowSelRole;

    //音效相关数据
    public MusicData musicData = new MusicData();
    //玩家相关数据
    public PlayerData playerData = new PlayerData();

    //所有的角色数据
    public List<RoleInfo> roleInfoList = new List<RoleInfo>();
    //所有的场景数据
    public List<SceneInfo> sceneInfoList = new List<SceneInfo>();
    //所有的怪物数据
    public List<MonsterInfo> monsterInfoList = new List<MonsterInfo>();
    //所有塔的数据
    public List<TowerInfo> towerInfoList = new List<TowerInfo>();

    private GameDataMgr() { }   // 构造函数只给空壳，重活全部挪进 InitAsync/InitSync

    /// <summary>
    /// 异步初始化（正式启动流程用）：热更完成后调用，不阻塞主线程
    /// </summary>
    public IEnumerator InitAsync()
    {
        if (isInited) yield break;

        // 1) 本地读档（小 json，毫秒级，放主线程可接受；大头是下面的配置表）
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");

        // 2) 4 组配置表异步加载：yield 等待期间主线程照常渲染进度条
        var roleHandle = Addressables.LoadAssetsAsync<RoleInfo>("RoleData", null);
        yield return roleHandle;
        if (roleHandle.Status == AsyncOperationStatus.Succeeded)
            roleInfoList = new List<RoleInfo>(roleHandle.Result);
        else
            Debug.LogWarning("RoleData 加载失败");

        var monsterHandle = Addressables.LoadAssetsAsync<MonsterInfo>("MonsterData", null);
        yield return monsterHandle;
        if (monsterHandle.Status == AsyncOperationStatus.Succeeded)
            monsterInfoList = new List<MonsterInfo>(monsterHandle.Result);
        else
            Debug.LogWarning("MonsterData 加载失败");

        var towerHandle = Addressables.LoadAssetsAsync<TowerInfo>("TowerData", null);
        yield return towerHandle;
        if (towerHandle.Status == AsyncOperationStatus.Succeeded)
            towerInfoList = new List<TowerInfo>(towerHandle.Result);
        else
            Debug.LogWarning("TowerData 加载失败");

        var sceneHandle = Addressables.LoadAssetsAsync<SceneInfo>("SceneData", null);
        yield return sceneHandle;
        if (sceneHandle.Status == AsyncOperationStatus.Succeeded)
            sceneInfoList = new List<SceneInfo>(sceneHandle.Result);
        else
            Debug.LogWarning("SceneData 加载失败");

        FinishInit();
    }

    /// <summary>
    /// 同步兜底初始化（测试直达用）：直接开 GameScene 没走启动流程时调用，保证不崩
    /// </summary>
    public void InitSync()
    {
        if (isInited) return;

        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");

        var roleHandle = Addressables.LoadAssetsAsync<RoleInfo>("RoleData", null);
        roleInfoList = new List<RoleInfo>(roleHandle.WaitForCompletion());
        var monsterHandle = Addressables.LoadAssetsAsync<MonsterInfo>("MonsterData", null);
        monsterInfoList = new List<MonsterInfo>(monsterHandle.WaitForCompletion());
        var towerHandle = Addressables.LoadAssetsAsync<TowerInfo>("TowerData", null);
        towerInfoList = new List<TowerInfo>(towerHandle.WaitForCompletion());
        var sceneHandle = Addressables.LoadAssetsAsync<SceneInfo>("SceneData", null);
        sceneInfoList = new List<SceneInfo>(sceneHandle.WaitForCompletion());

        FinishInit();
    }

    private void FinishInit()
    {
        // 按 id 排序，保证 list[id - 1] 这类索引安全
        roleInfoList.Sort((a, b) => a.id.CompareTo(b.id));
        monsterInfoList.Sort((a, b) => a.id.CompareTo(b.id));
        towerInfoList.Sort((a, b) => a.id.CompareTo(b.id));
        sceneInfoList.Sort((a, b) => a.id.CompareTo(b.id));

        isInited = true;
        OnInitFinished?.Invoke();
    }


    /// <summary>存储音效数据</summary>
    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }

    /// <summary>存储玩家数据</summary>
    public void SavePlayerData()
    {
        JsonMgr.Instance.SaveData(playerData, "PlayerData");
    }

    /// <summary>播放音效（门面，转发给 SoundMgr）</summary>
    public void PlaySound(string resName)
    {
        SoundMgr.Instance.PlaySound(resName);
    }
}
