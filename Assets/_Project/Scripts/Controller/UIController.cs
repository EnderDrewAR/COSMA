using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wires Run / Stop / Step buttons to ProgramExecutor.
/// Displays command messages in MessagePanel.
/// Drives execution-line highlights on ProgramLineView array.
/// Created and wired by COSMAUISceneBuilder at build time.
/// </summary>
public class UIController : MonoBehaviour
{
    [Header("Executor")]
    public ProgramExecutor executor;

    [Header("Control Buttons")]
    public Button stopBtn;
    public Button runBtn;
    public Button stepBtn;

    [Header("Message Panel")]
    public GameObject      messagePanel;
    public TextMeshProUGUI messageText;

    [Header("Program Lines (index 0 = line 01)")]
    public ProgramLineView[] programLines = new ProgramLineView[ProgramModel.SLOT_COUNT];

    // ── Private state ─────────────────────────────────────────────────────────
    private Coroutine _hideCoroutine;
    private int       _activeLine = -1;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (stopBtn != null) stopBtn.onClick.AddListener(OnStop);
        if (runBtn  != null) runBtn .onClick.AddListener(OnRun);
        if (stepBtn != null) stepBtn.onClick.AddListener(OnStep);

        if (executor != null)
        {
            executor.OnLineExecuted   += HandleLineExecuted;
            executor.OnCommandMessage += HandleCommandMessage;
            executor.OnFinished       += HandleFinished;
            executor.OnStopped        += HandleStopped;
        }

        RefreshButtons();
    }

    private void OnDestroy()
    {
        if (executor == null) return;
        executor.OnLineExecuted   -= HandleLineExecuted;
        executor.OnCommandMessage -= HandleCommandMessage;
        executor.OnFinished       -= HandleFinished;
        executor.OnStopped        -= HandleStopped;
    }

    // ── Button click handlers ─────────────────────────────────────────────────

    private void OnStop()
    {
        executor?.Stop();
        RefreshButtons();
    }

    private void OnRun()
    {
        executor?.Run();
        RefreshButtons();
    }

    private void OnStep()
    {
        executor?.Step();
        RefreshButtons();
    }

    // ── Executor event handlers ───────────────────────────────────────────────

    private void HandleLineExecuted(int lineIdx)
    {
        // Clear old highlight
        SetLineHighlight(_activeLine, false);

        // Apply new highlight
        _activeLine = lineIdx;
        SetLineHighlight(_activeLine, true);

        RefreshButtons();
    }

    private void HandleCommandMessage(string msg) => ShowMessage(msg, 2f);

    private void HandleFinished()
    {
        SetLineHighlight(_activeLine, false);
        _activeLine = -1;
        ShowMessage("✓  Программа завершена", 3f);
        RefreshButtons();
    }

    private void HandleStopped()
    {
        SetLineHighlight(_activeLine, false);
        _activeLine = -1;
        ShowMessage("■  Остановлено", 1.5f);
        RefreshButtons();
    }

    // ── Message panel ─────────────────────────────────────────────────────────

    public void ShowMessage(string msg, float duration = 2f)
    {
        if (messageText  != null) messageText.text = msg;
        if (messagePanel != null) messagePanel.SetActive(true);

        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
        _hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    private IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        if (messagePanel != null) messagePanel.SetActive(false);
    }

    // ── Line highlight ────────────────────────────────────────────────────────

    private void SetLineHighlight(int idx, bool on)
    {
        if (idx < 0 || programLines == null || idx >= programLines.Length) return;
        programLines[idx]?.SetExecutionHighlight(on);
    }

    // ── Button interactability ────────────────────────────────────────────────

    private void RefreshButtons()
    {
        if (executor == null) return;
        bool running  = executor.State == ProgramExecutor.ExecState.Running;
        bool idle     = executor.State == ProgramExecutor.ExecState.Idle;
        bool finished = executor.State == ProgramExecutor.ExecState.Finished;

        if (runBtn  != null) runBtn .interactable = !running;
        if (stopBtn != null) stopBtn.interactable = !(idle || finished);
        if (stepBtn != null) stepBtn.interactable = !running;
    }
}
