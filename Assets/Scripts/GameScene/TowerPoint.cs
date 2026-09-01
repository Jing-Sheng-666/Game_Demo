using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPoint : MonoBehaviour
{
    //造塔点关联的 塔对象
    private GameObject towerObj = null;
    //造塔点关联的 塔的数据
    public TowerInfo nowTowerInfo = null;
    //可以建造的三个塔的ID是多少
    public List<int> chooseIDs;

    /// <summary>
    /// 建造一个塔（按 id 查表，兼容键盘 1/2/3 的旧调用）
    /// </summary>
    public void CreateTower(int id)
    {
        CreateTower(GameDataMgr.Instance.towerInfoList[id - 1]);
    }

    /// <summary>
    /// 建造一个塔（直接传入塔配置，升级用）
    /// </summary>
    public void CreateTower(TowerInfo info)
    {
        // 空保护：满级塔 nextLev 为 null，按下空格时直接忽略
        if (info == null)
            return;

        //如果钱不够 就不用建造了
        if (info.money > GameLevelMgr.Instance.player.money)
            return;

        //扣钱
        GameLevelMgr.Instance.player.AddMoney(-info.money);
        //创建塔
        //先判断之前是否有塔  如果有 就删除
        if (towerObj != null)
        {
            Destroy(towerObj);
            towerObj = null;
        }
        //实例化塔对象
        towerObj = Instantiate(AddressablesMgr.Instance.LoadAssetSync<GameObject>(info.res), this.transform.position, Quaternion.identity);
        //初始化塔
        towerObj.GetComponent<TowerObject>().InitInfo(info);

        //记录当前塔的数据
        nowTowerInfo = info;

        //塔建造完毕 更新游戏界面上的内容
        //注意：nextLev 现在是 SO 引用，判空用 != null
        if (nowTowerInfo.nextLev != null)
        {
            UIManager.Instance.GetPanel<GamePanel>().UpdateSelTower(this);
        }
        else
        {
            UIManager.Instance.GetPanel<GamePanel>().UpdateSelTower(null);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        //如果现在已经有塔了 就没有必要再显示升级界面 或者造塔界面了
        if (nowTowerInfo != null && nowTowerInfo.nextLev == null)
            return;
        UIManager.Instance.GetPanel<GamePanel>().UpdateSelTower(this);
    }

    private void OnTriggerExit(Collider other)
    {
        //如果不希望游戏界面下方的造塔界面显示 直接传空
        UIManager.Instance.GetPanel<GamePanel>().UpdateSelTower(null);
    }
}
