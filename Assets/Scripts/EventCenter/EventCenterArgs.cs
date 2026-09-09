using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 波数变化事件参数
/// </summary>
public class WaveArgs
{
    public int now;
    public int max;

    public WaveArgs(int now, int max)
    {
        this.now = now;
        this.max = max;
    }
}

/// <summary>
/// 主塔血量变化事件参数
/// </summary>
public class MainTowerHpArgs
{
    public int hp;
    public int maxHp;

    public MainTowerHpArgs(int hp, int maxHp)
    {
        this.hp = hp;
        this.maxHp = maxHp;
    }
}

/// <summary>
/// 游戏结束事件参数
/// </summary>
public class GameOverArgs
{
    public int money;
    public bool isWin;

    public GameOverArgs(int money, bool isWin)
    {
        this.money = money;
        this.isWin = isWin;
    }
}
