using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BKMusic : MonoBehaviour
{
    private static BKMusic instance;
    public static BKMusic Instacne => instance;

    private AudioSource bkSource;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        bkSource = this.GetComponent<AudioSource>();

        // 启动阶段 GameDataMgr 可能还没初始化完（要等热更），完成后再应用音量设置
        if (GameDataMgr.Instance.isInited)
            ApplyMusicSetting();
        else
            GameDataMgr.Instance.OnInitFinished += ApplyMusicSetting;
    }

    private void OnDestroy()
    {
        // 防止事件持有已销毁对象（场景切换时 BKMusic 会被卸载）
        if (GameDataMgr.Instance != null)
            GameDataMgr.Instance.OnInitFinished -= ApplyMusicSetting;
    }

    // 把原来 Awake 里读数据设音量的逻辑搬到这里
    private void ApplyMusicSetting()
    {
        MusicData data = GameDataMgr.Instance.musicData;
        SetIsOpen(data.musicOpen);
        ChangeValue(data.musicValue);
    }

    //开关背景音乐的方法
    public void SetIsOpen(bool isOpen)
    {
        bkSource.mute = !isOpen;
    }

    //调整被背景音乐大小的方法
    public void ChangeValue(float v)
    {
        bkSource.volume = v;
    }
}
