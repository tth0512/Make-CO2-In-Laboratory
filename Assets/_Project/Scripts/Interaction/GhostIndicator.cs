using UnityEngine;

public class GhostIndicator : MonoBehaviour
{
    public GameObject targetObject;
    [Header("Visual Groups")]
    public GameObject whiteGhost;
    public GameObject greenGhost;

    private void Awake()
    {
        // Try to find existing groups if they weren't assigned (e.g. after scene load)
        if (whiteGhost == null) {
            Transform t = transform.Find("WhiteGhost");
            if (t != null) whiteGhost = t.gameObject;
        }
        if (greenGhost == null) {
            Transform t = transform.Find("GreenGhost");
            if (t != null) greenGhost = t.gameObject;
        }
    }

    public void Setup(GameObject target, Material whiteMat, Material greenMat)
    {
        targetObject = target;
        
        // Remove any old children if re-initializing
        for (int i = transform.childCount - 1; i >= 0; i--) {
            if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
            else DestroyImmediate(transform.GetChild(i).gameObject);
        }

        whiteGhost = CreateGhostGroup("WhiteGhost", target, whiteMat);
        greenGhost = CreateGhostGroup("GreenGhost", target, greenMat);
        
        SetState(false);
    }

    private GameObject CreateGhostGroup(string name, GameObject target, Material mat)
    {
        GameObject group = new GameObject(name);
        group.transform.SetParent(this.transform);
        group.transform.localPosition = Vector3.zero;
        group.transform.localRotation = Quaternion.identity;
        group.transform.localScale = Vector3.one;

        MeshFilter[] targetFilters = target.GetComponentsInChildren<MeshFilter>();
        foreach (var filter in targetFilters)
        {
            GameObject ghostPart = new GameObject("Part");
            ghostPart.transform.SetParent(group.transform);
            ghostPart.transform.localPosition = filter.transform.localPosition;
            ghostPart.transform.localRotation = filter.transform.localRotation;
            ghostPart.transform.localScale = filter.transform.localScale;
            
            ghostPart.AddComponent<MeshFilter>().sharedMesh = filter.sharedMesh;
            ghostPart.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }
        return group;
    }

    public void SetState(bool isNear)
    {
        if (whiteGhost != null) whiteGhost.SetActive(!isNear);
        if (greenGhost != null) greenGhost.SetActive(isNear);
    }

    public void Show(bool visible)
    {
        if (gameObject.activeSelf != visible)
            gameObject.SetActive(visible);
    }
}
