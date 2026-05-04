using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.5f; // Độ cao pha nhảy
    public float gravity = -9.81f;  // Trọng lực trái đất

    [Header("Ground Check")]
    public Transform groundCheck;   // Vị trí dưới chân nhân vật
    public float groundDistance = 0.4f; // Bán kính kiểm tra
    public LayerMask groundMask;    // Layer của mặt đất

    [Header("Camera Look (Cinemachine)")]
    public Transform cameraRoot;    // Điểm nhìn (object con của Player)
    public float lookSensitivity = 20f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    // Các biến xử lý vật lý
    private Vector3 velocity; // Vận tốc rơi/nhảy (trục Y)
    private bool isGrounded;  // Xác nhận nhân vật có đang chạm đất không

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        ApplyGravity(); // Phải tính trọng lực trước/cùng lúc với di chuyển
        LookAround();
        MovePlayer();
    }

    // --- CÁC HÀM NHẬN INPUT ---
    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        // Nếu người chơi bấm nút Nhảy (value.isPressed = true) VÀ đang chạm đất
        if (value.isPressed && isGrounded)
        {
            // Công thức vật lý tính vận tốc cần thiết để đạt độ cao jumpHeight
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // --- CÁC HÀM XỬ LÝ LỘGIC ---
    void ApplyGravity()
    {
        // Tạo một hình cầu ảo dưới chân nhân vật để xem có chạm vào layer Ground không
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Nếu chạm đất và đang có vận tốc rơi xuống, reset vận tốc về một số nhỏ để nhân vật bám sát mặt đất
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // Tăng vận tốc rơi theo thời gian (Gia tốc trọng trường)
        velocity.y += gravity * Time.deltaTime;

        // Di chuyển nhân vật theo trục Y (rơi xuống hoặc bay lên)
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

        if (cameraRoot != null)
        {
            cameraRoot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void MovePlayer()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}