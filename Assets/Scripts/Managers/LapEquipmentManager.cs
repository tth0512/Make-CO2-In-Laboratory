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
        // Chỉ setup cho object thật sự có mesh (kể cả mesh nằm ở child)
        if (!TryGetCombinedLocalBounds(obj, out Bounds localBounds))
        {
            return;
        }

        // 1. Tự động gắn script Outline nhưng TẮT nó đi lúc ban đầu
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }

        outline.OutlineWidth = outlineWidth;
        outline.enabled = false;

        // 2. Chuyển Layer vật lý (CoACD) về Default để tối ưu FPS
        obj.layer = LayerMask.NameToLayer("Default");

        // 3. Tự động tạo/cập nhật "Bóng ma" bắt tia Raycast
        Transform existingTarget = obj.transform.Find("AutoRaycastTarget");
        GameObject raycastTarget;

        if (existingTarget == null)
        {
            raycastTarget = new GameObject("AutoRaycastTarget");
            raycastTarget.transform.SetParent(obj.transform);
        }
        else
        {
            raycastTarget = existingTarget.gameObject;
        }

        raycastTarget.transform.localPosition = Vector3.zero;
        raycastTarget.transform.localRotation = Quaternion.identity;
        raycastTarget.transform.localScale = Vector3.one;
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

    private bool TryGetCombinedLocalBounds(GameObject root, out Bounds localBounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);

        bool hasValidMesh = false;
        Bounds worldBounds = default;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;
            if (renderer.gameObject.name == "AutoRaycastTarget") continue;

            bool validRenderer = false;

            if (renderer is MeshRenderer)
            {
                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                validRenderer = meshFilter != null && meshFilter.sharedMesh != null;
            }
            else if (renderer is SkinnedMeshRenderer skinned)
            {
                validRenderer = skinned.sharedMesh != null;
            }

            if (!validRenderer) continue;

            if (!hasValidMesh)
            {
                worldBounds = renderer.bounds;
                hasValidMesh = true;
            }
            else
            {
                worldBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasValidMesh)
        {
            localBounds = default;
            return false;
        }

        Vector3 min = worldBounds.min;
        Vector3 max = worldBounds.max;

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

        Vector3 localMin = root.transform.InverseTransformPoint(corners[0]);
        Vector3 localMax = localMin;

        for (int i = 1; i < corners.Length; i++)
        {
            Vector3 p = root.transform.InverseTransformPoint(corners[i]);
            localMin = Vector3.Min(localMin, p);
            localMax = Vector3.Max(localMax, p);
        }

        localBounds = new Bounds((localMin + localMax) * 0.5f, localMax - localMin);
        return true;
    }
}