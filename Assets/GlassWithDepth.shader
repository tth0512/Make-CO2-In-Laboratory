Shader "Custom/GlassWithDepth" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 0.2) // Màu kính, số cuối (Alpha) là độ trong suốt
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.95 // Độ bóng (Kính thì bóng cao)
        _Metallic ("Metallic", Range(0,1)) = 0.1 // Độ kim loại
    }
    SubShader {
        // Khai báo là vật liệu trong suốt
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        // ---- PASS 1: ĐÂY LÀ PHÉP MÀU ----
        // Vẽ một lớp "tàng hình" chỉ để ghi Z-Buffer, chặn mọi Outline phía sau nó
        Pass {
            ZWrite On
            ColorMask 0 
        }

        // ---- PASS 2: VẼ KÍNH BÌNH THƯỜNG ----
        CGPROGRAM
        // Khai báo dùng Standard Lighting và chế độ alpha blend
        #pragma surface surf Standard alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a; // Kích hoạt độ trong suốt
        }
        ENDCG
    }
    FallBack "Standard"
}