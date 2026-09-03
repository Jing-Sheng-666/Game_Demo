using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement; 
/// <summary>
/// 音效管理器：从对象池取 AudioSource 播放一次性音效，播完自动回池
///
/// 【使用前提】
/// 1. Addressables 中存在预制体 "Sound/soundObj"：空物体 + AudioSource，并挂 PoolObj 设 maxNum（建议 20~30）
/// 2. 音效以完整地址加载，如 "Music/Wound"
/// 3. 首次访问 SoundMgr.Instance 时自动创建 DontDestroyOnLoad 宿主物体（也可自己在场景放一个）
/// </summary>
public class SoundMgr : MonoBehaviour
{
    private static SoundMgr _instance;
    public static SoundMgr Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("[SoundMgr]");
                DontDestroyOnLoad(obj);
                _instance = obj.AddComponent<SoundMgr>();
            }
            return _instance;
        }
    }

    //音效池对象的 Addressables key（必须与预制体地址一致）
    private const string SOUND_OBJ_KEY = "Sound/soundObj";

    //正在播放的音效
    private List<AudioSource> soundList = new List<AudioSource>();
    //音效音量大小
    private float soundValue = 0.1f;
    //音效是否在播放
    private bool soundIsPlay = true;

    private void Awake()
    {
        //单例去重：防止场景里手动放了一份、代码又自建一份
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        //从本地存档(GameDataMgr.musicData)同步音效音量与开关
        MusicData data = GameDataMgr.Instance.musicData;
        soundValue = data.soundValue;
        soundIsPlay = data.soundOpen;
    }

    private void Update()
    {
        if (!soundIsPlay)
            return;

        //逆向遍历：播放完毕的音效回池并移除记录
        for (int i = soundList.Count - 1; i >= 0; --i)
        {
            AudioSource s = soundList[i];

            //★新增：对象可能已随场景销毁（== null 判断），只移除记录，绝不能再碰它
            if (s == null)
            {
                soundList.RemoveAt(i);
                continue;
            }

            if (!s.isPlaying)
            {
                s.clip = null;
                PoolMgr.Instance.PushObject(s.gameObject, SOUND_OBJ_KEY);
                soundList.RemoveAt(i);
            }
        }
    }


    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name">完整地址，如 "Music/Wound"</param>
    /// <param name="isLoop">是否循环</param>
    /// <param name="callBack">播放开始后的回调（可选）</param>
    public void PlaySound(string name, bool isLoop = false, UnityAction<AudioSource> callBack = null)
    {
        //音效总开关关着时直接不播（连加载都省了）
        if (!soundIsPlay)
            return;

        AudioClip clip = AddressablesMgr.Instance.LoadAssetSync<AudioClip>(name);
        if (clip == null)
        {
            Debug.LogWarning($"音效加载失败:{name}");
            return;
        }

        //从池取一个带 AudioSource 的音效对象
        GameObject obj = PoolMgr.Instance.GetObject(SOUND_OBJ_KEY);
        AudioSource source = obj.GetComponent<AudioSource>();
        if (source == null)
        {
            Debug.LogError($"预制体 {SOUND_OBJ_KEY} 上没有 AudioSource 组件");
            return;
        }

        //若取到的是上一个还在播的对象，先停掉再播新的
        source.Stop();
        source.clip = clip;
        source.loop = isLoop;
        source.volume = soundValue;
        source.Play();

        //记录起来让 Update 检测播完回池；避免重复记录同一个 source
        if (!soundList.Contains(source))
            soundList.Add(source);

        callBack?.Invoke(source);
    }

    /// <summary>
    /// 停止某个音效并回池
    /// </summary>
    public void StopSound(AudioSource source)
    {
        if (source != null && soundList.Contains(source))
        {
            source.Stop();
            soundList.Remove(source);
            source.clip = null;
            PoolMgr.Instance.PushObject(source.gameObject, SOUND_OBJ_KEY);
        }
    }

    /// <summary>
    /// 修改音效音量（并写回存档）
    /// </summary>
    public void ChangeSoundValue(float v)
    {
        soundValue = v;
        GameDataMgr.Instance.musicData.soundValue = v;   //写回存档，SettingPanel 关闭时才能保存
        for (int i = 0; i < soundList.Count; i++)
        {
            soundList[i].volume = v;
        }
    }

    /// <summary>
    /// 暂停/继续所有音效（并写回存档）
    /// </summary>
    public void PlayOrPauseSound(bool isPlay)
    {
        soundIsPlay = isPlay;
        GameDataMgr.Instance.musicData.soundOpen = isPlay;   //写回存档

        if (isPlay)
        {
            for (int i = 0; i < soundList.Count; i++)
                soundList[i].Play();
        }
        else
        {
            for (int i = 0; i < soundList.Count; i++)
                soundList[i].Pause();
        }
    }

    /// <summary>
    /// 清空所有音效记录（过场景清理池之前调用）
    /// </summary>
    public void ClearSound()
    {
        for (int i = soundList.Count - 1; i >= 0; --i)
        {
            AudioSource s = soundList[i];

            //★新增：已随场景销毁的对象只移除记录，不再 Stop/入池（入池会再次访问尸体）
            if (s == null)
            {
                soundList.RemoveAt(i);
                continue;
            }

            s.Stop();
            s.clip = null;
            PoolMgr.Instance.PushObject(s.gameObject, SOUND_OBJ_KEY);
            soundList.RemoveAt(i);
        }
        soundList.Clear();
    }
    /// <summary>
    /// 场景加载完成后的统一清理：游戏结束回主菜单、切换关卡都会触发
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearSound();                   //清掉上一场景残留的音效（活的回池、销毁的移除）
        PoolMgr.Instance.ClearPool();   //清掉旧场景的对象池（池对象已随场景销毁，字典里全是尸体）
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;   //销毁时注销监听
    }

}
