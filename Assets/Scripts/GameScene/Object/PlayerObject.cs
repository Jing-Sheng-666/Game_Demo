using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    private Animator animator;

    // 生成的包装类（不用拖 Inspector，代码里 new 就行）
    private PlayerControls controls;

    //1.玩家属性的初始化
    //玩家攻击力
    private int atk;
    //玩家拥有的钱
    public int money;
    //旋转的速度
    private float roundSpeed = 50;
    // 鼠标灵敏度（越小越不灵敏），0.1f ≈ 旧版 Input Manager Mouse X 的默认手感
    [SerializeField] private float mouseSensitivity = 0.1f;


    private float _v, _h;   // 平滑后的速度，跨帧保持

    //持枪对象才有的开火点
    public Transform gunPoint;

    void Awake()
    {
        // 创建输入对象（生成类的实例化）
        controls = new PlayerControls();
    }

    void OnEnable()
    {
        // 激活所有 action，不 Enable 的话输入是"死"的，ReadValue 永远是 0
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
    }

    /// <summary>
    /// 初始化玩家基础属性
    /// </summary>
    /// <param name="atk"></param>
    /// <param name="money"></param>
    public void InitPlayerInfo(int atk, int money)
    {
        this.atk = atk;
        this.money = money;
        //更新界面上钱的数量
        UpdateMoney();
    }

    // Update is called once per frame
    void Update()
    {
        // 移动：Move action 返回离散 Vector2，平滑还是得自己来（3f = 旧版 Gravity/Sensitivity）
        Vector2 move = controls.Player.Move.ReadValue<Vector2>();
        _v = Mathf.MoveTowards(_v, move.y, 3f * Time.deltaTime);
        _h = Mathf.MoveTowards(_h, move.x, 3f * Time.deltaTime);
        animator.SetFloat("VSpeed", _v);
        animator.SetFloat("HSpeed", _h);

        // 鼠标旋转：look.x 是原始像素位移，要乘灵敏度系数
        Vector2 look = controls.Player.Look.ReadValue<Vector2>();
        transform.Rotate(Vector3.up, look.x * mouseSensitivity * roundSpeed * Time.deltaTime);

        // Shift 切瞄准层：Squat 就是你的 Shift 键 action
        if (controls.Player.Squat.IsPressed())
            animator.SetLayerWeight(1, 1);
        else
            animator.SetLayerWeight(1, 0);

        // 触发型：R 翻滚、左键开火
        if (controls.Player.Roll.WasPressedThisFrame())
            animator.SetTrigger("Roll");
        if (controls.Player.Fire.WasPressedThisFrame())
            animator.SetTrigger("Fire");
    }


    //3.攻击动作的不同处理
    /// <summary>
    /// 专门用于处理刀武器攻击动作的伤害检测事件
    /// </summary>
    public void KnifeEvent()
    {
        //进行伤害检测
        Collider[] colliders = Physics.OverlapSphere(this.transform.position + this.transform.forward + this.transform.up, 1, 1 << LayerMask.NameToLayer("Monster"));

        //播放音效
        GameDataMgr.Instance.PlaySound("Music/Knife");

        //暂时无法继续写逻辑了 因为 我们没有怪物对应的脚本
        for (int i = 0; i < colliders.Length; i++)
        {
            //得到碰撞到的对象上的怪物脚本 让其受伤
            MonsterObject monster = colliders[i].gameObject.GetComponent<MonsterObject>();
            if (monster != null && !monster.isDead)
            {
                monster.Wound(this.atk);
                break;
            }
        }
    }

    public void ShootEvent()
    {
        //进行摄像检测 
        //前提是需要有开火点
        RaycastHit[] hits = Physics.RaycastAll(new Ray(gunPoint.position, this.transform.forward), 1000, 1 << LayerMask.NameToLayer("Monster"));

        //播放开枪音效
        GameDataMgr.Instance.PlaySound("Music/Gun");

        for (int i = 0; i < hits.Length; i++)
        {
            //得到对象上的怪物脚本 让其受伤
            //得到碰撞到的对象上的怪物脚本 让其受伤
            MonsterObject monster = hits[i].collider.gameObject.GetComponent<MonsterObject>();
            if (monster != null && !monster.isDead)
            {
                //进行打击特效的创建
                GameObject effObj = Instantiate(Resources.Load<GameObject>(GameDataMgr.Instance.nowSelRole.hitEff));
                effObj.transform.position = hits[i].point;
                effObj.transform.rotation = Quaternion.LookRotation(hits[i].normal);
                Destroy(effObj, 1);

                monster.Wound(this.atk);
                break;
            }
        }
    }


    //4.钱变化的逻辑
    public void UpdateMoney()
    {
        //间接的更新界面上 钱的数量
        UIManager.Instance.GetPanel<GamePanel>().UpdateMoney(money);
    }

    /// <summary>
    /// 提供给外部加钱的方法
    /// </summary>
    /// <param name="money"></param>
    public void AddMoney(int money)
    {
        //加钱
        this.money += money;
        UpdateMoney();
    }
}
