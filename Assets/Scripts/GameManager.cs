using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get; private set;}
    
    public BoardManager Board;
    public PlayerController Player;
    public TurnManager TurnManager {get; private set;}
    public UIDocument UIDoc;
    
    // UI Elements
    private VisualElement m_HealthBarFill;
    private VisualElement m_DangerIcon; // Referencia a la calavera
    
    private Label m_ScoreLabel;
    private VisualElement m_GameOverPanel;
    private Label m_GameOverMessage;
    private Button m_RestartButton;

    // Game State
    public int MaxFood = 100; 
    private int m_FoodAmount;
    private int m_CurrentLevel = 1;
    private int m_Score = 0;

    // Control del parpadeo
    private Coroutine m_BlinkCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;
        
        // Enlazar UI
        var root = UIDoc.rootVisualElement;
        
        m_HealthBarFill = root.Q<VisualElement>("HealthBarFill");
        m_DangerIcon = root.Q<VisualElement>("DangerIcon"); 
        
        m_ScoreLabel = root.Q<Label>("ScoreLabel");
        m_GameOverPanel = root.Q<VisualElement>("GameOverPanel");
        m_GameOverMessage = root.Q<Label>("GameOverMessage");
        m_RestartButton = root.Q<Button>("RestartButton");
        
        if (m_RestartButton != null) m_RestartButton.clicked += ReloadScene;
        
        StartNewGame();
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnTurnHappen()
    {
        ChangeFood(-1);
    }
    
    public void ChangeFood(int amount)
    {
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
    // ------------------------------------

    public void AddScore(int amount)
    {
        m_Score += amount;
        if (m_ScoreLabel != null) m_ScoreLabel.text = "Score: " + m_Score;
    }

    public void GameOver()
    {
        Player.GameOver();
        if(m_GameOverPanel != null) m_GameOverPanel.style.visibility = Visibility.Visible;
        string finalMessage = "GAME OVER\n\nLevel: " + m_CurrentLevel + "\nTOTAL SCORE: " + m_Score;
        if(m_GameOverMessage != null) m_GameOverMessage.text = finalMessage;
    }

    public void NewLevel()
    {
        Board.Clear();
        Board.Init();
        Player.Spawn(Board, new Vector2Int(1,1));
        m_CurrentLevel++;
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
        
        Board.Clear();
        Board.Init();
        Player.Init();
        Player.Spawn(Board, new Vector2Int(1,1));
    }
}