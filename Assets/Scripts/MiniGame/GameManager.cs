using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Referencias a Juego")]
    public Pillar[] allPillars;
    public Color[] possibleColors = { Color.red, Color.green, Color.blue, Color.yellow };
    public UIManager uiManager; // Ya no es destructivo si está vacío
    
    [Header("Ajustes y Dificultad")]
    public int maxRounds = 5;
    public float waitBeforeShow = 1f;
    public int maxMistakes = 3;

    // ----- ESTRUCTURAS DE DATOS REQUERIDAS OBLIGATORIAMENTE -----
    private List<Pillar> simonSequence = new List<Pillar>(); 
    private Queue<Pillar> presentationQueue = new Queue<Pillar>(); 
    private Stack<Pillar> playerValidationStack = new Stack<Pillar>(); 

    public enum GameState { Menu, Presenting, WaitingForPlayer, Defeat, Victory }
    public GameState currentState = GameState.Menu;

    private int currentMistakes = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < allPillars.Length; i++)
        {
            if (i < possibleColors.Length)
                allPillars[i].Setup(possibleColors[i]);
            else 
                allPillars[i].Setup(Color.magenta); 
        }

        if (uiManager != null) uiManager.ShowMenu();
        currentState = GameState.Menu;

        // Si no hay UI, auto-iniciamos el juego tras unos segundos para que puedas probar
        if (uiManager == null)
        {
            Debug.Log("--- SISTEMA: Sin UI. Auto-comenzando juego en 2 segundos... ---");
            Invoke(nameof(StartGame), 2f);
        }
    }

    public void StartGame()
    {
        simonSequence.Clear();
        currentMistakes = 0;
        
        if (uiManager != null)
        {
            uiManager.ShowInGame();
            uiManager.UpdateLives(maxMistakes - currentMistakes);
        }
        
        StartNextRound();
    }

    private void StartNextRound()
    {
        if (simonSequence.Count >= maxRounds)
        {
            WinGame();
            return;
        }

        LogInfo("Memoriza la secuencia...");
        currentState = GameState.Presenting;

        Pillar nextPillar = allPillars[Random.Range(0, allPillars.Length)];
        simonSequence.Add(nextPillar);

        StartCoroutine(ShowSequence());
    }

    private IEnumerator ShowSequence()
    {
        yield return new WaitForSeconds(waitBeforeShow);

        presentationQueue.Clear();
        foreach (var p in simonSequence)
        {
            presentationQueue.Enqueue(p);
        }

        while (presentationQueue.Count > 0)
        {
            Pillar current = presentationQueue.Dequeue();
            current.FlashColor();
            yield return new WaitForSeconds(1.2f);
        }

        PreparePlayerTurn();
    }

    private void PreparePlayerTurn()
    {
        LogInfo("¡Tu turno! Replica la secuencia.");
        
        playerValidationStack.Clear();
        for (int i = simonSequence.Count - 1; i >= 0; i--)
        {
            playerValidationStack.Push(simonSequence[i]);
        }

        currentState = GameState.WaitingForPlayer;
    }

    public void PlayerTouchedPillar(Pillar touched)
    {
        if (currentState != GameState.WaitingForPlayer) return;

        touched.FlashColor();

        if (playerValidationStack.Count > 0)
        {
            Pillar expected = playerValidationStack.Pop();

            if (touched == expected)
            {
                if (playerValidationStack.Count == 0)
                {
                    LogInfo("¡Correcto! Siguiente ronda...");
                    currentState = GameState.Presenting; 
                    Invoke(nameof(StartNextRound), 1.5f);
                }
            }
            else
            {
                currentMistakes++;
                if (uiManager != null) uiManager.UpdateLives(maxMistakes - currentMistakes);

                if (currentMistakes >= maxMistakes)
                {
                    LoseGame();
                }
                else
                {
                    LogInfo("¡Error! Te equivocaste de color. Vidas: " + (maxMistakes - currentMistakes));
                    currentState = GameState.Presenting; 
                    Invoke(nameof(RetrySameRound), 1.5f);
                }
            }
        }
    }

    private void RetrySameRound()
    {
        currentState = GameState.Presenting;
        LogInfo("Presta atención de nuevo...");
        StartCoroutine(ShowSequence());
    }

    private void LoseGame()
    {
        currentState = GameState.Defeat;
        if (uiManager != null) uiManager.ShowResult(false);
        Debug.LogWarning("--- SISTEMA: DERROTA ---");
    }

    private void WinGame()
    {
        currentState = GameState.Victory;
        if (uiManager != null) uiManager.ShowResult(true);
        Debug.LogWarning("--- SISTEMA: ¡VICTORIA! ---");
    }

    // Helper para que no falle el juego si no hay GameManager y usar Consola
    private void LogInfo(string msg)
    {
        Debug.Log(msg);
        if (uiManager != null) uiManager.UpdateInfo(msg);
    }
}
