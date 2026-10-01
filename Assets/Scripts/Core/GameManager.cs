using System;
using UnityEngine;

namespace CleanSlate.Core
{
    /// <summary>Central authority for the game flow. UI listens to StateChanged instead of polling.</summary>
    public sealed class GameManager : MonoBehaviour
    {
        public enum GameState { Tutorial, Work, DayTransition, GameOver }

        public static GameManager Instance { get; private set; }
        public static event Action<GameState> StateChanged;

        [Header("Runtime State")]
        [SerializeField, Min(1)] private int startingDays = 14;
        [SerializeField, Min(0f)] private float maximumSuspicion = 100f;

        public GameState State { get; private set; } = GameState.Tutorial;
        public int CurrentDay { get; private set; } = 1;
        public int DaysRemaining { get; private set; }
        public float Suspicion { get; private set; }
        public decimal DirtyBalance { get; private set; }
        public decimal CleanBalance { get; private set; }

        public event Action StateValuesChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DaysRemaining = startingDays;
        }

        private void Start() => SetState(GameState.Tutorial);

        public void CompleteTutorial() => SetState(GameState.Work);

        public bool CanPerformWork() => State == GameState.Work;

        public void AddDirtyMoney(decimal amount)
        {
            DirtyBalance = Math.Max(0m, DirtyBalance + amount);
            StateValuesChanged?.Invoke();
        }

        public bool TrySpendDirtyMoney(decimal amount)
        {
            if (amount <= 0m || DirtyBalance < amount || !CanPerformWork()) return false;
            DirtyBalance -= amount;
            StateValuesChanged?.Invoke();
            return true;
        }

        public void AddCleanMoney(decimal amount)
        {
            CleanBalance = Math.Max(0m, CleanBalance + amount);
            StateValuesChanged?.Invoke();
        }

        public void ChangeSuspicion(float amount)
        {
            Suspicion = Mathf.Clamp(Suspicion + amount, 0f, maximumSuspicion);
            StateValuesChanged?.Invoke();
            if (Suspicion >= maximumSuspicion) SetState(GameState.GameOver);
        }

        public void BeginDayTransition()
        {
            if (!CanPerformWork()) return;
            SetState(GameState.DayTransition);
        }

        public void StartNextDay()
        {
            if (State != GameState.DayTransition) return;
            CurrentDay++;
            DaysRemaining--;
            if (DaysRemaining <= 0) { SetState(GameState.GameOver); return; }
            SetState(GameState.Work);
            StateValuesChanged?.Invoke();
        }

        public void SetState(GameState next)
        {
            State = next;
            StateChanged?.Invoke(State);
        }
    }
}
