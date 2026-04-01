using System.Collections.Generic;
using UnityEngine;

public class LabEquipmentManager : MonoBehaviour
{
    [Header("1. KÉO CÁC PREFAB VÀO ĐÂY")]
    [Tooltip("Kéo các Prefab từ cửa sổ Project vào danh sách này.")]
    public List<GameObject> interactablePrefabs = new List<GameObject>();
    [Range(1f, 10f)] public float outlineWidth = 4f;

    [Header("3. Cài đặt Raycast")]
    public float raycastRange = 5f;
    public LayerMask interactableLayer; // Nhớ gán layer "Interactable" ở đây

    // Danh sách nội bộ để lưu các đồ vật THỰC TẾ tìm thấy trên Scene
    private List<GameObject> trackedSceneObjects = new List<GameObject>();
    private GameObject currentTarget;

    void Awake()
    {
        // Chạy hàm tự động quét và gắn Outline ngay khi bắt đầu game
        DetectAndSetupObjectsInScene();
    }

    private void DetectAndSetupObjectsInScene()
    {
        // Lấy toàn bộ Collider trên Scene
        Collider[] allObjectsInScene = FindObjectsByType<Collider>(FindObjectsSortMode.None);

        foreach (Collider col in allObjectsInScene)
        {
            Transform currentTransform = col.transform;
            GameObject matchedRootObject = null;

            // Vòng lặp leo ngược lên Object cha (như đã phân tích ở bài trước)
            while (currentTransform != null)
            {
                // Thay vì dùng HashSet, ta dùng hàm kiểm tra chuỗi thông minh này
                if (IsMatchingPrefabName(currentTransform.name))
                {
                    matchedRootObject = currentTransform.gameObject;
                    break;
                }

                currentTransform = currentTransform.parent;
            }

            if (matchedRootObject != null)
            {
                if (!trackedSceneObjects.Contains(matchedRootObject))
                {
                    trackedSceneObjects.Add(matchedRootObject);
                    SetupInitialOutline(matchedRootObject);
                }
            }
        }

        Debug.Log($"[LabManager] Đã tự động quét và nhận diện {trackedSceneObjects.Count} đồ vật (bao gồm cả bản sao 1, 2, 3...).");
    }

    // Hàm phụ trợ: Trái tim của thuật toán nhận diện tên mới
    private bool IsMatchingPrefabName(string sceneObjName)
    {
        // Duyệt qua từng Prefab bạn đã kéo vào Inspector
        foreach (GameObject prefab in interactablePrefabs)
        {
            if (prefab == null) continue;

            string pName = prefab.name;

            // Trường hợp 1: Tên giống y hệt ("BinhNon" == "BinhNon")
            if (sceneObjName == pName)
                return true;

            // Trường hợp 2: Có dấu cách theo sau ("BinhNon (1)", "BinhNon (2)", "BinhNon (Clone)")
            if (sceneObjName.StartsWith(pName + " "))
                return true;

            // Trường hợp 3: Có dấu ngoặc sát tên phòng khi Unity sinh ra dạng "BinhNon(Clone)"
            if (sceneObjName.StartsWith(pName + "("))
                return true;
        }

        // Nếu duyệt hết list Prefab mà không khớp cái nào
        return false;
    }

    private void SetupInitialOutline(GameObject obj)
    {
        // 1. Chuyển Layer vật lý (CoACD) về Default để tối ưu FPS
        obj.layer = LayerMask.NameToLayer("Default");

        // 2. Xóa các target cũ để tránh để lại collider lớn dạng cũ
        Transform[] allChildren = obj.GetComponentsInChildren<Transform>(true);
        for (int i = allChildren.Length - 1; i >= 0; i--)
        {
            Transform child = allChildren[i];
            if (child == null || child == obj.transform) continue;
            if (!child.name.StartsWith("AutoRaycastTarget")) continue;

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // 3. Ưu tiên phần được đánh dấu thủ công, nếu không có thì fallback mesh ngoài cùng
        List<Renderer> targetRenderers = GetPreferredRenderers(obj);
        if (targetRenderers.Count == 0)
        {
            return;
        }

        for (int i = 0; i < targetRenderers.Count; i++)
        {
            Renderer outerRenderer = targetRenderers[i];

            // Outline chỉ nằm ở mesh ngoài cùng
            Outline outline = outerRenderer.GetComponent<Outline>();
            if (outline == null)
            {
                outline = outerRenderer.gameObject.AddComponent<Outline>();
            }

            outline.OutlineWidth = outlineWidth;
            outline.enabled = false;

            if (!TryGetRendererLocalBounds(outerRenderer, out Bounds localBounds))
            {
                continue;
            }

            GameObject raycastTarget = new GameObject("AutoRaycastTarget");
            raycastTarget.transform.SetParent(outerRenderer.transform, false);
            raycastTarget.layer = LayerMask.NameToLayer("Interactable");

            BoxCollider box = raycastTarget.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = raycastTarget.AddComponent<BoxCollider>();
            }

            box.isTrigger = true;
            box.center = localBounds.center;
            box.size = localBounds.size;
        }
    }

    private List<Renderer> GetPreferredRenderers(GameObject root)
    {
        List<Renderer> markedRenderers = GetMarkedRenderers(root);
        if (markedRenderers.Count > 0)
        {
            return markedRenderers;
        }

        return GetOuterRenderers(root);
    }

    private List<Renderer> GetMarkedRenderers(GameObject root)
    {
        OutlinePartMarker[] markers = root.GetComponentsInChildren<OutlinePartMarker>(true);
        List<Renderer> markedRenderers = new List<Renderer>();
        HashSet<Renderer> unique = new HashSet<Renderer>();

        for (int i = 0; i < markers.Length; i++)
        {
            Renderer[] renderers = markers[i].GetComponentsInChildren<Renderer>(true);
            for (int j = 0; j < renderers.Length; j++)
            {
                Renderer renderer = renderers[j];
                if (!IsValidRenderer(renderer)) continue;
                if (!unique.Add(renderer)) continue;

                markedRenderers.Add(renderer);
            }
        }

        return markedRenderers;
    }

    private List<Renderer> GetOuterRenderers(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        List<Renderer> validRenderers = new List<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (!IsValidRenderer(renderer)) continue;
            validRenderers.Add(renderer);
        }

        List<Renderer> outerRenderers = new List<Renderer>();

        foreach (Renderer candidate in validRenderers)
        {
            bool isInsideAnother = false;

            foreach (Renderer other in validRenderers)
            {
                if (candidate == other) continue;
                if (!IsBoundsContained(candidate.bounds, other.bounds)) continue;

                isInsideAnother = true;
                break;
            }

            if (!isInsideAnother)
            {
                outerRenderers.Add(candidate);
            }
        }

        return outerRenderers;
    }

    private bool IsValidRenderer(Renderer renderer)
    {
        if (renderer == null) return false;
        if (renderer.gameObject.name.StartsWith("AutoRaycastTarget")) return false;

        if (renderer is MeshRenderer)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            return meshFilter != null && meshFilter.sharedMesh != null;
        }

        if (renderer is SkinnedMeshRenderer skinned)
        {
            return skinned.sharedMesh != null;
        }

        return false;
    }

    private bool TryGetRendererLocalBounds(Renderer renderer, out Bounds localBounds)
    {
        if (renderer is MeshRenderer)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                localBounds = meshFilter.sharedMesh.bounds;
                return true;
            }
        }
        else if (renderer is SkinnedMeshRenderer skinned)
        {
            if (skinned.sharedMesh != null)
            {
                localBounds = skinned.sharedMesh.bounds;
                return true;
            }
        }

        localBounds = default;
        return false;
    }

    private bool IsBoundsContained(Bounds inner, Bounds outer)
    {
        float innerVolume = inner.size.x * inner.size.y * inner.size.z;
        float outerVolume = outer.size.x * outer.size.y * outer.size.z;

        if (outerVolume <= innerVolume)
        {
            return false;
        }

        Vector3 min = inner.min;
        Vector3 max = inner.max;

        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            if (!outer.Contains(corners[i]))
            {
                return false;
            }
        }

        return true;
    }
}