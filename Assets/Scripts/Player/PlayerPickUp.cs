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


    public void OnTapInteract()
    {
        var hoveredObject = InteractionManager.Ins.GetHoveredObject();
        if (hoveredObject == null) return;

        hoveredObject.GetComponent<Rigidbody>().isKinematic = true;
        hoveredObject.transform.SetParent(RightHand);
        hoveredObject.transform.localPosition = Vector3.zero;

    }

    public void OnHoldInteract()
    {
        RightHand.GetChild(0).GetComponent<Rigidbody>().isKinematic = false;
        RightHand.GetChild(0).transform.SetParent(null);
    }
}
