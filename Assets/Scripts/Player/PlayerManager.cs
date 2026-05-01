using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class PlayerManager : MonoBehaviour
{
    private static PlayerManager _instance;
    public static PlayerManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private GameObject player;

    private PlayerController playerController;
    private PlayerPickUp playerPickUp;
    private InteractionManager interactionManager;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance);
        }
        else
        {
            _instance = this;
        }
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player.GetComponent<PlayerController>();
        playerPickUp = player.GetComponent<PlayerPickUp>();
        interactionManager = InteractionManager.Ins;

        //DisablePlayer();
    }

    public void DisablePlayer()
    {
        playerController.enabled = false;
        playerPickUp.enabled = false;
        interactionManager.enabled = false;
    }

    public void EnablePlayer()
    {
        playerController.enabled = true;
        playerPickUp.enabled = true;
        interactionManager.enabled = true;
    }
}
