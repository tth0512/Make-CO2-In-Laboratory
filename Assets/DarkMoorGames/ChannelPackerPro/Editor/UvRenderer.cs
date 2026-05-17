using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DarkMoorGames.ChannelPackerPro
{
    public sealed class UvRenderer : ScriptableObject
    {
        GameObject uvGameObject;
        GameObject cameraGameObject;
        Camera camera;

        Mesh mesh;
        Material material;
        RenderTexture outputRenderTexture;

        Scene scene;

        List<Vector3> vertices;
        List<Color> colors;
        List<int> tris;

        public static UvRenderer Create()
        {
            UvRenderer data = CreateInstance<UvRenderer>();
            data.hideFlags = HideFlags.HideAndDontSave;
            data.BuildScene();
            return data;
        }
        void OnDestroy()
        {
            CleanupScene();
        }
        void BuildScene()
        {
            vertices = new List<Vector3>();
            colors = new List<Color>();
            tris = new List<int>();

            mesh = new Mesh
            {
                hideFlags = HideFlags.HideAndDontSave,
                name = "UvRenderer Mesh"
            };
            material = new Material(Shader.Find("Hidden/UnlitVertexColor"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            scene = EditorSceneManager.NewPreviewScene();

            cameraGameObject = EditorUtility.CreateGameObjectWithHideFlags("UvRenderer Camera", HideFlags.HideAndDontSave, typeof(Camera));
            cameraGameObject.transform.position = new Vector3(0f, 0f, -1f);
            cameraGameObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            camera = cameraGameObject.GetComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.allowMSAA = false;
            camera.allowHDR = false;
            camera.scene = scene;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            camera.renderingPath = RenderingPath.Forward;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 3f;
            camera.targetTexture = null;

            uvGameObject = EditorUtility.CreateGameObjectWithHideFlags("UvRenderer GameObejct", HideFlags.HideAndDontSave, typeof(MeshFilter), typeof(MeshRenderer));
            uvGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            uvGameObject.GetComponent<MeshRenderer>().sharedMaterial = material;

            SceneManager.MoveGameObjectToScene(cameraGameObject, scene);
            SceneManager.MoveGameObjectToScene(uvGameObject, scene);
        }
        void CleanupScene()
        {
            DestroyImmediate(mesh);
            DestroyImmediate(material);

            DestroyImmediate(cameraGameObject);
            DestroyImmediate(uvGameObject);

            if (outputRenderTexture)
            {
                outputRenderTexture.Release();
                DestroyImmediate(outputRenderTexture);
            }

            EditorSceneManager.ClosePreviewScene(scene);
        }
        public RenderTexture RenderUvFaces(Vector2[] uvs, int[] indices, int width, int height, Color backgroundColor, Color color)
        {
            vertices.Clear();
            tris.Clear();
            colors.Clear();

            float textureAspect;
            float screenTextureWidth;
            float screenTextureHeight;

            if (width > height)
            {
                textureAspect = (float)width / height;
                screenTextureWidth = textureAspect * height;
                screenTextureHeight = height;
            }
            else if (width > height)
            {
                textureAspect = (float)height / width;
                screenTextureWidth = width;
                screenTextureHeight = textureAspect * width;
            }
            else
            {
                textureAspect = 1f;
                screenTextureWidth = textureAspect * height;
                screenTextureHeight = height;
            }

            for (int i = 2; i < indices.Length; i += 3)
            {
                if (i >= indices.Length)
                    break;

                Vector2 p1 = new Vector2((uvs[indices[i]].x * width) - (width / 2), (uvs[indices[i]].y * height) - (height / 2));
                Vector2 p2 = new Vector2((uvs[indices[i - 1]].x * width) - (width / 2), (uvs[indices[i - 1]].y * height) - (height / 2));
                Vector2 p3 = new Vector2((uvs[indices[i - 2]].x * width) - (width / 2), (uvs[indices[i - 2]].y * height) - (height / 2));

                int vertCount = vertices.Count;

                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);

                colors.Add(color);
                colors.Add(color);
                colors.Add(color);

                tris.Add(vertCount + 2);
                tris.Add(vertCount + 1);
                tris.Add(vertCount + 0);
            }

            mesh.MarkDynamic();

            mesh.Clear(false);
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetIndices(tris, MeshTopology.Triangles, 0);

            mesh.UploadMeshData(false);

            vertices.Clear();
            colors.Clear();
            tris.Clear();

            if (outputRenderTexture == null)
            {
                outputRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    antiAliasing = 1,
                    enableRandomWrite = true,
                    useMipMap = false,
                    autoGenerateMips = false,
                    dimension = UnityEngine.Rendering.TextureDimension.Tex2D
                };
                outputRenderTexture.Create();
                outputRenderTexture.DiscardContents(false, true);
            }
            else if (outputRenderTexture.width != width || outputRenderTexture.height != height)
            {
                DestroyImmediate(outputRenderTexture);

                outputRenderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    antiAliasing = 1,
                    enableRandomWrite = true,
                    useMipMap = false,
                    autoGenerateMips = false,
                    dimension = UnityEngine.Rendering.TextureDimension.Tex2D
                };
                outputRenderTexture.Create();
                outputRenderTexture.DiscardContents(false, true);
            }

            camera.backgroundColor = backgroundColor;
            camera.targetTexture = outputRenderTexture;

            camera.orthographicSize = screenTextureWidth > screenTextureHeight ? screenTextureHeight / 2f : screenTextureWidth / 2f;
            camera.Render();
            camera.targetTexture = null;

            return outputRenderTexture;
        }
    }
}
