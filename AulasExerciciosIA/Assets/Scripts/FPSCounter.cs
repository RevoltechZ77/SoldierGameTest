using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Texto pra mostrar FPS
    private float deltaTime; // Acumula tempo pra calcular FPS
    private int targetFPS = 60; // FPS alvo inicial

    void Start()
    {
        // Garante que o script tá ativo
        if (fpsText == null)
        {
            Debug.LogError("FPSCounter: TextMeshProUGUI não configurado!");
            enabled = false;
            return;
        }

        // Define FPS inicial
        Application.targetFrameRate = targetFPS;
        QualitySettings.vSyncCount = 0; // Desativa VSync
        Debug.Log($"FPSCounter Iniciado: FPS Alvo = {targetFPS}, VSync = {QualitySettings.vSyncCount}");
    }

    void Update()
    {
        // Calcula FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;

        // Atualiza texto
        if (fpsText != null)
        {
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}/{targetFPS}";
        }
        else
        {
            Debug.LogWarning("FPSCounter: TextMeshProUGUI é nulo!");
        }

        // Ajusta FPS alvo com teclas
        if (Input.GetKeyDown(KeyCode.F1))
        {
            targetFPS = 30;
            Application.targetFrameRate = targetFPS;
            Debug.Log($"FPS alvo definido para 30");
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            targetFPS = 60;
            Application.targetFrameRate = targetFPS;
            Debug.Log($"FPS alvo definido para 60");
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            targetFPS = 120;
            Application.targetFrameRate = targetFPS;
            Debug.Log($"FPS alvo definido para 120");
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            targetFPS = 240;
            Application.targetFrameRate = targetFPS;
            Debug.Log($"FPS alvo definido para 240");
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            targetFPS = -1; // Sem limite
            Application.targetFrameRate = targetFPS;
            Debug.Log($"FPS alvo definido para ilimitado");
        }
    }
}