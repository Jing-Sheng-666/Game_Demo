using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainTest : MonoBehaviour
{
    void Update()
    {
        var input = Keyboard.current.spaceKey.wasPressedThisFrame;
        if (input)
        {
            GameObject obj = PoolMgr.Instance.GetObject("Test/Cube");
            obj.transform.position = Vector3.zero;
        }
    }
}
