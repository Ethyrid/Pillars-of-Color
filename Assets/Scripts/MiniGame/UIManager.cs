using UnityEngine;
using TMPro; // Usando el Nuevo Formato de Textos de Unity

public class UIManager : MonoBehaviour
{
    [Header("Pantalla Central Reutilizable (Pop-Up)")]
    public GameObject centerScreen; 
    public TextMeshProUGUI titleText;
    public GameObject playButton;
    public GameObject returnButton;

    [Header("HUD del Jugador (InGame)")]
    public GameObject hudGroup;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI livesText;

    // Estado incial: Menu
    public void ShowMenu()
    {
        if(centerScreen) centerScreen.SetActive(true);
        if(titleText) {
            titleText.text = "SIMÓN DICE 3D";
            titleText.color = Color.white;
        }
        if(playButton) playButton.SetActive(true);
        if(returnButton) returnButton.SetActive(false);
        
        if(hudGroup) hudGroup.SetActive(false);
    }

    // Al arrancar el juego se oculta el popup
    public void ShowInGame()
    {
        if(centerScreen) centerScreen.SetActive(false);
        if(hudGroup) hudGroup.SetActive(true);
    }

    // Modular: Reutilizamos el mismo panel variando textos y colores
    public void ShowResult(bool isVictory)
    {
        if(centerScreen) centerScreen.SetActive(true);
        
        if(titleText) {
            titleText.text = isVictory ? "¡VICTORIA ÉPICA!" : "¡MEMORIA DE PEZ! (DERROTA)";
            titleText.color = isVictory ? Color.green : Color.red;
        }
        
        if(playButton) playButton.SetActive(false);
        if(returnButton) returnButton.SetActive(true); // Mostrar botón de volver a intentar
        
        if(hudGroup) hudGroup.SetActive(false);
    }

    public void UpdateInfo(string msg)
    {
        if (infoText != null) infoText.text = msg;
    }

    public void UpdateLives(int lives)
    {
        if (livesText != null) livesText.text = "Vidas Vivas: " + lives;
    }
}
