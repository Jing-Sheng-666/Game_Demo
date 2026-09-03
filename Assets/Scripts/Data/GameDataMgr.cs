using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// 专门用来管理数据的类
/// </summary>
public class GameDataMgr
{
    private static GameDataMgr instance = new GameDataMgr();
    public static GameDataMgr Instance => instance;

    //记录选择的角色数据 用于之后在游戏场景中创建
    public RoleInfo nowSelRole;

    //音效相关数据
    public MusicData musicData;

    //玩家相关数据
    public PlayerData playerData;

    //所有的角色数据
    public List<RoleInfo> roleInfoList;

    //所有的场景数据
    public List<SceneInfo> sceneInfoList;

    //所有的怪物数据
    public List<MonsterInfo> monsterInfoList;

    //所有塔的数据
    public List<TowerInfo> towerInfoList;

    private GameDataMgr()
    {
        //初始化一些默认数据
        musicData = JsonMgr.Instance.LoadData<MusicData>("MusicData");
        //获取初始化玩家数据
        playerData = JsonMgr.Instance.LoadData<PlayerData>("PlayerData");
        //初始化所有的角色数据，怪物数据，塔数据，场景数据
        var roleHandle = Addressables.LoadAssetsAsync<RoleInfo>("RoleData", null);
        roleInfoList = new List<RoleInfo>(roleHandle.WaitForCompletion());
        var monsterHandle = Addressables.LoadAssetsAsync<MonsterInfo>("MonsterData", null);
        monsterInfoList = new List<MonsterInfo>(monsterHandle.WaitForCompletion());
        var towerHandle = Addressables.LoadAssetsAsync<TowerInfo>("TowerData", null);
        towerInfoList = new List<TowerInfo>(towerHandle.WaitForCompletion());
        var sceneHandle = Addressables.LoadAssetsAsync<SceneInfo>("SceneData", null);
        sceneInfoList = new List<SceneInfo>(sceneHandle.WaitForCompletion());


        // 按 id 排序，保证 list[id - 1] 这类索引安全
        roleInfoList.Sort((a, b) => a.id.CompareTo(b.id));
        monsterInfoList.Sort((a, b) => a.id.CompareTo(b.id));
        towerInfoList.Sort((a, b) => a.id.CompareTo(b.id));
        sceneInfoList.Sort((a, b) => a.id.CompareTo(b.id));

    }

    /// <summary>
    /// 存储音效数据
    /// </summary>
    public void SaveMusicData()
    {
        JsonMgr.Instance.SaveData(musicData, "MusicData");
    }

    /// <summary>
    /// 存储玩家数据
    /// </summary>
    public void SavePlayerData()
    {
        JsonMgr.Instance.SaveData(playerData, "PlayerData");
    }

    /// <summary>
    /// 播放音效方法
    /// </summary>
    /// <param name="resName"></param>
    public void PlaySound(string resName)
    {
        SoundMgr.Instance.PlaySound(resName);
    }
}
