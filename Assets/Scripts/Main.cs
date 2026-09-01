using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{

    void Awake()
    {
        //创建一个更新管理器，用来检查AB包的更新
        new GameObject("UpdateMgr").AddComponent<UpdateManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.ShowPanel<BeginPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
