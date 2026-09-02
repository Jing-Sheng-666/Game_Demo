using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    //摄像机要看向的目标对象
    public Transform target;
    //摄像机相对目标对象 在xyz上的偏移位置
    public Vector3 offsetPos;
    public float lookForwardDist = 5f;   // 相机看向角色正前方多远
    public float lookHeightOffset = 1.4f; // 视线点离地面的高度（角色胸/肩高度）

    //移动和旋转速度
    public float moveSpeed;
    public float rotationSpeed;

    private Vector3 targetPos;
    private Quaternion targetRotation;
    private float pitchDeg;

    public Vector3 adsOffset = new Vector3(1f, 2f, -2f); // 贴右肩,Inspector 再微调
    public float adsMoveSpeed = 12f;   // ADS 时相机跟得更紧

    private Vector3 hipOffset;   // 记住调好的腰射机位
    private Vector3 curOffset;
    private bool isAiming;



    void Awake()
    {
        hipOffset = offsetPos;
        curOffset = offsetPos;
    }

    public void SetAim(bool aiming) { isAiming = aiming; }
    
    // Update is called once per frame
    void Update()
    {
        if (target == null)
            return;
        // 平滑切换腰射/肩射机位
        float t = 1 - Mathf.Exp(-8f * Time.deltaTime);//Exp相当于 e^x ,因为指数是负数，所以这个函数的值会随着时间推移逐渐趋近于 0,1-则平滑趋近于1
        curOffset = Vector3.Lerp(curOffset, isAiming ? adsOffset : hipOffset, t);
        float spd = isAiming ? adsMoveSpeed : moveSpeed;

        targetPos = target.position + target.forward * curOffset.z;
        targetPos += Vector3.up * curOffset.y;
        targetPos += target.right * curOffset.x;
        this.transform.position = Vector3.Lerp(this.transform.position, targetPos, spd * Time.deltaTime);


        Vector3 aimDir = Quaternion.AngleAxis(pitchDeg, target.right) * target.forward;
        Vector3 aimPoint = target.position + aimDir * lookForwardDist + Vector3.up * lookHeightOffset;
        targetRotation = Quaternion.LookRotation(aimPoint - this.transform.position);

        //让摄像机不停的向目标角度靠拢
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 设置摄像机看向的目标对象
    /// </summary>
    /// <param name="player"></param>
    public void SetTarget(Transform player)
    {
        target = player;
    }


    public void SetPitch(float p)
    {
        pitchDeg = p;
    }

}
