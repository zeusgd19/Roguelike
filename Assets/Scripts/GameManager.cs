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
    public InventoryManager Inventory; 

    [FormerlySerializedAs("UIDoc")] public UIDocument uiDoc;
    public HealthBarController healthBar; 
    public int MaxFood = 100;
    private int m_FoodAmount = 50; // Inicia en 50
    
    
    private DeathManager m_DeathManager;
    private int m_CurrentLevel = 1;
    private int m_Score = 0;
    private bool initialized = false;

    private Label m_FoodLabel;
    private Label m_DeathLabel;
    private Label m_ScoreLabel;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    private Button m_RestartButton;
    private Text m_FoodTextFallback;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        m_DeathManager = new DeathManager();
    }

    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        
        InitializeUITexts();

        if (healthBar != null) healthBar.Initialize();
        
        if (uiDoc != null)
        {
            var root = uiDoc.rootVisualElement;
            m_ScoreLabel = root.Q<Label>("ScoreLabel");
            m_RestartButton = root.Q<Button>("RestartButton");
            if (m_RestartButton != null) m_RestartButton.clicked += StartNewGame;
        }

        initialized = false; 
        NewLevel(); 
        StartNewGame();
        initialized = true;
    }

    void InitializeUITexts()
    {
        if (uiDoc != null)
        {
            try
            {
                var root = uiDoc.rootVisualElement;
                m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
                if (m_GameOverPanel != null) m_GameOverMessage = m_GameOverPanel.Q<Label>("GameOverMessage");
                
                m_ScoreLabel = root.Q<Label>("ScoreLabel");
                m_FoodLabel = root.Q<Label>("FoodLabel");
                m_DeathLabel = root.Q<Label>("DeathLabel");
                
                if(m_FoodLabel != null) m_FoodLabel.text = "Food : " + MaxFood;
                if(m_DeathLabel != null) m_DeathLabel.text = "Deaths: " + m_DeathManager.TotalDeaths;
            }
            catch (Exception ex) { Debug.LogWarning("[GameManager] UI Error: " + ex.Message); }
        }

        if (m_FoodLabel == null) CreateFallbackFoodText();
    }

    void OnTurnHappen()
    {
        if (!initialized) return;
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        if (!initialized) return;

        m_FoodAmount += amount;
        m_FoodAmount = Mathf.Clamp(m_FoodAmount, 0, MaxFood);

        if (healthBar != null) 
        {
            healthBar.UpdateHealth(m_FoodAmount, MaxFood);
        }

        if (m_FoodLabel != null) m_FoodLabel.text = "Food : " + m_FoodAmount;
        if (m_FoodTextFallback != null) m_FoodTextFallback.text = "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0) GameOver();
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
        
        string msg = "GAME OVER\n\nLevel: " + m_CurrentLevel + "\nTOTAL SCORE: " + m_Score;
        if(m_GameOverMessage != null) m_GameOverMessage.text = msg;
        
        m_DeathManager.RegisterDeath();
        if(m_DeathLabel != null) m_DeathLabel.text = "Deaths: " + m_DeathManager.TotalDeaths;
    }

    public void StartNewGame()
    {
        if(m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Hidden;
        m_CurrentLevel = 1;
        
        m_FoodAmount = MaxFood; // Empieza con 50
        m_Score = 0;
        
        // Aquí es donde daba el error antes, ahora ya existe la función
        if (healthBar != null) healthBar.StopBlinking();
        
        ChangeFood(0); 
        
        if(m_ScoreLabel != null) m_ScoreLabel.text = "Score: " + m_Score;
        
        board.Clear();
        board.Init();
        player.Init();
        player.Spawn(board, new Vector2Int(1,1));
    }

    public void NewLevel()
    {
        m_CurrentLevel++;
        if (board != null) { board.Clear(); board.InitLevel(m_CurrentLevel); }
        if(player != null) player.Spawn(board, new Vector2Int(1,1));
    }

    void CreateFallbackFoodText()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("RuntimeCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        GameObject txtGO = new GameObject("FoodTextFallback");
        txtGO.transform.SetParent(canvas.transform, false);
        m_FoodTextFallback = txtGO.AddComponent<Text>();
        m_FoodTextFallback.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); 
        m_FoodTextFallback.fontSize = 18;
        m_FoodTextFallback.alignment = TextAnchor.UpperLeft;
        m_FoodTextFallback.color = Color.black;
        m_FoodTextFallback.rectTransform.anchorMin = new Vector2(0, 1);
        m_FoodTextFallback.rectTransform.anchorMax = new Vector2(0, 1);
        m_FoodTextFallback.rectTransform.pivot = new Vector2(0, 1);
        m_FoodTextFallback.rectTransform.anchoredPosition = new Vector2(10, -10);
        m_FoodTextFallback.text = "Food : " + m_FoodAmount;
    }
}