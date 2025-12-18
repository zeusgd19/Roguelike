using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class HealthBarController : MonoBehaviour
{
    [Header("Referencias UI")]
    public UIDocument uiDocument;

    [Header("--- CONFIGURACIÓN DE ARTISTAS ---")]
    [Tooltip("Define los colores de la barra (Izquierda=Vacío/Rojo, Derecha=Lleno/Verde)")]
    public Gradient barraDeVida;

    private VisualElement m_HealthBarFill;
    private VisualElement m_DangerIcon;
    private Coroutine m_BlinkCoroutine;

    void Start()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            m_HealthBarFill = root.Q<VisualElement>("HealthBarFill");
            m_DangerIcon = root.Q<VisualElement>("DangerIcon");

            if (m_DangerIcon != null)
                m_DangerIcon.style.visibility = Visibility.Hidden;

            // --- CORRECCIÓN AQUÍ ---
            // Forzamos que la barra empiece al 100% (Verde) y llena visualmente
            // para evitar que se vea roja o vacía mientras carga el juego.
            if (m_HealthBarFill != null)
            {
                UpdateHealth(100, 100); 
            }
        }
    }

    public void UpdateHealth(int current, int max)
    {
        if (m_HealthBarFill == null) return;

        // Evitar división por cero
        if (max == 0) max = 1;

        float percentage01 = Mathf.Clamp01((float)current / max);
        
        m_HealthBarFill.style.width = Length.Percent(percentage01 * 100f);

        Color colorEvaluado = barraDeVida.Evaluate(percentage01);
        m_HealthBarFill.style.backgroundColor = new StyleColor(colorEvaluado);

        if (percentage01 < 0.25f && current > 0) StartBlinking();
        else StopBlinking();
        
        if (current <= 0) StopBlinking();
    }

    private void StartBlinking()
    {
        if (m_BlinkCoroutine != null || m_DangerIcon == null) return;
        m_BlinkCoroutine = StartCoroutine(BlinkRoutine());
    }

    public void StopBlinking()
    {
        if (m_BlinkCoroutine != null)
        {
            StopCoroutine(m_BlinkCoroutine);
            m_BlinkCoroutine = null;
        }
        if (m_DangerIcon != null)
        {
            m_DangerIcon.style.visibility = Visibility.Hidden;
            m_DangerIcon.style.opacity = 1f;
        }
    }

    private IEnumerator BlinkRoutine()
    {
        m_DangerIcon.style.visibility = Visibility.Visible;
        float duration = 0.5f;

        while (true)
        {
            for (float t = 0; t < 1f; t += Time.deltaTime / duration)
            {
                m_DangerIcon.style.opacity = t;
                yield return null;
            }
            for (float t = 0; t < 1f; t += Time.deltaTime / duration)
            {
                m_DangerIcon.style.opacity = 1f - t;
                yield return null;
            }
        }
    }
}