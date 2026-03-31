using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    // Bajamos un poco la sensibilidad del Look porque el InputSystem suele ser más fuerte que el Input convencional
    public float lookSpeed = 0.5f; 
    public Transform cameraTransform;

    private CharacterController controller;
    private float verticalRotation = 0f;

    // Variables internas que guardan el estado del Input System
    private Vector2 moveInput;
    private Vector2 lookInput;

    [Header("Interacción")]
    public float interactRange = 5f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --------- NEW INPUT SYSTEM MESSAGES ---------
    // Estos se llaman automáticamente si tu Player tiene el componente "Player Input" en modo "Send Messages"
    
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    // Tu action map tiene un botón "Attack" (Clic izquierdo)
    public void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("SISTEMA: Botón Attack (Clic izquierdo) presionado.");
            TryInteract();
        }
    }

    public void OnInteract(InputValue value)
    {
         if (value.isPressed)
        {
            Debug.Log("SISTEMA: Botón Interact (Tecla E) presionado.");
            TryInteract();
        }
    }
    // ---------------------------------------------

    void Update()
    {
        // Controles de estado de UI
        if (GameManager.Instance != null && GameManager.Instance.currentState != GameManager.GameState.WaitingForPlayer && GameManager.Instance.currentState != GameManager.GameState.Presenting)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return; 
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // --- Movimiento 3D (New Input System) ---
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * speed * Time.deltaTime);

        // --- Rotación (New Input System) ---
        float mouseX = lookInput.x * lookSpeed;
        float mouseY = lookInput.y * lookSpeed;

        transform.Rotate(Vector3.up * mouseX);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -85f, 85f);
        
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    private void TryInteract()
    {
        if (cameraTransform == null) 
        {
            Debug.LogError("ERROR: ¡No has asignado la Cámara (Main Camera) al script PlayerController en el Inspector!");
            return;
        }
        
        // Excepción de seguridad (evita clics si estás en el menu de derrota/victoria)
        if (Cursor.lockState != CursorLockMode.Locked) 
        {
            Debug.LogWarning("No se disparó el rayo porque el ratón no está bloqueado (estás en menú o es turno de Simón).");
            return;
        }

        // Disparamos un Rayo y lo dibujamos en la escena
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        Debug.DrawRay(cameraTransform.position, cameraTransform.forward * interactRange, Color.red, 2f);
        
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
        {
            Debug.Log("Rayo golpeó a: " + hit.collider.gameObject.name);
            Pillar touchedPillar = hit.collider.GetComponent<Pillar>();
            if (touchedPillar != null)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerTouchedPillar(touchedPillar);
                }
            }
            else
            {
                Debug.LogWarning("Golpeó a " + hit.collider.name + " pero NO tiene el script Pillar.");
            }
        }
        else
        {
            Debug.Log("El rayo no golpeó nada en " + interactRange + " metros.");
        }
    }
}
