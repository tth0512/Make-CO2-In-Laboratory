using UnityEngine;

public class InputManager : MonoBehaviour
{
    private static InputManager _instance;
    
    public static InputManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private PlayerControls playerControls;

    public System.Action OnTapInteract;
    public System.Action OnHoldInteract;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // Fix: Destroy this instead of the instance if it exists
            return;
        }
        else
        {
            _instance = this;
        }
        playerControls = new PlayerControls();

        // Bind interaction events
        playerControls.Player.TapInteract.performed += ctx => {
             if (PlayerManager.Instance != null) {
                 var pickup = PlayerManager.Instance.GetComponentInChildren<PlayerPickUp>();
                 if (pickup != null) 
                 {
                     pickup.OnTapInteract();
                 }
             }
        };
        playerControls.Player.HoldInteract.performed += ctx => {
             if (PlayerManager.Instance != null) {
                 var pickup = PlayerManager.Instance.GetComponentInChildren<PlayerPickUp>();
                 if (pickup != null) 
                 {
                     pickup.OnHoldInteract();
                 }
             }
        };
}

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerControls.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMouseDelta()
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    public bool PlayerJumpedThisFrame()
    {
        return playerControls.Player.Jump.WasPressedThisFrame();
    }

}
