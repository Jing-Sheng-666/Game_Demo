using UnityEngine;

[CreateAssetMenu(menuName = "GameData/SceneInfo", fileName = "SceneInfo")]
public class SceneInfo : ScriptableObject
{
    public int id;
    public string imgRes;   // 关卡缩略图
    public string name;
    public string tips;
    public string sceneName; // 场景名
    public int money;        // 初始金币
    public int towerHp;      // 主塔血量
}
