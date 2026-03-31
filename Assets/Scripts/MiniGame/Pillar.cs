using UnityEngine;
using System.Collections;

public class Pillar : MonoBehaviour
{
    private Material mat;
    private Color originalColor = Color.white;
    private Color assignedColor;
    
    [SerializeField] private float flashDuration = 1f;

    public void Setup(Color color)
    {
        // Obtenemos el renderizador y su material instance para no afectar a los demás
        mat = GetComponent<Renderer>().material;
        mat.color = originalColor;
        assignedColor = color;
    }

    public void FlashColor()
    {
        StopAllCoroutines(); // Por si tocan muy rápido
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        mat.color = assignedColor;
        yield return new WaitForSeconds(flashDuration);
        mat.color = originalColor;
    }
}
