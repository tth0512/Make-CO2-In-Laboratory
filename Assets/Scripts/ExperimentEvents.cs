using System;
using UnityEngine;

public static class ExperimentEvents
{
    // Sự kiện 1: Khi phản ứng vừa bắt đầu (không gửi kèm dữ liệu gì)
    public static Action OnReactionStarted;

    // Sự kiện 2: Cập nhật thể tích khí CO2 liên tục (Gửi kèm 1 số float là thể tích)
    public static Action<float> OnGasVolumeUpdated;

    // Sự kiện 3: Khi phản ứng kết thúc
    public static Action OnReactionFinished;
}