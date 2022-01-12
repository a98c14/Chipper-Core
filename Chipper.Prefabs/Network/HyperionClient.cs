using Chipper.Prefabs.Data;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Chipper.Prefabs.Network
{
    /// <summary>
    /// Http client that communicates with the backend `Hyperion` (Rogue Champions Editor Backend)
    /// </summary>
    public class HyperionClient
    {
        private readonly string     m_BaseUrl = "http://localhost:8000";
        private readonly string     m_CreatePrefabPartEndpoint = "/modules";
        private readonly string     m_CreateTextureEndpoint    = "/textures";
        private readonly string     m_CreateSpriteEndpoint     = "/sprites";
        private readonly HttpClient m_Client                   = new HttpClient();

        public HyperionClient()
        {
            m_Client.BaseAddress = new System.Uri(m_BaseUrl);
        }

        public IEnumerator CreatePrefabModule(string body)
        {
            using var request = new UnityWebRequest(m_BaseUrl + m_CreatePrefabPartEndpoint, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(body);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log("Module created successfully!");
            }
        }

        public IEnumerator UploadSprites(TextureMetaData textureData)
        {
            var form = new WWWForm();
            form.AddField("name", textureData.Name);
            form.AddField("guid", textureData.Guid);
            form.AddBinaryData("texture", textureData.Texture);
            using var textureRequest = UnityWebRequest.Post(m_BaseUrl + m_CreateTextureEndpoint, form);
            yield return textureRequest.SendWebRequest();
            if (textureRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(textureRequest.downloadHandler.text);
                Debug.Log(textureRequest.error);
            }
            else
            {
                var resp = JsonConvert.DeserializeObject<TextureDb>(textureRequest.downloadHandler.text);
                var spriteJson = JsonConvert.SerializeObject(new SpriteReq
                {
                    Sprites = textureData.Sprites,
                    TextureId = resp.Id
                });
                using var request = new UnityWebRequest(m_BaseUrl + m_CreateSpriteEndpoint, "POST");
                var bodyRaw = Encoding.UTF8.GetBytes(spriteJson);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(request.downloadHandler.text);
                    Debug.Log(request.error);
                }
                else
                {
                    Debug.Log(request.downloadHandler.text);
                    Debug.Log("Sprites created successfully!");
                }
            }
        }
        public void LoadPrefabs()
        {

        }
    }
}
