using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTowerObject : MonoBehaviour
{
    //血量相关
    private int hp;
    private int maxHp;
    //是否死亡
    private bool isDead;

    //能够被别人快速获取到位置
    private static MainTowerObject instance;
    public static MainTowerObject Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    //更新血量
    public void UpdateHp(int hp, int maxHP)
    {
        this.hp = hp;
        this.maxHp = maxHP;

        //更新血量：血量变化广播事件
        EventCenter.Instance.EventTrigger(E_EventType.E_MainTower_HpChanged, new MainTowerHpArgs(hp, maxHP));
    }

    //自己受到伤害
    public void Wound(int dmg)
    {
        //如果保护区域已经被打死 就没有必要再减血了
        if (isDead)
            return;
        //受到伤害
        hp -= dmg;
        //死亡逻辑
        if( hp <= 0 )
        {
            hp = 0;
            isDead = true;
            //失败奖励 = 当前金钱一半；结算面板由 E_Game_Over 的监听方（UI）弹出
            EventCenter.Instance.EventTrigger(E_EventType.E_Game_Over,
            new GameOverArgs((int)(GameLevelMgr.Instance.player.money * 0.5f), false));
        }

        //更新血量
        UpdateHp(hp, maxHp);
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
