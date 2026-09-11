using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolData 
{
    //存储对象
    private Stack<GameObject> dataStack = new Stack<GameObject>();
    //存储使用中的对象
    private List<GameObject> usedList = new List<GameObject>();
    //对象池最大数量
    private int maxNum;

#if UNITY_EDITOR
    //对象的父物体，用来进行布局管理
    private GameObject dataRoot;
#endif
    //获取容器中是否有对象
    public int Count => dataStack.Count;
    //获取容器中使用中的对象
    public int UsedCount => usedList.Count;

    public bool NeedCreate => UsedCount < maxNum;
    public PoolData(GameObject root, string name, GameObject usedObj)
    {
#if UNITY_EDITOR
        dataRoot = new GameObject(name);
        dataRoot.transform.SetParent(root.transform);
#endif

        //创建对象栈时，外面一定是会创建一个对象的，所以这里直接把这个对象存储到使用中的列表中
        PushUsedList(usedObj);

        PoolObj poolObj = usedObj.GetComponent<PoolObj>();
        if(poolObj != null)
        {
            maxNum = poolObj.maxNum;
        }
        else
        {
            Debug.LogError($"对象{usedObj.name}没有挂载PoolObj脚本，无法获取最大数量");
            return;
        }
    }
    public GameObject Pop()
    {
        GameObject obj;
        //取出对象
        if(Count > 0)
        {
            obj = dataStack.Pop();
            usedList.Add(obj);
        }
        else
        {
            obj = usedList[0];
            usedList.RemoveAt(0);
            obj.SetActive(false); // 触发 OnDisable，保持对象状态干净
            usedList.Add(obj);
        }
        
        //激活对象
        obj.SetActive(true);
#if UNITY_EDITOR
        //断开父子关系
        obj.transform.SetParent(null);
#endif

        return obj;
    }
    public void Push(GameObject obj)
    {
        //失活对象
        obj.SetActive(false);
#if UNITY_EDITOR
        //设置父物体
        obj.transform.SetParent(dataRoot.transform);
#endif
        //存储对象
        dataStack.Push(obj);
        //从使用中的列表中移除
        usedList.Remove(obj);
    }

    public void PushUsedList(GameObject obj)
    {
        usedList.Add(obj);
    }

    //清空该池：销毁栈内+使用中的所有实例，以及编辑器下的布局容器
    public void Clear()
    {
        while (dataStack.Count > 0)
        {
            GameObject go = dataStack.Pop();
            if (go != null) Object.Destroy(go);
        }
        foreach (GameObject go in usedList)
            if (go != null) Object.Destroy(go);
        usedList.Clear();
#if UNITY_EDITOR
        if (dataRoot != null) Object.Destroy(dataRoot);
#endif
    }
}

public class PoolMgr : BaseManager<PoolMgr>
{
    //对象池字典
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    //缓存加载到的预制体，同一个key只加载一次，避免引用计数只增不减
    private Dictionary<string, GameObject> prefabDic = new Dictionary<string, GameObject>();
#if UNITY_EDITOR
    //对象池根物体
    private GameObject poolRoot;
#endif
    private PoolMgr()
    {
#if UNITY_EDITOR
        EnsurePoolRoot();   // 创建池根
#endif
    }
    /// <summary>
    /// 获取对象
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject GetObject(string name)
    {
#if UNITY_EDITOR
        EnsurePoolRoot();   //用 poolRoot 前先确保它活着，否则 new PoolData 会碰已销毁对象
#endif
        GameObject obj;
        if (!poolDic.ContainsKey(name) ||
            (poolDic[name].Count == 0 && poolDic[name].NeedCreate))
        {
            //预制体只加载一次，之后从缓存拿引用直接实例化
            if (!prefabDic.TryGetValue(name, out GameObject prefab))
            {
                prefab = AddressablesMgr.Instance.LoadAssetSync<GameObject>(name);
                if (prefab != null)
                    prefabDic.Add(name, prefab);
            }
            if (prefab == null)
            {
                Debug.LogError($"对象{name}不存在");
                return null;
            }

            obj = GameObject.Instantiate(prefab);
            obj.name = name;
            if(!poolDic.ContainsKey(name))
                //创建对象池数据
                poolDic.Add(name, new PoolData(poolRoot, name, obj));
            else
                poolDic[name].PushUsedList(obj);
        }
        else 
        {
            obj = poolDic[name].Pop();
        }
        return obj;
    }

    public void PushObject(GameObject obj, string name)
    {
#if UNITY_EDITOR
        EnsurePoolRoot();   // ★新增：替换原来散落的判空创建，且名字统一为 "Pool"
#endif
        if(!poolDic.ContainsKey(name))
        {
            Debug.LogError($"对象{name}不存在，无法回收");
            GameObject.Destroy(obj);
        }
        else
        {
            poolDic[name].Push(obj);
        }
    }

    public void ClearPool()
    {
        //1. 先销毁各池内的所有实例（含editor下的dataRoot容器）
        foreach (var kv in poolDic)
        {
            kv.Value.Clear();
        }
        poolDic.Clear();

        //2. 再释放加载过的预制体句柄——此时已无实例引用它们，可安全卸载
        foreach (string key in prefabDic.Keys)
        {
            AddressablesMgr.Instance.ReleaseAsset<GameObject>(key);
        }
        prefabDic.Clear();
#if UNITY_EDITOR
        poolRoot = null;
#endif
    }

    //确保池根物体活着（切场景后旧 poolRoot 被销毁时，== null 为 true，会安全重建）
    private void EnsurePoolRoot()
    {
#if UNITY_EDITOR
        if (poolRoot == null)
            poolRoot = new GameObject("PoolRoot");
#endif
    }
}
