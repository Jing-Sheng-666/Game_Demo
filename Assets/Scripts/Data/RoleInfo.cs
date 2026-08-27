using UnityEngine;

[CreateAssetMenu(menuName = "GameData/RoleInfo", fileName = "RoleInfo")]
public class RoleInfo : ScriptableObject
{
    public int id;
    public string res;    // 角色预制体路径
    public int atk;       // 攻击力
    public string tips;   // 描述
    public int lockMoney; // 解锁价格
    public int type;      // 角色类型
    public string hitEff; // 受击特效
}
