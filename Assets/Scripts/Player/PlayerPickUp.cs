using System;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUp : MonoBehaviour
{
    private Transform LeftHand;
    private Transform RightHand;

    private void Awake()
    {
        LeftHand = transform.GetChild(2);
        RightHand = transform.GetChild(3);
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (InteractionManager.Ins.GetHoveredObject() == null) return;
        if (context.performed)
        {
            if (context.interaction is UnityEngine.InputSystem.Interactions.HoldInteraction)
            {
                DoHoldAction();
            }
            else if (context.interaction is UnityEngine.InputSystem.Interactions.TapInteraction)
            {
                DoTapAction();
            }
        }
    }

    private void DoTapAction()
    {
        
    }

    private void DoHoldAction()
    {
        throw new NotImplementedException();
    }
}
