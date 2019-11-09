using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TextureLoader : MonoBehaviour {
    public delegate void TextureCallback(Texture tex);
    const string CACHE_PATH = "Cache";
    const float MAX_WAIT_TIME = 10f; // Maximum time to wait for multiple identical textures to load;
    static TextureLoader main;
    public static Dictionary<string, Texture> texList = new Dictionary<string, Texture>();

    public static void LoadTexture(string url, TextureCallback callback = null, Material mat = null, string property = null, Texture tex = null) {
        if (!main) {
            main = FindObjectOfType<TextureLoader>();
        }
        if (main && !string.IsNullOrEmpty(url)) {
            main.PrivateLoadTexture(url, callback, mat, property, tex);
        }
    }

    public void PrivateLoadTexture(string url, TextureCallback callback = null, Material mat = null, string property = null, Texture tex = null) {
        if (texList.ContainsKey(url) && texList[url] != null) {
            FinishLoad(url, callback, mat, property, tex);
            return;
        }
        if (!texList.ContainsKey(url)) {
            Texture tempTex = Resources.Load(url) as Texture;
            if (tempTex) {
                texList[url] = tempTex;
                FinishLoad(url, callback, mat, property, tex);
            } else if (File.Exists(GetCachePath(url))) {
                StartCoroutine(CoLoadTexture(GetCachePath(url), callback, mat, property, tex));
            } else {
                StartCoroutine(CoLoadTexture(url, callback, mat, property, tex, cache: true));
            }
        } else {
            StartCoroutine(CoLoadTexture(url, callback, mat, property, tex));
        }
    }

    IEnumerator CoLoadTexture(string url, TextureCallback callback = null, Material mat = null, string property = null, Texture tex = null, bool cache = false) {
        if (!texList.ContainsKey(url)) {
            texList[url] = null;
            WWW www = new WWW(url);
            yield return www;
            //yield return new WaitForSeconds(3f); // Simulated lag
            if (string.IsNullOrEmpty(www.error)) {
                Texture2D newTex = new Texture2D(0, 0);
                www.LoadImageIntoTexture(newTex);
                texList[url] = newTex;
                if (cache && !File.Exists(GetCachePath(url))) {
                    if (!Directory.Exists(Directory.GetCurrentDirectory() + "/" + CACHE_PATH)) {
                        Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/" + CACHE_PATH);
                    }
                    File.WriteAllBytes(GetCachePath(url), www.bytes);
                }
            } else {
                texList.Remove(url);
            }
        }
        if (mat != null || tex != null || callback != null) {
            if (texList.ContainsKey(url) && texList[url] == null) {
                float startTime = Time.time;
                while (Time.time < startTime + MAX_WAIT_TIME) {
                    yield return new WaitForSeconds(0.05f);
                    if (texList.ContainsKey(url) && texList[url] != null) {
                        break;
                    }
                }
            }
            FinishLoad(url, callback, mat, property, tex);
        }
    }

    void FinishLoad(string url, TextureCallback callback = null, Material mat = null, string property = null, Texture tex = null) {
        if (texList.ContainsKey(url) && texList[url] != null) {
            if (mat) {
                if (property != null) {
                    mat.SetTexture(property, texList[url]);
                } else {
                    mat.mainTexture = texList[url];
                }

            }
            if (tex) {
                tex = texList[url];
            }
            if (callback != null) {
                callback.Invoke(texList[url]);
            }
        }
    }

    // Use this for initialization
    void Start() {
        if (!main) {
            main = this;
        }
    }

    static string GetCachePath(string url) {
        return Directory.GetCurrentDirectory() + "/" + CACHE_PATH + "/" + url.GetHashCode();
    }
}
