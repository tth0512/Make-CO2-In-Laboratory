using UnityEngine;

public abstract class BaseChalkboardState : IState
{
    // Giữ reference (tham chiếu) đến Manager để các State con có thể lấy Data hoặc gọi UI
    protected ChalkboardManager context;

    public BaseChalkboardState(ChalkboardManager context)
    {
        this.context = context;
    }

    // --- 3 Hàm vòng đời bắt buộc phải có ---
    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();

    // --- Các hàm xử lý Event từ UI (để virtual để không bắt buộc ghi đè) ---

    // Gọi khi người chơi click vào 1 trong 4 đáp án (0 = A, 1 = B, 2 = C, 3 = D)
    public virtual void HandleAnswerClick(int answerIndex)
    {
        // Mặc định không làm gì cả
    }

    // Gọi khi người chơi click nút "Thoát" hoặc ấn ESC trên bảng
    public virtual void HandleExitClick()
    {
        // Mặc định không làm gì cả
    }
}