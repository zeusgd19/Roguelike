using System;
using System.Collections;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [FormerlySerializedAs("Board")] public BoardManager board;
    [FormerlySerializedAs("Player")] public PlayerController player;
    public TurnManager TurnManager { get; private set; }

    [FormerlySerializedAs("UIDoc")] public UIDocument uiDoc;

    public InventoryManager Inventory;

    private Label m_FoodLabel;

    // Fallback UI (UnityEngine.UI) si no hay UIDocument / Label
    private Text m_FoodTextFallback;

    private Label m_DeathLabel;
    private int m_FoodAmount = 20;
    private DeathManager m_DeathManager;
    private int m_CurrentLevel = 1;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;

    // impide que ticks tempranos afecten al estado
    private bool initialized = false;

    private bool m_IsGameActive = false;
    public int MaxFood = 100;
    // UI Elements
    private VisualElement m_HealthBarFill;
    private VisualElement m_DangerIcon;
    private Label m_ScoreLabel;
    private Button m_RestartButton;
    private Coroutine m_BlinkCoroutine;
    private int m_Score = 0;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // aseguramos un valor por defecto
        m_FoodAmount = 20;
        m_DeathManager = new DeathManager();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // inicializamos TurnManager y nos suscribimos
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        
        // inicializamos UI (UI Toolkit si hay uiDoc)
        InitializeUI();
        
        // Enlazar UI
        var root = uiDoc.rootVisualElement;
        
        m_HealthBarFill = root.Q<VisualElement>("HealthBarFill");
        m_DangerIcon = root.Q<VisualElement>("DangerIcon"); 
        m_ScoreLabel = root.Q<Label>("ScoreLabel");
        m_RestartButton = root.Q<Button>("RestartButton");
        
        if (m_RestartButton != null) m_RestartButton.clicked += StartNewGame;

        // empezamos el juego con el flujo existente, pero protegidos contra ticks tempranos
        initialized = false; // bloquea OnTurnHappen hasta que StartNewGame termine
        NewLevel(); // mantengo tu llamada original por compatibilidad con tu flujo
        StartNewGame();
        initialized = true;
    }

    void InitializeUI()
    {
        // Si hay UIDocument, intentar obtener elementos por nombre
        if (uiDoc != null)
        {
            try
            {
                m_GameOverPanel = uiDoc.rootVisualElement.Q<VisualElement>("GameOverPanel");
                if (m_GameOverPanel != null)
                    m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
                m_FoodLabel = uiDoc.rootVisualElement.Q<Label>("FoodLabel");
                m_FoodLabel.text = "Food : " + m_FoodAmount;
                m_DeathLabel = uiDoc.rootVisualElement.Q<Label>("DeathLabel");
                m_DeathLabel.text = "Deaths: " + m_DeathManager.TotalDeaths;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[GameManager] Error leyendo UIDocument: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] uiDoc no asignado en el inspector. Usaré un fallback UI Text.");
        }

        // Si no encontramos la FoodLabel de UI Toolkit, creamos un fallback sencillo con Canvas + Text
        if (m_FoodLabel == null)
        {
            CreateFallbackFoodText();
            Debug.Log("[GameManager] FoodLabel no encontrada -> creado fallback Text en Canvas.");
        }
    }

    void CreateFallbackFoodText()
    {
        // intenta reutilizar un Canvas existente
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("RuntimeCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // crear GameObject Text
        GameObject txtGO = new GameObject("FoodTextFallback");
        txtGO.transform.SetParent(canvas.transform, false);
        m_FoodTextFallback = txtGO.AddComponent<Text>();
        m_FoodTextFallback.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        m_FoodTextFallback.fontSize = 18;
        m_FoodTextFallback.alignment = TextAnchor.UpperLeft;
        m_FoodTextFallback.color = Color.black;
        m_FoodTextFallback.rectTransform.anchorMin = new Vector2(0, 1);
        m_FoodTextFallback.rectTransform.anchorMax = new Vector2(0, 1);
        m_FoodTextFallback.rectTransform.pivot = new Vector2(0, 1);
        m_FoodTextFallback.rectTransform.anchoredPosition = new Vector2(10, -10);

        // inicial text
        m_FoodTextFallback.text = "Food : " + m_FoodAmount;
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnTurnHappen()
    {
        // Ignorar ticks hasta que la inicialización haya finalizado
        if (!initialized) return;
        ChangeFood(-1);

    }

    public void ChangeFood(int amount)
    {
        // Seguridad: si aún no estamos completamente inicializados, ignorar cambios accidentales
        if (!initialized)
        {
            Debug.Log("[GameManager] ChangeFood ignorado porque game no está inicializado todavía.");
            return;
        }

        m_FoodAmount += amount;
        m_FoodAmount = Mathf.Clamp(m_FoodAmount, 0, MaxFood);

        // ---  BARRA Y COLORES ---
        if (m_HealthBarFill != null)
        {
            float percentage = (float)m_FoodAmount / MaxFood * 100f;
            m_HealthBarFill.style.width = Length.Percent(percentage);
            
            // Usamos StyleColor para compatibilidad completa
            if(percentage > 50) 
            {
                // Verde - Todo bien
                m_HealthBarFill.style.backgroundColor = new StyleColor(new Color(0.2f, 0.8f, 0.2f)); 
                StopBlinking(); 
            }
            else if (percentage > 25)
            {
                // Naranja - Advertencia
                m_HealthBarFill.style.backgroundColor = new StyleColor(new Color(1f, 0.64f, 0f)); 
                StopBlinking();
            }
            else 
            {
                // Rojo - CRÍTICO
                m_HealthBarFill.style.backgroundColor = new StyleColor(Color.red);
                
                // Solo parpadea si estamos vivos
                if(m_FoodAmount > 0)
                {
                    StartBlinking(); 
                }
            }
        }
        // --------------------------------

        // actualizar UI Toolkit si existe
        if (m_FoodLabel != null)
        {
            try
            {
                m_FoodLabel.text = "Food : " + m_FoodAmount;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[GameManager] error actualizando m_FoodLabel: " + ex.Message);
            }
        }

        // actualizar fallback UI Text si existe
        if (m_FoodTextFallback != null)
        {
            m_FoodTextFallback.text = "Food : " + m_FoodAmount;
        }

        // comprobaciones GameOver
        if (m_FoodAmount <= 0)
        {
            StopBlinking();
            GameOver();
        }
    }

    // LATIDO 
    void StartBlinking()
    {
        // Si ya está parpadeando, no hacemos nada para no duplicar corrutinas
        if (m_BlinkCoroutine != null) return;
        
        if (m_DangerIcon != null)
        {
            m_BlinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    void StopBlinking()
    {
        if (m_BlinkCoroutine != null)
        {
            StopCoroutine(m_BlinkCoroutine);
            m_BlinkCoroutine = null;
        }
        
        // Aseguramos que se oculte y reseteamos la opacidad
        if (m_DangerIcon != null)
        {
            m_DangerIcon.style.visibility = Visibility.Hidden;
            m_DangerIcon.style.opacity = 1f; // Resetear para la próxima vez
        }
    }

    IEnumerator BlinkRoutine()
    {
        // Hacemos visible el icono antes de empezar a cambiar la opacidad
        m_DangerIcon.style.visibility = Visibility.Visible;
        
        float duration = 0.5f; // Duración de medio latido 

        while (true)
        {
            // De transparente a visible (Fade In)
            for (float t = 0; t < 1f; t += Time.deltaTime / duration)
            {
                m_DangerIcon.style.opacity = t; 
                yield return null;
            }
            
            m_DangerIcon.style.opacity = 1f; // Asegurar tope visible

            // De visible a transparente 
            for (float t = 0; t < 1f; t += Time.deltaTime / duration)
            {
                m_DangerIcon.style.opacity = 1f - t;
                yield return null;
            }
            // Pequeña pausa 
            yield return null; 
        }
    }
    
    private void HandlePlayerDeath()
        {
            m_DeathManager.RegisterDeath();
            m_DeathLabel.text = "Deaths: " + m_DeathManager.TotalDeaths;
        }
    // ------------------------------------

    public void AddScore(int amount)
    {
        m_Score += amount;
        if (m_ScoreLabel != null) m_ScoreLabel.text = "Score: " + m_Score;
    }

    public void GameOver()
    {
        player.GameOver();
        if(m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Visible;
        string finalMessage = "GAME OVER\n\nLevel: " + m_CurrentLevel + "\nTOTAL SCORE: " + m_Score;
        if(m_GameOverMessage != null) m_GameOverMessage.text = finalMessage;
        HandlePlayerDeath();
    }

    public void NewLevel()
    {
        m_CurrentLevel++;

        if (board != null)
        {
            board.Clear();
            board.InitLevel(m_CurrentLevel);
        }
        else
        {
            
        }
        
        if(player != null) player.Spawn(board, new Vector2Int(1,1));
    }

    public void StartNewGame()
    {
        if(m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;
        m_CurrentLevel = 1;
        m_FoodAmount = MaxFood; 
        m_Score = 0;
        
        // Reset inicial UI
        ChangeFood(0); 
        StopBlinking(); 
        
        if(m_ScoreLabel != null) m_ScoreLabel.text = "Score: " + m_Score;
        
        board.Clear();
        board.Init();
        player.Init();
        player.Spawn(board, new Vector2Int(1,1));
    }
}
