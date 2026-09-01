using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

    void Awake()
    {
        //创建一个更新管理器，用来检查AB包的更新
        new GameObject("UpdateMgr").AddComponent<UpdateMgr>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.ShowPanel<UpdatePanel>();   // 先显示更新面板
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
