/// <summary>
/// 事件类型 枚举 —— 注释里写清楚每个事件的参数类型，发布/订阅必须一致
/// </summary>
public enum E_EventType
{
    /// <summary>怪物死亡瞬间（发击杀奖励用）——参数：MonsterObject</summary>
    E_Monster_Dead,
    /// <summary>玩家金钱变化（加钱/扣钱后广播最新总额）——参数：int</summary>
    E_Player_MoneyChanged,
    /// <summary>剩余波数变化 ——参数：WaveArgs(now, max)</summary>
    E_Level_WaveChanged,
    /// <summary>主塔血量变化 ——参数：MainTowerHpArgs(hp, maxHp)</summary>
    E_MainTower_HpChanged,
    /// <summary>游戏结束（胜利/失败统一广播，结算UI自己响应）——参数：GameOverArgs(money, isWin)</summary>
    E_Game_Over,
}
