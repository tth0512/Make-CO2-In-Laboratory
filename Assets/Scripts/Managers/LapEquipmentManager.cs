using System.Collections.Generic;
using UnityEngine;

public class LabEquipmentManager : MonoBehaviour
{
    [Header("1. KÉO CÁC PREFAB VÀO ĐÂY")]
    [Tooltip("Kéo các Prefab từ cửa sổ Project vào danh sách này.")]
    public List<GameObject> interactablePrefabs = new List<GameObject>();

    [Header("2. Cài đặt Outline")]
    public Color idleOutlineColor = new Color(1f, 1f, 1f, 0.3f);
    public Color highlightOutlineColor = new Color(1f, 1f, 0f, 1f);
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
        Collider[] allObjectsInScene = FindObjectsOfType<Collider>();

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
        Debug.Log($"Detect {obj.name}");
        // Tự động gắn script Outline (giả sử bạn đã có script Outline.cs trong project)
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }

        // Áp dụng thông số
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = idleOutlineColor;
        outline.OutlineWidth = outlineWidth;

        // Tự động chuyển Layer sang Interactable để Raycast dò trúng
        obj.layer = LayerMask.NameToLayer("Interactable");
    }

    //void Update()
    //{
    //    // Liên tục dò tâm (giữ nguyên logic bài trước)
    //    CheckPlayerSight();
    //}

    //private void CheckPlayerSight()
    //{
    //    Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
    //    RaycastHit hit;

    //    if (Physics.Raycast(ray, out hit, raycastRange, interactableLayer))
    //    {
    //        GameObject hitObj = hit.collider.gameObject;

    //        // Kiểm tra xem vật trúng tia có nằm trong danh sách đã quét được lúc Awake không
    //        if (trackedSceneObjects.Contains(hitObj))
    //        {
    //            if (hitObj != currentTarget)
    //            {
    //                SetOutlineHighlight(currentTarget, false);
    //                SetOutlineHighlight(hitObj, true);
    //                currentTarget = hitObj;
    //            }
    //            return;
    //        }
    //    }

    //    if (currentTarget != null)
    //    {
    //        SetOutlineHighlight(currentTarget, false);
    //        currentTarget = null;
    //    }
    //}

    private void SetOutlineHighlight(GameObject obj, bool isHighlighted)
    {
        if (obj == null) return;
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null)
        {
            outline.OutlineColor = isHighlighted ? highlightOutlineColor : idleOutlineColor;
        }
    }
}