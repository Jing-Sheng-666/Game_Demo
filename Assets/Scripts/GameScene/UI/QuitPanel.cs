using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitPanel : BasePanel
{
    public Button btnContinue;
    public Button btnBack;

    public override void Init()
    {
        btnContinue.onClick.AddListener(() =>
        {
            ContinueGame();
        });

        btnBack.onClick.AddListener(() =>
        {
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            UIManager.Instance.HidePanel<QuitPanel>();
            UIManager.Instance.HidePanel<GamePanel>();
            //清空当前的关卡数据
            GameLevelMgr.Instance.ClearInfo();
            //返回开始场景
            UnityEngine.SceneManagement.SceneManager.LoadScene("BeginScene");
        });
    }

    /// <summary>
    /// 继续游戏 供按钮和ESC关闭时调用共用
    /// </summary>
    public void ContinueGame()
    {
        //恢复时间 再隐藏
        Time.timeScale = 1;
        UIManager.Instance.HidePanel<QuitPanel>();

        //恢复鼠标锁定
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    /// <summary>
    /// 显示自己时做的逻辑
    /// </summary>
    public override void ShowMe()
    {
        base.ShowMe();
        //暂停游戏
        Time.timeScale = 0;
        //解锁鼠标 才能点按钮
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

}
