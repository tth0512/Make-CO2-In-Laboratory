using UnityEditor;
using UnityEngine;

namespace DarkMoorGames.ChannelPackerPro
{
    public sealed class UvDisplay : ScriptableObject
    {
        public Mesh mesh;
        public bool show = true;
        public int uvSetIndex;

        int[] indices = new int[0];
        Vector2[] uv0 = new Vector2[0];
        Vector2[] uv1 = new Vector2[0];
        Vector2[] uv2 = new Vector2[0];
        Vector2[] uv3 = new Vector2[0];
        Vector2[] uv4 = new Vector2[0];
        Vector2[] uv5 = new Vector2[0];
        Vector2[] uv6 = new Vector2[0];
        Vector2[] uv7 = new Vector2[0];

        Vector3[] segments = new Vector3[0];

        public static UvDisplay Create()
        {
            UvDisplay uvDisplay = CreateInstance<UvDisplay>();
            uvDisplay.hideFlags = HideFlags.HideAndDontSave;
            return uvDisplay;
        }
        Vector2[] GetSelectedUvs()
        {
            switch (uvSetIndex)
            {
                case 0:
                    return uv0;
                case 1:
                    return uv1;
                case 2:
                    return uv2;
                case 3:
                    return uv3;
                case 4:
                    return uv4;
                case 5:
                    return uv5;
                case 6:
                    return uv6;
                case 7:
                    return uv7;
                default:
                    return null;
            }
        }
        public void CacheMeshData()
        {
            if (mesh != null)
            {
                uv0 = mesh.uv;
                uv1 = mesh.uv2;
                uv2 = mesh.uv3;
                uv3 = mesh.uv4;
                uv4 = mesh.uv5;
                uv5 = mesh.uv6;
                uv6 = mesh.uv7;
                uv7 = mesh.uv8;

                indices = mesh.triangles;
            }
            else
            {
                uv0 = new Vector2[0];
                uv1 = new Vector2[0];
                uv2 = new Vector2[0];
                uv3 = new Vector2[0];
                uv4 = new Vector2[0];
                uv5 = new Vector2[0];
                uv6 = new Vector2[0];
                uv7 = new Vector2[0];

                indices = new int[0];
                segments = new Vector3[0];
            }
        }
        public void TryDrawWireframeUvs(int imageWidth, int imageHeight, Rect rect)
        {
            if (!show)
                return;

            Vector2[] uvs = GetSelectedUvs();
            if (uvs.Length == 0 || indices.Length == 0)
                return;

            float textureAspect;
            float screenTextureWidth;
            float screenTextureHeight;
            float offsetX;
            float offsetY;

            if (imageHeight > imageWidth)
            {
                textureAspect = (float)imageWidth / imageHeight;

                screenTextureWidth = textureAspect * rect.height;
                screenTextureHeight = rect.height;
                offsetX = (rect.width / 2) - (screenTextureWidth / 2);
                offsetY = 0f;
            }
            else if (imageWidth > imageHeight)
            {
                textureAspect = (float)imageHeight / imageWidth;

                screenTextureWidth = rect.width;
                screenTextureHeight = textureAspect * rect.width;
                offsetX = 0f;
                offsetY = (rect.height / 2) - (screenTextureHeight / 2);
            }
            else
            {
                textureAspect = 1f;
                screenTextureWidth = textureAspect * rect.height;
                screenTextureHeight = rect.height;
                offsetX = (rect.width / 2) - (screenTextureWidth / 2);
                offsetY = 0f;
            }

            if (segments.Length != indices.Length * 2)
            {
                segments = new Vector3[indices.Length * 2];
            }

            int index = 0;
            //Overdrawing lines, should use indices with Handles.DrawLines().
            for (int i = 2; i < uvs.Length * 6; i += 3)
            {
                if (i >= indices.Length)
                    break;

                Vector2 uv1 = uvs[indices[i]];
                Vector2 uv2 = uvs[indices[i - 1]];
                Vector2 uv3 = uvs[indices[i - 2]];

                Vector2 p1 = new Vector2(offsetX + (uv1.x * screenTextureWidth), offsetY + screenTextureHeight - uv1.y * screenTextureHeight);
                Vector2 p2 = new Vector2(offsetX + (uv2.x * screenTextureWidth), offsetY + screenTextureHeight - uv2.y * screenTextureHeight);
                Vector2 p3 = new Vector2(offsetX + (uv3.x * screenTextureWidth), offsetY + screenTextureHeight - uv3.y * screenTextureHeight);

                segments[index] = p1;
                index++;
                segments[index] = p2;
                index++;

                segments[index] = p2;
                index++;
                segments[index] = p3;
                index++;

                segments[index] = p3;
                index++;
                segments[index] = p1;
                index++;
            }
            ColorUtility.TryParseHtmlString(ChannelPackUtility.GetData().Preferences.wireframeColorHtml, out Color color);
            Handles.color = color;
            Handles.DrawLines(segments);
            Handles.color = Color.white;
        }
        public RenderTexture RenderMeshUvsToTexture(int width, int height)
        {
            Vector2[] uvs = GetSelectedUvs();
            if (uvs.Length == 0 || indices.Length == 0)
                return null;
            UvRenderer renderer = ChannelPackUtility.GetData().UvRenderer;
            return renderer.RenderUvFaces(uvs, indices, width, height, Color.black, Color.white);
        }
    }
}
