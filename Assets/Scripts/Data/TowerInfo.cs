using UnityEngine;

/// <summary>
/// 塔数据（ScriptableObject）
/// 创建：Project 窗口右键 -> Create -> GameData/TowerInfo
/// </summary>
[CreateAssetMenu(menuName = "GameData/TowerInfo", fileName = "TowerInfo")]
public class TowerInfo : ScriptableObject
{
    public int id;
    public string name;
    public int money;        // 建造/升级费用
    public int atk;          // 攻击力
    public int atkRange;     // 攻击范围
    public float offsetTime; // 攻击间隔
    public TowerInfo nextLev; // 下一级塔：原来是 int 存 id，现在直接拖下一级塔的 SO 引用
    public string imgRes;    // UI 图标路径
    public string res;       // 预制体路径
    public int atkType;      // 1单体 2群体
    public string eff;       // 攻击特效路径
}
