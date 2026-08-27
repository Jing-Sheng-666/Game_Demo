using UnityEngine;

[CreateAssetMenu(menuName = "GameData/MonsterInfo", fileName = "MonsterInfo")]
public class MonsterInfo : ScriptableObject
{
    public int id;
    public string res;      // 预制体路径
    public string animator; // 动画控制器路径
    public int atk;
    public int moveSpeed;
    public int roundSpeed;
    public int hp;
    public float atkOffset; // 攻击偏移
}
