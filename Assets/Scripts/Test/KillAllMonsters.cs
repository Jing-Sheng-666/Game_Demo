using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// 测试脚本：秒杀全场怪物，用于验证 怪物对象池 复用是否正常
// 
// 用法：
// 1. 把本脚本挂到场景中任意物体上（建议挂 Main Camera）
// 2. Play 后：
//    K 键 = 手动秒杀当前所有存活怪物（观察死亡→回池）
//    L 键 = 切换自动秒杀模式：每 2 秒自动秒杀一次（持续逼怪物复用）
//
// 观察点：
// - 每一轮 Log 会打印"秒杀了几只"和"场上怪物组件总数"
// - 若某只怪 复用后 定住不动 / 还播着死亡动画 / 位置不对 / 报重复组件错误 = 状态没重置干净
public class KillAllMonsters : MonoBehaviour
{
    private bool autoKill = false;      // 自动秒杀开关
    private int round = 0;              // 已进行的秒杀轮数
    private float timer = 0f;           // 自动模式计时器
    private const float AUTO_INTERVAL = 2f;  // 自动秒杀间隔（秒）

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null)
            return;

        // K：手动秒杀一次
        if (kb.kKey.wasPressedThisFrame)
        {
            KillAll();
        }

        // L：切换自动秒杀
        if (kb.lKey.wasPressedThisFrame)
        {
            autoKill = !autoKill;
            Debug.Log($"自动秒杀模式：{(autoKill ? "已开启（每2秒一次）" : "已关闭")}");
        }

        // 自动模式计时
        if (autoKill)
        {
            timer += Time.deltaTime;
            if (timer >= AUTO_INTERVAL)
            {
                timer = 0f;
                KillAll();
            }
        }
    }

    /// <summary>
    /// 秒杀场景中所有存活的怪物
    /// </summary>
    void KillAll()
    {
        // FindObjectsOfType 默认找不到"失活"的对象 → 只会扫到 正在场上的怪（回池的怪已失活，不会误伤）
        MonsterObject[] all = FindObjectsOfType<MonsterObject>();
        int killed = 0;

        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && !all[i].isDead)
            {
                // 高额伤害走完整流程：受伤 → 死亡(agent禁用/播放死亡动画) → 动画事件 DeadEvent → 回池
                all[i].Wound(99999);
                killed++;
            }
        }

        round++;
        Debug.Log($"[KillAll] 第{round}轮：秒杀{killed}只 | 场上(激活)怪物组件数={all.Length} | 自动模式={(autoKill ? "开" : "关")}");
    }
}
