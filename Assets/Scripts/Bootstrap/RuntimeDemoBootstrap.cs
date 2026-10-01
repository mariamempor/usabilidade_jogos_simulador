using System.Collections;
using CleanSlate.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CleanSlate.Bootstrap
{
    /// <summary>
    /// Creates a playable academic prototype at runtime. It deliberately has no Inspector references,
    /// so pressing Play works even in a newly created or otherwise empty Unity scene.
    /// </summary>
    public sealed class RuntimeDemoBootstrap : MonoBehaviour
    {
        private const decimal StartingDirtyBalance = 25000m;
        private const decimal DailyCapacity = 8000m;
        private const float LossRate = .15f;
        private const float SuspicionPerOperation = 7.5f;

        private Text dirtyText;
        private Text cleanText;
        private Text riskText;
        private Text dayText;
        private Text statusText;
        private GameObject tutorialPanel;
        private GameObject dashboardPanel;
        private bool operatedToday;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateIfNeeded()
        {
            if (FindObjectOfType<RuntimeDemoBootstrap>() != null) return;
            new GameObject("CleanSlateRuntimeDemo").AddComponent<RuntimeDemoBootstrap>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsureEventSystem();
            EnsureGameManager();
            CreateInterface();
        }

        private void Start()
        {
            GameManager.Instance.AddDirtyMoney(StartingDirtyBalance);
            RefreshDashboard();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private void EnsureGameManager()
        {
            if (GameManager.Instance != null) return;
            GameObject systems = new GameObject("_Systems");
            systems.AddComponent<GameManager>();
        }

        private void CreateInterface()
        {
            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            GameObject canvasObject = new GameObject("CleanSlateCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            DontDestroyOnLoad(canvasObject);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            tutorialPanel = CreatePanel("Tutorial", canvasObject.transform, new Color(.035f, .055f, .09f, 1f));
            CreateLabel("Clean Slate", tutorialPanel.transform, font, 54, TextAnchor.MiddleCenter, new Vector2(0f, 250f), new Vector2(1000f, 80f));
            CreateLabel("SIMULADOR ACADÊMICO DE GESTÃO DE RISCO", tutorialPanel.transform, font, 22, TextAnchor.MiddleCenter, new Vector2(0f, 185f), new Vector2(1000f, 45f), new Color(.35f, .75f, 1f));
            CreateLabel("1. Colocação: processe valores pela empresa fictícia.\n2. Ocultação: cada operação aumenta o indicador de suspeita.\n3. Integração: o resultado líquido entra no saldo limpo.\n\nMeta do protótipo: testar o loop de decisões e finalizar os dias sem alcançar 100% de risco.", tutorialPanel.transform, font, 28, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(1120f, 330f));
            CreateButton("INICIAR SIMULAÇÃO", tutorialPanel.transform, font, new Vector2(0f, -250f), StartSimulation);

            dashboardPanel = CreatePanel("Dashboard", canvasObject.transform, new Color(.025f, .035f, .06f, 1f));
            CreateLabel("CLEAN SLATE  /  PAINEL OPERACIONAL", dashboardPanel.transform, font, 30, TextAnchor.MiddleLeft, new Vector2(-760f, 470f), new Vector2(1300f, 60f), new Color(.55f, .82f, 1f));
            dirtyText = CreateLabel("", dashboardPanel.transform, font, 28, TextAnchor.MiddleLeft, new Vector2(-760f, 345f), new Vector2(700f, 52f));
            cleanText = CreateLabel("", dashboardPanel.transform, font, 28, TextAnchor.MiddleLeft, new Vector2(-760f, 280f), new Vector2(700f, 52f), new Color(.4f, 1f, .68f));
            riskText = CreateLabel("", dashboardPanel.transform, font, 28, TextAnchor.MiddleLeft, new Vector2(-760f, 215f), new Vector2(700f, 52f), new Color(1f, .67f, .35f));
            dayText = CreateLabel("", dashboardPanel.transform, font, 28, TextAnchor.MiddleLeft, new Vector2(-760f, 150f), new Vector2(700f, 52f));
            statusText = CreateLabel("Selecione uma ação para iniciar o dia.", dashboardPanel.transform, font, 25, TextAnchor.MiddleCenter, new Vector2(250f, 40f), new Vector2(1050f, 180f), new Color(.82f, .88f, .95f));
            CreateButton("PROCESSAR R$ 8.000", dashboardPanel.transform, font, new Vector2(-350f, -280f), ProcessOperation);
            CreateButton("ENCERRAR O DIA", dashboardPanel.transform, font, new Vector2(350f, -280f), EndDay);
            dashboardPanel.SetActive(false);
        }

        private void StartSimulation()
        {
            GameManager.Instance.CompleteTutorial();
            tutorialPanel.SetActive(false);
            dashboardPanel.SetActive(true);
            RefreshDashboard();
        }

        private void ProcessOperation()
        {
            if (!GameManager.Instance.CanPerformWork() || operatedToday) return;
            decimal processed = System.Math.Min(DailyCapacity, GameManager.Instance.DirtyBalance);
            if (processed <= 0m) { statusText.text = "Não há saldo sujo disponível para processar."; return; }
            GameManager.Instance.TrySpendDirtyMoney(processed);
            GameManager.Instance.AddCleanMoney(processed * (decimal)(1f - LossRate));
            GameManager.Instance.ChangeSuspicion(SuspicionPerOperation);
            operatedToday = true;
            statusText.text = "Operação concluída: perda operacional de 15% e risco atualizado.";
            RefreshDashboard();
        }

        private void EndDay()
        {
            if (!GameManager.Instance.CanPerformWork()) return;
            StartCoroutine(EndDaySequence());
        }

        private IEnumerator EndDaySequence()
        {
            GameManager.Instance.BeginDayTransition();
            statusText.text = "22:00 — Encerrando o computador...";
            yield return new WaitForSecondsRealtime(1f);
            statusText.text = "DORMINDO...";
            yield return new WaitForSecondsRealtime(1f);
            GameManager.Instance.StartNextDay();
            operatedToday = false;
            statusText.text = GameManager.Instance.State == GameManager.GameState.GameOver
                ? "Fim da simulação. Reinicie o Play Mode para uma nova partida."
                : "07:00 — Novo dia iniciado. Escolha uma ação.";
            RefreshDashboard();
        }

        private void RefreshDashboard()
        {
            if (GameManager.Instance == null || dirtyText == null) return;
            dirtyText.text = $"SALDO DISPONÍVEL   R$ {GameManager.Instance.DirtyBalance:N0}";
            cleanText.text = $"RESULTADO LÍQUIDO  R$ {GameManager.Instance.CleanBalance:N0}";
            riskText.text = $"SUSPEITA           {GameManager.Instance.Suspicion:0}%";
            dayText.text = $"DIA {GameManager.Instance.CurrentDay}  •  {GameManager.Instance.DaysRemaining} DIAS RESTANTES";
        }

        private static GameObject CreatePanel(string objectName, Transform parent, Color color)
        {
            GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static Text CreateLabel(string value, Transform parent, Font font, int fontSize, TextAnchor alignment, Vector2 position, Vector2 size, Color? color = null)
        {
            GameObject label = new GameObject("Label", typeof(RectTransform), typeof(Text));
            label.transform.SetParent(parent, false);
            RectTransform rect = label.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Text text = label.GetComponent<Text>();
            text.font = font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color ?? Color.white;
            text.text = value;
            return text;
        }

        private static void CreateButton(string caption, Transform parent, Font font, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject(caption, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(500f, 90f);
            buttonObject.GetComponent<Image>().color = new Color(.08f, .37f, .57f, 1f);
            buttonObject.GetComponent<Button>().onClick.AddListener(action);
            Text label = CreateLabel(caption, buttonObject.transform, font, 24, TextAnchor.MiddleCenter, Vector2.zero, rect.sizeDelta);
            label.raycastTarget = false;
        }
    }
}
