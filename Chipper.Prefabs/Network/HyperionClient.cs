using Chipper.Prefabs.Data;
using Chipper.Prefabs.Data.Response;
using Chipper.Prefabs.Types;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Chipper.Prefabs.Network
{
    /// <summary>
    /// Http client that communicates with the backend `Hyperion` (Rogue Champions Editor Backend)
    /// </summary>
    public class HyperionClient
    {
        private readonly string     m_BaseUrl                   = "http://localhost:8000";
        private readonly string     m_ModulesEndpoint           = "/modules";
        private readonly string     m_CreateTextureEndpoint     = "/textures";
        private readonly string     m_SpritesEndpoint           = "/sprites";
        private readonly string     m_AnimationsEndpoint        = "/animations";
        private readonly string     m_PrefabsEndpoint           = "/prefabs";
        private readonly string     m_GenerateAnimationEndpoint = "/animations/generate";
        private readonly string     m_SyncAssets                = "/assets/sync";
        private readonly HttpClient m_Client = new HttpClient();

        public HyperionClient()
        {
            m_Client.BaseAddress = new System.Uri(m_BaseUrl);
        }

        public async Task<PrefabSimple[]> GetPrefabsAsync()
        {
            var res = await m_Client.GetAsync(m_PrefabsEndpoint);
            var text = await res.Content.ReadAsStringAsync();
            if(res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<PrefabSimple[]>(text);
            return null;
        }

        public async Task<Data.Sprite[]> GetSpritesAsync()
        {
            var res = await m_Client.GetAsync(m_SpritesEndpoint);
            var text = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Data.Sprite[]>(text);
            return null;
        }

        public async Task<Prefab[]> GetPrefabsDetailed()
        {
            var resp = await m_Client.GetAsync(m_PrefabsEndpoint);
            var text = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                return null;

            var prefabs = JsonConvert.DeserializeObject<PrefabSimple[]>(text);
            var detailedPrefabs = new List<Prefab>();
            foreach(var prefab in prefabs)
            {
                var prefabResp = await m_Client.GetAsync(m_PrefabsEndpoint + $"/{prefab.Id}");
                var prefabText = await prefabResp.Content.ReadAsStringAsync();
                if(!resp.IsSuccessStatusCode)
                    continue;
                var p = JsonConvert.DeserializeObject<Prefab>(prefabText);
                detailedPrefabs.Add(p);
            }

            return detailedPrefabs.ToArray();
        }

        public async Task<ModulePartSimple[]> GetModules()
        {
            var res = await m_Client.GetAsync(m_ModulesEndpoint);
            var text = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<ModulePartSimple[]>(text);
            return null;
        }

        public async Task<Data.Response.Animation[]> GetAnimationsAsync()
        {
            var res = await m_Client.GetAsync(m_AnimationsEndpoint);
            var text = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<Data.Response.Animation[]>(text);
            return null;
        }

        public IEnumerator GetPrefabsEnumerable()
        {
            using var request = new UnityWebRequest(m_BaseUrl + m_PrefabsEndpoint, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                yield return JsonConvert.DeserializeObject<PrefabSimple[]>(request.downloadHandler.text);
            }
        }

        public IEnumerator SyncAssets(AssetType type, Asset[] assets)
        {
            using var request = new UnityWebRequest(m_BaseUrl + m_SyncAssets, "POST");
            var json = JsonConvert.SerializeObject(new { Type = type, Assets = assets });
            var bodyRaw = Encoding.UTF8.GetBytes(json);
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
                yield return JsonConvert.DeserializeObject<Asset[]>(request.downloadHandler.text);
            }
        }

        public IEnumerator GetAnimations()
        {
            using var request = new UnityWebRequest(m_BaseUrl + m_AnimationsEndpoint, "GET");
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                yield return JsonConvert.DeserializeObject<Data.Response.Animation[]>(request.downloadHandler.text);
            }
        }

        public IEnumerator CreatePrefabModule(string body)
        {
            using var request = new UnityWebRequest(m_BaseUrl + m_ModulesEndpoint, "POST");
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
                using var request = new UnityWebRequest(m_BaseUrl + m_SpritesEndpoint, "POST");
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

        public IEnumerator GenerateAnimations()
        {
            using var request = new UnityWebRequest(m_BaseUrl + m_GenerateAnimationEndpoint, "POST");
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request?.downloadHandler?.text);
                Debug.Log(request?.error);
            }
            else
            {
                Debug.Log(request?.downloadHandler?.text);
            }
        }

    }
}
