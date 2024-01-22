using System.Collections;
using CatLib;
using UnityEngine;
using UnityEngine.SceneManagement;

// using Carbon.Services.ResourceService;

// using YooAsset;

namespace Core.Runtime.Game
{
    public class Restart : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            Framework.Instance.OnBeforeDestroy();
            StartCoroutine(RestartApp());
        }

        private IEnumerator RestartApp()
        {
            var scene = SceneManager.LoadScene("Launcher", new LoadSceneParameters(LoadSceneMode.Single));
            while (!scene.isLoaded)
            {
                yield return null;
            }

            //todo 
            // YooAssets.GetAssetsPackage(ResourceSystem.DefaultPackage).ForceUnloadAllAssets();
            Destroy(gameObject);
            Destroy(Framework.Instance.gameObject);
        }
    }
}