using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YooAsset;
using UnityEngine.SceneManagement;
public class OnInitYooAsset : MonoBehaviour
{
    public EPlayMode playMode = EPlayMode.HostPlayMode;
    private ResourcePackage resPackage;
    private string resVersion;
    
    public static List<string> AOTMetaAssemblyNames { get; } = new List<string>()
    {
        //"mscorlib.dll",
        //"System.dll",
        //"System.Core.dll",
        "HotUpdate.dll"
    };
    private static Dictionary<string, byte[]> s_assetDatas = new Dictionary<string, byte[]>();
    public static byte[] GetAssetData(string dllName)
    {
        return s_assetDatas[dllName];
    }
    void Awake()
    {
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        // 初始化资源系统
        YooAssets.Initialize();

        resPackage = YooAssets.CreatePackage("ResPackage");
 
        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(resPackage);

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("The device is not connected to the network");
        }
        else
        {
            StartCoroutine(InitializeHostPlayMode());
        }
    }

    /// <summary>
    /// 联机运行模式
    /// DecryptionServices : 如果资源包在构建的时候有加密，需要提供实现IDecryptionServices接口的实例类。
    /// QueryServices：内置资源查询服务接口。
    /// RemoteServices: 远端服务器查询服务接口。
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitializeHostPlayMode()
    {
        // 注意：GameQueryServices.cs 太空战机的脚本类，详细见StreamingAssetsHelper.cs
        string defaultHostServer = "http://127.0.0.1:8080/Android/";
        string fallbackHostServer = "http://127.0.0.1:8080/Android/";
        //string defaultHostServer = "https://ruijie666.oss-cn-beijing.aliyuncs.com/TestCDN/Windows/V1.0";
        //string fallbackHostServer = "https://ruijie666.oss-cn-beijing.aliyuncs.com/TestCDN/Windows/V1.0";
        var initParameters = new HostPlayModeParameters();
        initParameters.BuildinQueryServices = new GameQueryServices();
        initParameters.DecryptionServices = new FileOffsetDecryption();
        initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
       
        var initOperation = resPackage.InitializeAsync(initParameters);
        yield return initOperation;
        if (initOperation.Status == EOperationStatus.Succeed)
        {
           // CheckUpdate.Instance.SetTextMessage("资源包初始化成功！");
            var operation = resPackage.UpdatePackageVersionAsync();
            yield return operation;
            if (operation.Status == EOperationStatus.Succeed)
            {
                resVersion = operation.PackageVersion;
                StartCoroutine(UpdatePackageManifest());
            }
        }
        else
        {
           // CheckUpdate.Instance.SetTextMessage("资源包初始化失败！");
            Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }
    }
    
    IEnumerator UpdatePackageManifest()
    {
        yield return new WaitForSeconds(0.5f);
       // CheckUpdate.Instance.SetTextMessage("更新资源清单！");
        Debug.Log("更新资源清单！");
        var operation = resPackage.UpdatePackageManifestAsync(resVersion, true);

        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {
            Debug.Log("完成资源Package更新！");
            StartCoroutine(CreatePackageDownloader());
        }
       
    }
    
    IEnumerator CreatePackageDownloader() 
    {
        yield return new WaitForSeconds(0.5f);
        //CheckUpdate.Instance.SetTextMessage("创建补丁下载器！");
        Debug.Log("创建补丁下载器！");

        int downloadingMaxNum = 10;
        int failedTryAgain = 3;
        downloader = resPackage.CreateResourceDownloader(downloadingMaxNum, failedTryAgain);
       
        if (downloader.TotalDownloadCount == 0)
        {
            Debug.Log("没有发现下载文件!");
            StartCoroutine(UpdaterDone());
        }
        else
        {//// 发现新更新文件后，挂起流程系统
            //// 注意：开发者需要在下载前检测磁盘空间不足
            int totalDownloadCount = downloader.TotalDownloadCount;
            long totalDownloadBytes = downloader.TotalDownloadBytes;

            float sizeMb = totalDownloadBytes / 1048576f;
            sizeMb = Mathf.Clamp(sizeMb, 0.1f, float.MaxValue);
            string totalSizeMb = sizeMb.ToString("f1");
    
            //CheckUpdate.Instance.SetTextMessage(
              //  $"Found update patch files, Total count {totalDownloadCount} Total szie {totalDownloadBytes}MB");
            StartCoroutine(BeginDownload());
        }
    }
    ResourceDownloaderOperation downloader;
    IEnumerator BeginDownload() 
    {
        yield return new WaitForSeconds(0.5f);
        //CheckUpdate.Instance.SetTextMessage("开始下载补丁文件！");
        Debug.Log("开始下载补丁文件！");
 
        downloader.OnDownloadErrorCallback = OnDownLoadError;
        downloader.OnDownloadProgressCallback = OnDownLoadProgress;
        downloader.BeginDownload();
        yield return downloader;

        // 检测下载结果
        if (downloader.Status != EOperationStatus.Succeed)
            yield break;
       // CheckUpdate.Instance.SetTextMessage("下载完成!");
        yield return new WaitForSeconds(0.5f);
       // CheckUpdate.Instance.SetTextMessage("清理未使用的缓存文件!");

        var operation = resPackage.ClearUnusedCacheFilesAsync();
        yield return operation;
        if (operation.Status == EOperationStatus.Succeed)
        {
            operation.Completed += CacheFilesCleardCompleted;
        }

    }
    
    private void CacheFilesCleardCompleted(AsyncOperationBase @base)
    {
        resPackage.ForceUnloadAllAssets();

        StartCoroutine(UpdaterDone());
    }

    private void OnDownLoadProgress(int totalDownloadCount, int currentDownloadCount, long totalDownloadBytes, long currentDownloadBytes)
    {
        string descriptionText = string.Format("正在更新，已更新{0}，总更新{1}，已更新大小{2}，总更新大小{3}，更新进度{4}",
            currentDownloadCount.ToString(),
            totalDownloadCount.ToString(),
            GameUtil.GetByteLengthString(currentDownloadBytes),
            GameUtil.GetByteLengthString(totalDownloadBytes),
            downloader.Progress );

        //CheckUpdate.Instance.SetTextMessage(descriptionText);
//Debug.Log(descriptionText);
//Debug.Log(downloader.Progress);
//进度0-1
        //CheckUpdate.Instance.SetProcress(downloader.Progress);
    }
    private void OnDownLoadError(string fileName, string error)
    {
        //CheckUpdate.Instance.SetTextMessage("下载出现问题，请检查网络!");
        StartCoroutine(BeginDownload());
    }
   
    IEnumerator UpdaterDone() 
    {
        yield return new WaitForSeconds(0.2f);
        //CheckUpdate.Instance.SetTextMessage("更新完成！");

        string location = "Assets/AssetBundles/DLLs/HotUpdate.dll.bytes";
        AllAssetsHandle handle = resPackage.LoadAllAssetsAsync<UnityEngine.TextAsset>(location);
        yield return handle;

        foreach (var assetObj in handle.AllAssetObjects)
        {
            if (assetObj.name == "HotUpdate.dll")
            {
                UnityEngine.TextAsset textAsset = assetObj as UnityEngine.TextAsset;
                Assembly hotUpdateAss = Assembly.Load(textAsset.bytes);
            }
        }

        string initScenelocation = "Assets/AssetBundles/GameSources/Scenes/Login";
        var sceneMode = LoadSceneMode.Single;
        bool suspendLoad = false;
        SceneHandle handle1 = resPackage.LoadSceneAsync(initScenelocation, sceneMode, suspendLoad);
        yield return handle1;
        Debug.Log($"Scene name is {handle1.SceneName}");
    }
}
