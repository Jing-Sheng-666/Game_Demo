using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// AutoReturnPool.cs —— 挂在特效预制体上
public class AutoReturnPoolPS : MonoBehaviour
{
    public float delay = 1f;      // 播多久回池，每类特效可在预制体上单独调

    ParticleSystem ps;

    void Awake() { ps = GetComponentInChildren<ParticleSystem>(); }

    // 用 OnEnable 而不是 Start：每次从池里 Pop 出来 SetActive(true) 都会触发
    void OnEnable()
    {
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); // 清掉上一轮的残留粒子
            ps.Play();
        }
        Invoke("ReturnObj", delay);
    }

    void OnDisable()
    {
        CancelInvoke();   // 防止回池失活后还残留计时器
    }

    void ReturnObj()
    {
        PoolMgr.Instance.PushObject(this.gameObject, this.gameObject.name);  // 回池
    }
}
