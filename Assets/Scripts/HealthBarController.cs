using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarController : MonoBehaviour
{
    [Header("Referencias UI")]
    public UIDocument uiDocument;

    [Header("--- SENSACIÓN DE JUEGO (GAME FEEL) ---")]
    [Tooltip("Gradiente de color (Rojo a Verde/Morado)")]
    public Gradient barraDeVida;
    
    [Tooltip("Velocidad barra principal")]
    public float velocidadVerde = 5f; 
    
    [Tooltip("Velocidad barra blanca (fantasma)")]
    public float velocidadFantasma = 2f; 
    
    [Tooltip("Tiempo de espera antes de que baje la fantasma")]
    public float retrasoFantasma = 0.5f;

    // Referencias internas
    private VisualElement m_Container;      
    private VisualElement m_HealthBarFill;  
    private VisualElement m_HealthBarGhost; 
    private VisualElement m_DangerIcon;

    // Variables de estado
    private float m_FillAmount = 1f;   
    private float m_GhostAmount = 1f;  
    private float m_TargetAmount = 1f; 
    
    // Control de daño
    private int m_LastHealthValue; 
    private bool m_EsPrimeraActualizacion = true; 

    private Coroutine m_MainCoroutine;
    private Coroutine m_ImpactShakeCoroutine;
    private Coroutine m_CriticalShakeCoroutine;

    public void Initialize()
    {
        if (uiDocument == null) uiDocument = FindObjectOfType<UIDocument>();
        if (uiDocument == null) return;

        var root = uiDocument.rootVisualElement;
        
        m_Container = root.Q<VisualElement>("HealthBarContainer");
        m_HealthBarFill = root.Q<VisualElement>("HealthBarFill");
        m_HealthBarGhost = root.Q<VisualElement>("HealthBarGhost");
        m_DangerIcon = root.Q<VisualElement>("DangerIcon");

        if (m_DangerIcon != null) 
            m_DangerIcon.style.visibility = Visibility.Hidden;

        m_EsPrimeraActualizacion = true; 
        
        m_FillAmount = 1f;
        m_GhostAmount = 1f;
        m_TargetAmount = 1f;
        ActualizarGraficos();
    }

    public void UpdateHealth(int current, int max)
    {
        if (m_HealthBarFill == null) return;
        if (max == 0) max = 1;

        if (m_EsPrimeraActualizacion)
        {
            m_LastHealthValue = current;
            m_EsPrimeraActualizacion = false;
            
            m_TargetAmount = Mathf.Clamp01((float)current / max);
            m_FillAmount = m_TargetAmount;
            m_GhostAmount = m_TargetAmount;
            ActualizarGraficos();
            
            CheckCriticalMode(current);
            return; 
        }

        float nuevoObjetivo = Mathf.Clamp01((float)current / max);
        
        int damageReceived = m_LastHealthValue - current;
        m_LastHealthValue = current;

        // Solo tiembla si el daño es MAYOR que 1 (ignora caminar)
        if (damageReceived > 1) 
        {
            StartImpactShake();
        }

        m_TargetAmount = nuevoObjetivo;

        if (m_MainCoroutine == null)
            m_MainCoroutine = StartCoroutine(RutinaAnimacion());

        CheckCriticalMode(current);
    }

    private void CheckCriticalMode(int current)
    {
        if (m_TargetAmount < 0.25f && current > 0) 
        {
            StartCriticalMode();
        }
        else 
        {
            StopCriticalMode();
        }
    }

    private IEnumerator RutinaAnimacion()
    {
        float timerRetraso = 0f;

        while (Mathf.Abs(m_FillAmount - m_TargetAmount) > 0.001f || Mathf.Abs(m_GhostAmount - m_TargetAmount) > 0.001f)
        {
            m_FillAmount = Mathf.Lerp(m_FillAmount, m_TargetAmount, Time.deltaTime * velocidadVerde);

            if (m_FillAmount < m_GhostAmount)
            {
                timerRetraso += Time.deltaTime;
                if (timerRetraso > retrasoFantasma)
                {
                    m_GhostAmount = Mathf.Lerp(m_GhostAmount, m_FillAmount, Time.deltaTime * velocidadFantasma);
                }
            }
            else
            {
                m_GhostAmount = m_FillAmount; 
                timerRetraso = 0f;
            }

            ActualizarGraficos();
            yield return null;
        }

        m_FillAmount = m_TargetAmount;
        m_GhostAmount = m_TargetAmount;
        ActualizarGraficos();
        m_MainCoroutine = null;
    }

    private void ActualizarGraficos()
    {
        if (m_HealthBarFill != null) 
            m_HealthBarFill.style.width = Length.Percent(m_FillAmount * 100f);
        
        if (m_HealthBarGhost != null) 
            m_HealthBarGhost.style.width = Length.Percent(m_GhostAmount * 100f);

        if (barraDeVida != null && m_HealthBarFill != null)
        {
            m_HealthBarFill.style.backgroundColor = new StyleColor(barraDeVida.Evaluate(m_FillAmount));
        }
    }

    private void StartImpactShake()
    {
        if (m_CriticalShakeCoroutine != null) return;
        if (m_ImpactShakeCoroutine != null) StopCoroutine(m_ImpactShakeCoroutine);
        m_ImpactShakeCoroutine = StartCoroutine(ImpactShakeRoutine());
    }

    private IEnumerator ImpactShakeRoutine()
    {
        if (m_Container == null) yield break;

        float duration = 0.2f; 
        float magnitude = 8f; 

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            m_Container.style.translate = new Translate(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        m_Container.style.translate = new Translate(0, 0, 0);
    }

    private void StartCriticalMode()
    {
        if (m_DangerIcon != null) m_DangerIcon.style.visibility = Visibility.Visible;
        if (m_CriticalShakeCoroutine == null) m_CriticalShakeCoroutine = StartCoroutine(CriticalShakeRoutine());
    }

    private void StopCriticalMode()
    {
        if (m_DangerIcon != null) m_DangerIcon.style.visibility = Visibility.Hidden;
        if (m_CriticalShakeCoroutine != null)
        {
            StopCoroutine(m_CriticalShakeCoroutine);
            m_CriticalShakeCoroutine = null;
            if (m_Container != null) m_Container.style.translate = new Translate(0, 0, 0);
        }
    }

    // --- ESTA ES LA FUNCIÓN RESTAURADA PARA ARREGLAR EL ERROR ---
    public void StopBlinking()
    {
        // Al reiniciar el juego, simplemente paramos el modo crítico
        StopCriticalMode();
    }
    // -------------------------------------------------------------

    private IEnumerator CriticalShakeRoutine()
    {
        if (m_Container == null) yield break;
        
        float magnitude = 2f; 

        while (true)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            m_Container.style.translate = new Translate(x, y, 0);
            
            if (m_DangerIcon != null)
            {
                m_DangerIcon.style.opacity = Mathf.PingPong(Time.time * 2f, 1f);
            }

            yield return null;
        }
    }
}