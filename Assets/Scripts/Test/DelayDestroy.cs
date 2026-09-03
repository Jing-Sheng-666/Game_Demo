using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayDestroy : MonoBehaviour
{
    void Start()
    {
        Invoke("DelayDestroyObj", 3f);
    }
    public void DelayDestroyObj()
    {
        PoolMgr.Instance.PushObject(this.gameObject, "Test/Cube");
    }
}
