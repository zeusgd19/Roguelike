using System;
using System.Collections;
using DefaultNamespace; // NECESARIO para que reconozca InventoryManager
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

    // --- VARIABLES ORIGINALES CONSERVADAS ---
    public InventoryManager Inventory; 
    private Label m_FoodLabel;
    private Text m_FoodTextFallback; // Fallback UI (UnityEngine.UI)
    private Label m_DeathLabel;
    private int m_FoodAmount = 20;
    private DeathManager m_DeathManager;
    private int m_CurrentLevel = 1;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    
    // Impide que ticks tempranos afecten al estado
    private bool initialized = false;
    private bool m_IsGameActive = false;
    public int MaxFood = 100;
    
    // UI Elements
    private Label m_ScoreLabel;
    private Button m_RestartButton;
    private int m_Score = 0;

    // --- NUEVA CONEXIÓN A LA BARRA EXTERNA ---
    [Header("Conexión UI")]
    public HealthBarController healthBar; // Arrastra aquí el nuevo objeto HealthBarSystem

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
        if (uiDoc != null)
        {
            var root = uiDoc.rootVisualElement;
            m_ScoreLabel = root.Q<Label>("ScoreLabel");
            m_RestartButton = root.Q<Button>("RestartButton");
            
            if (m_RestartButton != null) m_RestartButton.clicked += StartNewGame;
        }

        // empezamos el juego con el flujo existente
        initialized = false; 
        NewLevel(); 
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
                var root = uiDoc.rootVisualElement;
                m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
                if (m_GameOverPanel != null)
                    m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
                
                m_FoodLabel = root.Q<Label>("FoodLabel");
                if(m_FoodLabel != null) m_FoodLabel.text = "Food : " + m_FoodAmount;
                
                m_DeathLabel = root.Q<Label>("DeathLabel");
                if(m_DeathLabel != null) m_DeathLabel.text = "Deaths: " + m_DeathManager.TotalDeaths;
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

        // Si no encontramos la FoodLabel de UI Toolkit, creamos un fallback
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
        // Seguridad: si aún no estamos completamente inicializados, ignorar
        if (!initialized) return;

        m_FoodAmount += amount;
        m_FoodAmount = Mathf.Clamp(m_FoodAmount, 0, MaxFood);

        // --- ENVIAR DATOS A LA BARRA EXTERNA ---
        if (healthBar != null)
        {
            healthBar.UpdateHealth(m_FoodAmount, MaxFood);
        }
        // --------------------------------------

        // actualizar UI Toolkit texto si existe
        if (m_FoodLabel != null)
        {
            m_FoodLabel.text = "Food : " + m_FoodAmount;
        }

        // actualizar fallback UI Text si existe
        if (m_FoodTextFallback != null)
        {
            m_FoodTextFallback.text = "Food : " + m_FoodAmount;
        }

        // comprobaciones GameOver
        if (m_FoodAmount <= 0)
        {
            GameOver();
        }
    }

    private void HandlePlayerDeath()
    {
        m_DeathManager.RegisterDeath();
        if(m_DeathLabel != null) m_DeathLabel.text = "Deaths: " + m_DeathManager.TotalDeaths;
    }
    
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
        
        if(player != null) player.Spawn(board, new Vector2Int(1,1));
    }

    public void StartNewGame()
    {
        if(m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;
        m_CurrentLevel = 1;
        m_FoodAmount = MaxFood; 
        m_Score = 0;
        
        // Detener parpadeo en la barra externa
        if (healthBar != null) healthBar.StopBlinking();
        
        // Reset inicial UI
        ChangeFood(0); 
        
        if(m_ScoreLabel != null) m_ScoreLabel.text = "Score: " + m_Score;
        
        board.Clear();
        board.Init();
        player.Init();
        player.Spawn(board, new Vector2Int(1,1));
    }
}