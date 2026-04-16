using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Executes the 13-slot program one line at a time.
/// Supports Run, Step, Pause, Stop.
/// Fires events so UIController can update highlights and messages.
///
/// ARCHITECTURE RULES (do not break):
///   • Reads data ONLY from ProgramModel  — never touches any View.
///   • Writes state ONLY to SatelliteState — never touches any View.
///   • State transitions and OnFinished are owned EXCLUSIVELY by this class.
///   • DoExecuteLine() must NEVER set State or fire OnFinished;
///     only Run-loop and Step() do that.
/// </summary>
public class ProgramExecutor : MonoBehaviour
{
    [Header("References")]
    public ProgramModel   model;
    public SatelliteState satellite;

    [Header("Timing")]
    [Tooltip("Seconds between auto-steps in Run mode")]
    public float stepDelay = 0.6f;

    // Prevent infinite JumpTo loops from hanging Unity
    private const int MAX_ITERATIONS = ProgramModel.SLOT_COUNT * 40;

    // ── State ──────────────────────────────────────────────────────────────────
    public enum ExecState { Idle, Running, Paused, Finished }

    public ExecState State       { get; private set; } = ExecState.Idle;
    public int       CurrentLine { get; private set; } = 0;   // 0-based

    // ── Events ─────────────────────────────────────────────────────────────────
    /// <summary>Fired just before a line is executed. Arg = 0-based index.</summary>
    public event Action<int>    OnLineExecuted;

    /// <summary>Human-readable result string for the last command.</summary>
    public event Action<string> OnCommandMessage;

    public event Action OnFinished;
    public event Action OnStopped;

    private Coroutine _runCoroutine;

    // ── Validation ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (model == null)
            Debug.LogError("[ProgramExecutor] 'model' is not assigned. " +
                           "Assign ProgramModel in the Inspector (or run COSMA → Rebuild UI Scene).");
        if (satellite == null)
            Debug.LogError("[ProgramExecutor] 'satellite' is not assigned. " +
                           "Assign SatelliteState in the Inspector (or run COSMA → Rebuild UI Scene).");
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Start from line 1 (Idle/Finished) or resume from the paused line (Paused).
    /// No-op if already Running.
    /// </summary>
    public void Run()
    {
        if (State == ExecState.Running) return;

        if (State == ExecState.Idle || State == ExecState.Finished)
        {
            CurrentLine = 0;
            satellite?.Reset();
        }

        State = ExecState.Running;
        _runCoroutine = StartCoroutine(RunLoop());
    }

    /// <summary>Freeze at the current line. Call Run() to resume.</summary>
    public void Pause()
    {
        if (State != ExecState.Running) return;
        State = ExecState.Paused;
        StopRunCoroutine();
    }

    /// <summary>Abort, reset satellite, return to line 1.</summary>
    public void Stop()
    {
        StopRunCoroutine();
        State       = ExecState.Idle;
        CurrentLine = 0;
        satellite?.Reset();
        OnStopped?.Invoke();
    }

    /// <summary>
    /// Execute exactly one line.
    /// Idle/Finished → resets first (starts a fresh single-step session).
    /// Paused        → continues from the paused line.
    /// Running       → ignored.
    /// </summary>
    public void Step()
    {
        if (State == ExecState.Running) return;

        // Starting fresh — reset satellite and rewind to line 0
        if (State == ExecState.Idle || State == ExecState.Finished)
        {
            CurrentLine = 0;
            satellite?.Reset();
        }

        State = ExecState.Paused;
        DoExecuteLine();

        // Did the step reach the end of the program?
        if (CurrentLine >= ProgramModel.SLOT_COUNT)
            FinishProgram();
    }

    // ── Internals ──────────────────────────────────────────────────────────────

    private void StopRunCoroutine()
    {
        if (_runCoroutine != null)
        {
            StopCoroutine(_runCoroutine);
            _runCoroutine = null;
        }
    }

    private void FinishProgram()
    {
        State = ExecState.Finished;
        OnFinished?.Invoke();
    }

    // ── Run loop ────────────────────────────────────────────────────────────────

    private IEnumerator RunLoop()
    {
        int iterations = 0;

        while (CurrentLine < ProgramModel.SLOT_COUNT)
        {
            // ── Infinite-loop guard ──
            if (++iterations > MAX_ITERATIONS)
            {
                OnCommandMessage?.Invoke("⚠  Бесконечный цикл обнаружен — выполнение остановлено.");
                break;
            }

            DoExecuteLine();

            // ── Early exits (Pause / Stop called during execution) ──
            if (State != ExecState.Running) yield break;

            // ── Natural end reached inside DoExecuteLine ──
            if (CurrentLine >= ProgramModel.SLOT_COUNT) break;

            // ── Wait before next line ──
            yield return new WaitForSecondsRealtime(stepDelay);

            // ── Check again after the wait (user may Stop/Pause during delay) ──
            if (State != ExecState.Running) yield break;
        }

        // Reached here: either the program ended naturally or the loop guard fired.
        // RunLoop is the ONLY place that may call FinishProgram() in Run mode.
        _runCoroutine = null;
        FinishProgram();
    }

    // ── Core: execute one line ──────────────────────────────────────────────────

    /// <summary>
    /// Execute the slot at <see cref="CurrentLine"/>.
    /// Increments CurrentLine for normal commands;
    /// redirects CurrentLine for JumpTo.
    ///
    /// CONTRACT:
    ///   - NEVER sets State.
    ///   - NEVER fires OnFinished.
    ///   - NEVER calls StopCoroutine.
    ///   Callers (RunLoop and Step) are responsible for all of the above.
    /// </summary>
    private void DoExecuteLine()
    {
        if (model == null)
        {
            Debug.LogError("[ProgramExecutor] DoExecuteLine: model is null.");
            return;
        }

        // Highlight the line BEFORE execution so the orange glow shows immediately
        OnLineExecuted?.Invoke(CurrentLine);

        var data = model.GetSlot(CurrentLine);

        // ── Empty slot: skip silently ──
        if (data.IsEmpty)
        {
            OnCommandMessage?.Invoke($"[{CurrentLine + 1:D2}]  —");
            CurrentLine++;
            return;
        }

        // ── Execute command on satellite ──
        string msg;
        if (satellite != null)
        {
            // Call WITHOUT null-conditional so the 'out' variable is always assigned.
            satellite.ExecuteCommand(data, out msg);
        }
        else
        {
            msg = "(SatelliteState not assigned)";
            Debug.LogWarning("[ProgramExecutor] 'satellite' is null — command was not executed.");
        }

        // Rich message: "[ 03]  Face Earth  →  Facing Earth"
        OnCommandMessage?.Invoke(
            $"[{CurrentLine + 1:D2}]  {data.definition.displayName}  →  {msg}");

        // ── JumpTo: redirect CurrentLine instead of incrementing ──
        if (data.definition.type == CommandType.JumpTo)
        {
            // lineParam is 1-based (displayed as "line 5");
            // convert to 0-based index.
            int target = data.lineParam - 1;

            bool valid = target >= 0 && target < ProgramModel.SLOT_COUNT;
            if (!valid)
            {
                OnCommandMessage?.Invoke(
                    $"[{CurrentLine + 1:D2}]  JumpTo: строка {data.lineParam} вне диапазона " +
                    $"(1–{ProgramModel.SLOT_COUNT}) — пропускаю.");
                CurrentLine++;   // treat as no-op
                return;
            }

            CurrentLine = target;
            return;   // ← do NOT increment
        }

        // ── Normal advance ──
        CurrentLine++;
    }
}
