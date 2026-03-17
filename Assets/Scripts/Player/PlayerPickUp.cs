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
        var curObj = RightHand.GetChild(0);
        curObj.GetComponent<Rigidbody>().isKinematic = false;

        float keepY = curObj.position.y;
        Vector3 dropPosition = curObj.position;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Ray centerRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Plane keepHeightPlane = new Plane(Vector3.up, new Vector3(0f, keepY, 0f));

            if (keepHeightPlane.Raycast(centerRay, out float enter))
            {
                dropPosition = centerRay.GetPoint(enter); // đổi X,Z theo tâm cam, giữ Y
            }
        }

        curObj.SetParent(null);
        curObj.position = dropPosition;
    }
}
