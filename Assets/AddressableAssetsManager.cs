using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AddressableAssetsManager : MonoBehaviour
{
    public static AddressableAssetsManager instance;

    private SceneInstance menuSceneInstance;

    // Dictionary for scene name and its scene
    private IDictionary<string, SceneInstance> sceneParts = new Dictionary<string, SceneInstance>(); 
    private SceneInstance loadedLevel; // currently active loaded level
    private SceneInstance temporaryLevel; // holds the current level thats being loaded
    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            instance = this; 
        } 

        setDeviceSettings();
    }
 
    void Start()
    {      
        // Dont destroy this script on laod
        DontDestroyOnLoad(gameObject);
        InitialiseAndDownload();   
        // StartCoroutine(DownloadData());   
    }

    private void setDeviceSettings(){
        //set device resolution
        Screen.SetResolution(1920, 1080, false);
        //turn of vsync  
        QualitySettings.vSyncCount = 0; 
        //set the target framerate
        Application.targetFrameRate = 120;  
    }

    private async void InitialiseAndDownload(){
        
        // Initialise the system
        var initOp = Addressables.InitializeAsync();
        await initOp.Task;

        // All the assets and levels to download for the initial game load
        var initialGameAssets = new[] { "menu", "level1", "scene1", "scene2", "scene3", "scene4", "scene5", "scene6" };
        var level2 = new [] { "level2" };
        // download the initial game assets
        await DownloadAssets(initialGameAssets);
    }

    private async Task DownloadAssets(string[] initialGameAssets)
    {
        var sizeOps = new Dictionary<string, AsyncOperationHandle<long>>();

        // Determine the download size for all the initialGameAssets
        // in parallel
        foreach (var k in initialGameAssets)
        {
            var op = Addressables.GetDownloadSizeAsync(k);
            sizeOps[k] = op;
        }

        // Wait for all to complete
        await Task.WhenAll(sizeOps.Select(x => x.Value.Task));

      

        // Perform the download sequentally
        foreach (var k in initialGameAssets)
        {
            Debug.Log($"Downloading {k}");
            await Addressables.DownloadDependenciesAsync(k, true).Task;
        }

        Debug.Log("Download finished.");
        // DownloadedMenus();
    }

    private void DownloadedMenus(){
        StartCoroutine(loadSceness());
    }

    IEnumerator loadSceness(){
        Addressables.LoadSceneAsync("menu", LoadSceneMode.Additive).Completed += (asyncHandle) => {
            menuSceneInstance = asyncHandle.Result;
        };

        yield return null;
    }

    //Downloading levels;
    public void DownloadLevel(string level){
        StartCoroutine(StartDownloadingLevel(level));
    }

    IEnumerator StartDownloadingLevel(string level){

        Addressables.DownloadDependenciesAsync(level, true).Completed += (asyncHandle) => {
            
            Addressables.UnloadSceneAsync(loadedLevel, true).Completed += (asyncHandle) => {
                loadedLevel = temporaryLevel;
            };

            Addressables.LoadSceneAsync(level, LoadSceneMode.Additive).Completed += (asyncHandle) => {
                temporaryLevel = asyncHandle.Result;
            };

        };

        yield return null;
    
    }

    // Game Logics
    // Start Button Click
    public void StartGame(){
    }

    public void NewGame(){
        StartCoroutine(StartNewGame());
    }

    IEnumerator StartNewGame(){
        AsyncOperationHandle<SceneInstance> sceneUnload = Addressables.UnloadSceneAsync(menuSceneInstance, true);
        Addressables.LoadSceneAsync("level1", LoadSceneMode.Additive).Completed += (asyncHandle) => {
            loadedLevel = asyncHandle.Result;
        };
        yield return sceneUnload;
    }

    // Use this to load scene using additively
    public void LoadScene(string sceneName){
        StartCoroutine(StartLoadingScene(sceneName));
    }

    public void UnloadScene(string sceneName){
        StartCoroutine(StartUnloadingScene(sceneName));
    }

    IEnumerator StartLoadingScene(string sceneName){
        Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).Completed += (asyncHandle) => {
            // store it in a list
            sceneParts.Add(sceneName, asyncHandle.Result);
        };
        yield return null;
    }

    // unload scenes with auto release handle
    IEnumerator StartUnloadingScene(string sceneName){
        Addressables.UnloadSceneAsync(sceneParts[sceneName], true).Completed += (asyncHandle) => {
            sceneParts.Remove(sceneName);
        };
        yield return null;
    }

}



// code to check the downlaod size
// There should be some error handling here.
// var size = sizeOps.Sum(x => x.Value.Result);
// foreach(var k in sizeOps.Keys)
// {
//     Debug.Log($"Size: {k} - {sizeOps[k].Result} bytes");
// }
// Debug.Log($"Downloading total bytes {size}");


    // private async Task DownloadAssets(string[] initialGameAssets)
    // {
    //     var sizeOps = new Dictionary<string, AsyncOperationHandle<long>>();

    //     // Determine the download size for all the initialGameAssets
    //     // in parallel
    //     foreach (var k in initialGameAssets)
    //     {
    //         var op = Addressables.GetDownloadSizeAsync(k);
    //         sizeOps[k] = op;
    //     }

    //     // Wait for all to complete
    //     await Task.WhenAll(sizeOps.Select(x => x.Value.Task));

    //     // There should be some error handling here.
    //     var size = sizeOps.Sum(x => x.Value.Result);
    //     foreach(var k in sizeOps.Keys)
    //     {
    //         Debug.Log($"Size: {k} - {sizeOps[k].Result} bytes");
    //     }
        
    //     Debug.Log($"Downloading total bytes {size}");

    //     // Perform the download sequentally
    //     foreach (var k in initialGameAssets)
    //     {
    //         Debug.Log($"Downloading {k}");
    //         await Addressables.DownloadDependenciesAsync(k, true).Task;
    //     }

    //     Debug.Log("Download finished.");
    //     DownloadedMenus();
    // }