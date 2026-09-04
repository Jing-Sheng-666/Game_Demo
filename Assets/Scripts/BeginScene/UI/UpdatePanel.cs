using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePanel : BasePanel
{
    [SerializeField] private Slider progressSlider;   
    [SerializeField] private TextMeshProUGUI progressText;

    [SerializeField] private float successShowTime = 3;   // 成功最少展示
    [SerializeField] private float failShowTime = 3;      // 失败最少展示（让用户看清提示）

    private float startTime;

    public override void Init() { }

    private void OnEnable()
    {
        startTime = Time.time;
        UpdateMgr.OnUpdateProgress += OnProgress;
        UpdateMgr.OnUpdateFinished += OnFinished;   // 签名同步改为 bool
    }

    private void OnDisable()
    {
        UpdateMgr.OnUpdateProgress -= OnProgress;
        UpdateMgr.OnUpdateFinished -= OnFinished;
        StopAllCoroutines();
    }

    private void OnProgress(float p)
    {
        if (progressSlider != null) progressSlider.value = p; 
        if (progressText != null) progressText.text = $"更新中 {p * 100:0}%";
    }

    private void OnFinished(bool success)
    {
        // 兜底：成功结束，进度条强制拉满、文字明确
        if (progressSlider != null)
            progressSlider.value = success ? 1f : progressSlider.value;

        if (progressText != null)
            progressText.text = success ? "更新完成" : "更新失败，正在使用本地版本";

        float need = success ? successShowTime : failShowTime;
        float wait = Mathf.Max(0f, need - (Time.time - startTime));
        StartCoroutine(SwitchAfter(wait));
    }


    private IEnumerator SwitchAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 热更（catalog/远程资源）就绪后，再异步加载游戏配置，加载完才进主菜单
        yield return GameDataMgr.Instance.InitAsync();

        UIManager.Instance.HidePanel<UpdatePanel>(false);
        UIManager.Instance.ShowPanel<BeginPanel>();
    }
}
