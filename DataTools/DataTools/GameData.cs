//using LitJson;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using System.Collections;
//using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using UnityEngine.Networking;


namespace DataTools
{
    /// <summary>
    /// V1.5
    /// </summary>
    public class GameData
    {
        /// <summary>
        /// 获取单例对象
        /// </summary>
        /// <returns></returns>
        public static GameData getInstance()
        { return Single<GameData>.getInstance(); }
                                               
        private static T LoadDataInternal<T>(string _path, DataType saveDataType)
        {
            try
            {
                switch (saveDataType)
                {
                    case DataType.Json:
                        return  /*JsonMapper.ToObject<T>(File.ReadAllText(_path))*/JsonConvert.DeserializeObject<T>(File.ReadAllText(_path));
                    case DataType.Xml:
                        return XmlHelper.XmlDeserializeFromFile<T>(_path, System.Text.Encoding.UTF8);
                    case DataType.Binary:
                        BinaryFormatter bf = new BinaryFormatter();
                        FileStream fileStream = File.Open(_path, System.IO.FileMode.Open);
                        return (T)bf.Deserialize(fileStream);
                    default:
                        return default(T);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading file content format,return null\nError: " + e.Message);
                return default(T);
                //throw;
            }
            //finally
            //{
            //    Debug.Log(T);
            //}
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="_path">加载路径</param>
        /// <param name="saveDataType">数据格式</param>
        public static T OnLoadData<T>(string _path ,DataType saveDataType = DataType.Json)
        {
            if (File.Exists(_path)&&File.ReadAllText(_path).Length > 0)
                return LoadDataInternal<T>(_path, saveDataType);
            else
            {
                int Flength = _path.Split('/')[_path.Split('/').Length - 1].Length;
                string Dpath = _path.Substring(0, _path.Length - Flength);
                if (!Directory.Exists(Dpath))
                {
                    //Directory.CreateDirectory(Dpath);
                    Debug.LogError("Error：Path not found!\n"+Dpath);
                }
                else if (!File.Exists(_path))
                {
                    //File.CreateText(_path);
                    Debug.LogError("Error：Path file not found!\n" + Dpath);
                }
                else
                {
                    Debug.LogError("Error：File content is empty!\n" + Dpath);
#if UNITY_EDITOR
                    //Task.Run(async delegate { await Task.Delay(100); LoadDataInternal<T>(_path, saveDataType); });
#endif
                }
                return default(T);
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="_path">写入数据路径</param>
        /// <param name="_datas">数据类型</param>
        /// <param name="saveDataType">保存格式（注：保存方式为三种 json丶xml丶binary,
        /// json需要Newtonsoft.Json.dll,xml不支持直接序列化Dictionary请使用SerialDictionary）</param>
        public static void OnUpdateData<T>(string _path,T _datas, DataType saveDataType = DataType.Json)
        {
            int Flength = _path.Split('/')[_path.Split('/').Length - 1].Length;
            string Dpath = _path.Substring(0, _path.Length - Flength);
            if (!Directory.Exists(Dpath))
                Directory.CreateDirectory(Dpath);

            switch (saveDataType)
            {
                case DataType.Json:
                    JsonSerializerSettings js = new JsonSerializerSettings();
                    js.NullValueHandling = NullValueHandling.Ignore;
                    js.Formatting = Newtonsoft.Json.Formatting.Indented;
                    File.WriteAllText(_path, /*JsonMapper.ToJson(_datas)*/ JsonConvert.SerializeObject(_datas, js));
                    break;
                case DataType.Xml:
                    //List<string[]> lisKV = new List<string[]>();
                    //foreach (KeyValuePair<string,string> item in dicSetData)
                    //    lisKV.Add(new[] { item.Key, item.Value });
                    if (_datas.GetType().Name.Split('`')[0].Equals("Dictionary")/*typeof(System.Collections.IDictionary).IsAssignableFrom(_datas.GetType())*/)
                    {
                        Debug.LogErrorFormat("XmlSerialize does not support Dictionary types,Please use ToSerialDictionary");
                        break;
                    }
                    XmlHelper.XmlSerializeToFile(_datas, _path, System.Text.Encoding.UTF8);
                    break;
                case DataType.Binary:
                    BinaryFormatter binary = new BinaryFormatter();
                    FileStream fileStream = File.Create(_path);
                    binary.Serialize(fileStream, _datas);
                    fileStream.Close();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 加载路径图片
        /// </summary>
        /// <param name="urls">路径</param>
        /// <param name="bSprite">是否转换成精灵图</param>
        /// <returns>return texture or sprite</returns>
        public static UnityEngine.Object LoadingPhoto(string urls, bool bSprite = false)
        {
            if (urls.Length > 1)
            {
                string url =/* Application.streamingAssetsPath +*/ urls;
                Texture2D TT = new Texture2D(1, 1);
                Sprite _sprite = null;
                if (!File.Exists(url))
                {
                    Debug.LogError("路径错误" + url);

                    //byte[] ls_ = System.IO.File.ReadAllBytes(Application.streamingAssetsPath + "/拍照失败.png");
                    //Texture2D _ls = new Texture2D(1, 1);
                    //_ls.LoadImage(ls_);
                    //_ls.Apply();
                    return null;
                }
                else
                {
                    TT.LoadImage(File.ReadAllBytes(url));
                    TT.name = url.Substring(url.LastIndexOf("\\") + 1).Replace(".png", "");
                    TT.Apply();
                    if (bSprite)
                        _sprite = TT.ToSprite();
                    //print("头像读取成功");
                    Resources.UnloadUnusedAssets(); 
                    return bSprite ? (UnityEngine.Object)_sprite : TT;
                }
            }
            else
            {
                Debug.LogError("路径为空");
                return null;
            }
            //System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png"
       
        }
        
        /// <summary>
        /// 保存图片到指定路径下
        /// </summary>
        /// <param name="_tex">图片数据流</param>
        /// <param name="_path">路径</param>
        /// <param name="name">名字+".格式"</param>
        public static void SavePicToFile(byte[] _tex,string _path,string name)
        {
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            File.WriteAllBytes(_path + "/" + name, _tex);//保存
        }

        /// <summary>
        /// 获取文件夹路径下所有类型文件urlByFileType
        /// </summary>
        /// <param name="_path">文件夹路径</param>
        /// <param name="urls">存储路径数组</param>
        /// <param name="_fileType">获取文件类型</param>
        public static void GetAllURL(string _path, ref string[] urls, FileType _fileType = FileType.Picture)
        {
            if (_path == null || _path == string.Empty || !Directory.Exists(_path))
            {
                Debug.LogError("Folder path error\n" + _path);
                return;
            }
            string[] iv = _fileType == FileType.Video ? Directory.GetFiles(_path, "*.mp4") : GetAllURL(_path, new string[] { "*.jpg", "*.png" });
            urls = new string[iv.Length];
            iv.CopyTo(urls,0);
        }

        /// <summary>
        /// 获取文件夹路径下所有类型文件url
        /// </summary>
        /// <param name="dirPath">路径</param>
        /// <param name="searchPatterns">类型数组</param>
        /// <returns>所有类型名</returns>
        public static string[] GetAllURL(string dirPath, params string[] searchPatterns)
        {
            if (!System.IO.File.Exists(dirPath))
                System.IO.Directory.CreateDirectory(dirPath);
            if (searchPatterns.Length <= 0)
                return null;
            else
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dirPath);
                System.IO.FileInfo[][] fis = new System.IO.FileInfo[searchPatterns.Length][];
                int count = 0;
                for (int i = 0; i < searchPatterns.Length; i++)
                {
                    System.IO.FileInfo[] fileInfos = di.GetFiles(searchPatterns[i]);
                    fis[i] = fileInfos;
                    count += fileInfos.Length;
                }
                string[] files = new string[count];
                int n = 0;
                for (int i = 0; i <= fis.GetUpperBound(0); i++)
                {
                    for (int j = 0; j < fis[i].Length; j++)
                    {
                        string temp = fis[i][j].FullName;
                        files[n] = temp;
                        n++;
                    }
                }
                return files;
            }
        }

        /// <summary>
        /// 加载指定文件夹下所有图片
        /// </summary>
        /// <typeparam name="T">图片类型</typeparam>
        /// <param name="_path">文件夹路径</param>
        /// <returns>所有类型图片</returns>
        public static T[] LoadAllPicByFolder<T>(string _path) where T : UnityEngine.Object
        {
            if (!typeof(T).Name.Contains("Sprite") && !typeof(T).Name.Contains("Texture2D"))
            {
                Debug.LogError("Error in type\nType must be Sprite or Texture2D");
                return null;
            }
            string[] allpicUrl = null;
            GetAllURL(_path, ref allpicUrl, FileType.Picture);
            T[] pic = new T[allpicUrl.Length];
            for (int i = 0; i < allpicUrl.Length; i++)
                pic[i] = LoadingPhoto(allpicUrl[i], typeof(T).Name.Contains("Sprite")) as T;
            return pic;
        }

        /// <summary>
        /// 异步加载指定文件夹路径下所有图片数据
        /// </summary>
        /// <param name="_path">文件夹路径</param>
        /// <param name="_complete">下载完成后委托事件，传入数据流及其原始文件名</param>
        /// <returns>异步下载消息</returns>
        public static Async LoadAsyncAllPicByFolder(string _path, System.Action<AsyncData> _complete)
        {
            string[] allpicUrl = null;
            GameData.GetAllURL(_path, ref allpicUrl, FileType.Picture);
            Async async = new GameObject("LoadAsync").AddComponent<Async>();
            async.StartCoroutine(Async.DownloadAllPicByUrls(allpicUrl, async));
            async._loadCompleted += _complete;
            async._loadCompleted += a => UnityEngine.Object.Destroy(async.gameObject);
            return async;
            //var load = DownloadEnumerator(www);
            //while (load.MoveNext()) ;
            //return www;
        }

        /// <summary>
        ///  线程加载指定文件夹路径下所有图片数据
        /// </summary>
        /// <param name="_path">文件夹路径</param>
        /// <param name="_complete">下载完成后委托事件，传入数据流及其原始文件名</param>
        public static LoadThread LoadThreadAllPicByFolder(string _path, System.Action<List<ThreadData>> _complete)
        {
            string[] allpicUrl = null;
            GameData.GetAllURL(_path, ref allpicUrl, FileType.Picture);
            LoadThread loadThread = new LoadThread(allpicUrl);
            loadThread.LoadThreadAllPicByFolder(_complete);
            return loadThread;
        }
    }

    /// <summary>
    /// 异步加载类
    /// </summary>
    public class Async : MonoBehaviour
    {
        /// <summary>
        /// 是否打印实时下载消息
        /// </summary>
        public bool bDebug;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string debugTxt { get; private set; }
        /// <summary>
        /// 进度
        /// </summary>
        public float progress { get; private set; }
        /// <summary>
        /// 下载返回数据
        /// </summary>
        public System.Action<AsyncData> _loadCompleted { get; set; }

        /// <summary>
        /// 协程加载图片数据
        /// </summary>
        /// <param name="path">路径组</param>
        /// <param name="async">异步类对象</param>
        /// <returns></returns>
        public static IEnumerator DownloadAllPicByUrls(string[] path, Async async)
        {
            float progress = 0;
            string[] names = new string[path.Length];

            #region WWW
            //WWW[] www = new WWW[path.Length];
            //for (int i = 0; i < www.Length; i++)
            //{
            //    www[i] = new WWW(path[i]);
            //    while (!www[i].isDone) ;
            //    yield return www[i];
            //    ++progress;
            //    string name = www[i].url.Substring(www[i].url.LastIndexOf("/") + 1).Replace(".png", "");
            //    //www[i].texture.name = name;
            //    names[i] = name;
            //    async.progress = progress / www.Length;
            //    async.debugTxt = "Loading：" + name/*www[i].url*/+ "     Progress:" + (async.progress * 100f).ToString("F1") + "%";
            //    if (async.bDebug)
            //        Debug.Log(async.debugTxt);
            //}
            //yield return www;
            #endregion

            #region UnityWebRequest
            Texture2D[] downloadHandlerTextures = new Texture2D[path.Length];
            for (int i = 0; i < downloadHandlerTextures.Length; i++)
            {
                UnityWebRequest wr = new UnityWebRequest(path[i]);
                DownloadHandlerTexture texDl = new DownloadHandlerTexture(true);
                wr.downloadHandler = texDl;
                yield return wr.SendWebRequest();
                if ((wr.isNetworkError || wr.isHttpError))
                {
                    Debug.LogError(wr.error);
                    yield break;
                }
                //Debug.LogWarning(texDl.text);
                downloadHandlerTextures[i] = texDl.texture;
                ++progress;
                string name = path[i].Substring(path[i].LastIndexOf("\\") + 1).Replace(".png", "");
                names[i] = name;
                async.progress = progress / downloadHandlerTextures.Length;
                async.debugTxt = "Loading：" + name + "     Progress:" + (async.progress * 100f).ToString("F1") + "%";
                if (async.bDebug)
                    Debug.Log(async.debugTxt);
            }
            #endregion

            if (async._loadCompleted != null)
            {
                AsyncData asyncData = new AsyncData(downloadHandlerTextures, names);
                async._loadCompleted(asyncData);
            }
            Resources.UnloadUnusedAssets();
        }
    }
    
    /// <summary>
    /// 线程加载
    /// </summary>
    public class LoadThread
    {
        /// <summary>
        /// 是否打印实时下载消息
        /// </summary>
        public bool bDebug;
        /// <summary>
        /// 消息内容
        /// </summary>
        public string debugTxt { get; private set; }
        /// <summary>
        /// 进度
        /// </summary>
        public float progress { get; private set; }
        /// <summary>
        /// 下载返回数据
        /// </summary>
        public System.Action<List<ThreadData>> _loadCompleted { get; set; }

        /// <summary>
        /// 线程
        /// </summary>
        private System.Threading.Thread thread;
        //public System.IO.Stream[] GetStream { get { return LoadStream; } }
        //protected System.IO.Stream[] LoadStream;
        private string[] _url;
        //private string[] _filePath;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="url">路径</param>
        public LoadThread(string[] url/*,string[] filePath*/)
        {
            _url = url;
            //LoadStream = new System.IO.Stream[_url.Length];
            //_filePath = filePath;
        }

        /// <summary>
        ///  线程加载指定文件夹路径下所有图片数据
        /// </summary>
        /// <param name="_complete">下载完成后监听，传入数据流及其原始文件名</param>
        public void LoadThreadAllPicByFolder(System.Action<List<ThreadData>> _complete)
        {
            this.thread = new System.Threading.Thread(new System.Threading.ThreadStart(DownLoadImage));
            this._loadCompleted += _complete;
            this.thread.Start();
        }

        private void DownLoadImage()
        {
            System.Net.WebClient web = new System.Net.WebClient();
            float _progress = 0;
            List<ThreadData> lisbyte = new List<ThreadData>();
            for (int i = 0; i < _url.Length; i++)
            {
                System.IO.Stream stream = web.OpenRead(_url[i]);
                //web.DownloadFile(new System.Uri(_url[i]), _filePath[i]);
                //web.Encoding = System.Text.Encoding.GetEncoding("GB2312");
                //string a = System.Text.Encoding.UTF8.GetString(web.DownloadData(_url[i]));
                string name = _url[i].Substring(_url[i].LastIndexOf("\\") + 1).Replace(".png", "");
                //www[i].texture.name = name;
                ++_progress;
                progress = _progress / _url.Length;
                debugTxt = "Loading：" + name/*www[i].url*/+ "     Progress:" + (progress * 100f).ToString("F1") + "%";
                if (bDebug)
                    Debug.Log(debugTxt);
                ThreadData threadData = new ThreadData(stream.ToBytes(), name);
                lisbyte.Add(threadData);
            }

            if (_loadCompleted != null)
                this._loadCompleted(lisbyte);
            thread.Abort();
            //web.DownloadFileAsync(new System.Uri(_url[0]), _filePath);
        }
    }

    /// <summary>
    /// 线程数据
    /// </summary>
    public struct ThreadData
    {
        public byte[] picBytes;
        public string flieName;
        public ThreadData(byte[] _picBytes, string _name)
        {
            picBytes = _picBytes;
            flieName = _name;
        }
    }

    /// <summary>
    /// 协程数据类
    /// </summary>
    public struct AsyncData
    {
        public Texture2D[] picBytes;
        public string[] flieName;
        public AsyncData(Texture2D[] _picBytes, string[] _name)
        {
            picBytes = _picBytes;
            flieName = _name;
        }
    }

    /// <summary>
    ///  加载资源
    /// </summary>
    public class LoadAssetBundle : MonoBehaviour
    {

        // 根据不同平台，声明StreamingAssetsPath路径
        public static readonly string STREAMING_ASSET_PATH =
#if UNITY_ANDROID
             Application.dataPath + "!assets";   // 安卓平台
#else
             Application.streamingAssetsPath;  // 其他平台
#endif

        private static CallBack<AssetBundle> CompleteLoad;

        /// <summary>
        /// 读取本地文件assetBundle
        /// </summary>
        /// <param name="_assetbundle">路径</param>
        /// <param name="_crc">校验码</param>
        /// <returns></returns>
        public static AssetBundle LoadFromFile(string _assetbundle, uint _crc = 0)
        {
            return AssetBundle.LoadFromFile(STREAMING_ASSET_PATH + _assetbundle, _crc);
        }

        /// <summary>
        /// 异步下载本地assetBundle
        /// </summary>
        /// <param name="_assetbundle">路径</param>
        /// <param name="_crc">校验码</param>
        /// <param name="callBack">下载完成委托</param>
        public static void LoadFromFileAsyn(string _assetbundle, CallBack<AssetBundle> callBack, uint _crc = 0)
        {
            GameObject load = new GameObject("LoadABAsyn");
            load.AddComponent<LoadAssetBundle>().StartCoroutine(LoadFileAsyn(STREAMING_ASSET_PATH + _assetbundle, _crc));
            CompleteLoad += a => Destroy(load);
            CompleteLoad += callBack;
        }

        /// <summary>
        /// 外部下载assetBundle
        /// </summary>
        /// <param name="_url">链接</param>
        /// <param name="_crc">校验码</param>
        /// <param name="callBack">下载完成委托</param>
        public static void LoadABAsyn(string _url, CallBack<AssetBundle> callBack, uint _crc = 0)
        {
            GameObject load = new GameObject("LoadABAsyn");
            load.AddComponent<LoadAssetBundle>().StartCoroutine(LoadAsynAB(_url, _crc));
            CompleteLoad += a => Destroy(load);
            CompleteLoad += callBack;
        }

        static IEnumerator LoadFileAsyn(string _path, uint _crc)
        {
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(_path, _crc);
            yield return request;
            AssetBundle ab = request.assetBundle;
            if (CompleteLoad != null)
                CompleteLoad(ab);
        }

        static IEnumerator LoadAsynAB(string url, uint _crc)
        {
            UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url, _crc);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.LogError(request.error);
                yield break;
            }
            //AssetBundle ab = DownloadHandlerAssetBundle.GetContent(request);
            AssetBundle ab = (request.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
            if (CompleteLoad != null)
                CompleteLoad(ab);
        }
    }
}

/// <summary>
/// 泛单
/// </summary>
/// <typeparam name="T"></typeparam>
public class Single<T> where T : class, new()
{
    private static T _instance;
    private static readonly object syslock = new object();

    /// <summary>
    /// 获取单例对象
    /// </summary>
    /// <returns></returns>
    public static T getInstance()
    {
        if (_instance == null)
        {
            lock (syslock)
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
            }
        }
        return _instance;
    }
}

/// <summary>
/// 玩家数据类
/// </summary>
[Serializable]
public partial class UserData
{
    /// <summary>
    /// 积分
    /// </summary>
    public int grade;
    /// <summary>
    /// 照片相对路径
    /// </summary>
    public string photoPath;
    /// <summary>
    /// 用户ID
    /// </summary>
    public string openid;
}

/// <summary>
/// 游戏基本状态
/// </summary>
public enum GameState
{
    /// <summary>
    /// 首页
    /// </summary>
    HomePage,
    /// <summary>
    /// 开始游戏
    /// </summary>
    GameStart,
    /// <summary>
    /// 游戏结束
    /// </summary>
    GameOver
}

/// <summary>
/// 路径文件类型
/// </summary>
public enum FileType
{
    /// <summary>
    /// 音频
    /// </summary>
    Audio,
    /// <summary>
    /// 视频
    /// </summary>
    Video,
    /// <summary>
    /// 图片
    /// </summary>
    Picture
}

/// <summary>
/// 图片格式
/// </summary>
public enum PicType
{
    PNG,
    JPG,
    EXR,
    WEBP,
    BMP,
    PCX,
    TIF,
    GIF,
    JPEG,
    TGA,
    UFO,
    SVG,
    PSD,
    CDR,
    PCD,
    DXF,
    EXIF,
    FPX,
    EPS,
    AI,
    HDRI,
    RAW
}

/// <summary>
/// 保存数据类型
/// </summary>
public enum DataType
{
    Json,
    Xml,
    Binary
}

/// <summary>
/// 扩展工具
/// </summary>
public static class ExtendTools
{
    /// <summary> 
    /// 将 byte[] 转成 Base64 
    /// </summary> 
    public static string ToBase64(this byte[] bytes)
    {
        return Convert.ToBase64String(bytes);
    }

    /// <summary> 
    /// 将 Stream 转成 byte[] 
    /// </summary> 
    public static byte[] ToBytes(this Stream stream)
    {
        byte[] bytes = new byte[stream.Length];
        stream.Read(bytes, 0, bytes.Length);

        // 设置当前流的位置为流的开始 
        stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }

    /// <summary>
    /// string to vector3
    /// </summary>
    /// <param name="vString">string</param>
    public static Vector3 Parse(this Vector3 vector3,string vString)
    {
        vString = vString.Replace("(", "").Replace(")", "");
        string[] s = vString.Split(',');
        return new Vector3(float.Parse(s[0]), float.Parse(s[1]), s.Length > 2 && s[2].Length > 0 ? float.Parse(s[2]) : 0);
    }

    /// <summary>
    /// string to vector3
    /// </summary>
    /// <param name="vString">string</param>
    public static Vector3 ToVector3(this string vString)
    {
        vString = vString.Replace("(", "").Replace(")", "");
        string[] s = vString.Split(',');
        return new Vector3(float.Parse(s[0]), float.Parse(s[1]), s.Length > 2 && s[2].Length > 0 ? float.Parse(s[2]) : 0);
    }

    /// <summary>
    /// Base64转换成Texture2D
    /// </summary>
    /// <param name="base64">base64字符串</param>
    public static Texture2D LoadImageByBase64(this Texture2D tex,string base64)
    {
        Texture2D tx = new Texture2D(1, 1);

        tx.LoadImage(Convert.FromBase64String(base64));

        tx.Apply();
       
        Resources.UnloadUnusedAssets();  //清理游离资源

        return tx;
    }

    /// <summary>
    /// Texture2D转换成Sprite
    /// </summary>
    public static Sprite ToSprite(this Texture2D texture2D)
    {
        if (texture2D == null)
        {
            Debug.LogWarning("texture2D is Null");
            return null;
        }
        Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
        sprite.name = texture2D.name;
        return sprite;
    }

    /// <summary>
    /// 加入用户
    /// </summary>
    /// <param name="lsUd">用户数据</param>
    /// <param name="_data">用户类</param>
    /// <param name="MaxRankNum">最大人数</param>
    /// <param name="bRepeat">是否排名重复</param>
    /// <returns></returns>
    public static List<UserData> AddUser(this List<UserData> lsUd, UserData _data, int MaxRankNum, bool bRepeat = true)
    {
        if (lsUd.Count < MaxRankNum || (lsUd.Count == MaxRankNum && lsUd[MaxRankNum - 1].grade < _data.grade))
        {
            if (!bRepeat)
            {
                int _index = lsUd.FindIndex(a => a.openid == _data.openid);
                if (_index != -1 && lsUd[_index].grade < _data.grade)
                    lsUd[_index].grade = _data.grade;
                else if (_index == -1)
                    lsUd.Add(_data);
            }
            else
                lsUd.Add(_data);
        }
        lsUd = lsUd.ToSort(true);
        if (lsUd.Count > MaxRankNum)
        {
            for (int i = MaxRankNum; i < lsUd.Count; i++)
            {
                if (lsUd.FindAll(a => a.photoPath.Split('/')[2] == lsUd[i].photoPath.Split('/')[2]).Count == 0)
                    File.Delete(Application.streamingAssetsPath + lsUd[i].photoPath);
                lsUd.RemoveAt(i);
            }
        }
        //清除多余照片
        string Loadpath = Application.streamingAssetsPath + "/UserPortrait";
        string[] allpath = Directory.GetFiles(Loadpath, "*.png");
        for (int i = 0; i < allpath.Length; i++)
        {
            if (lsUd.FindAll(a => a.photoPath != string.Empty && a.photoPath.Split('/')[2] == allpath[i].Split('\\')[1]).Count < 1)
                File.Delete(allpath[i]);
        }
        return lsUd;
    }

    #region 排序算法
    #region 链表排序
    //升序
    //list.OrderBy(i => i.a).ThenBy(i => i.b).ToList();
    //降序
    //list.OrderByDescending(i => i.a).ThenByDescending(i => i.b).ToList();
    #endregion

    #region 字典排序
    //升序
    //dic1 = dic1.OrderBy(o => o.Value.grade).ToDictionary(o => o.Key, p => p.Value);
    //dic1 = (from d in dic1 orderby d.Value.grade ascending select d).ToDictionary(k => k.Key, v => v.Value);
    //降序
    //dic1 = dic1.OrderByDescending(o => o.Value.grade).ToDictionary(o => o.Key, p => p.Value);
    //dic1 = (from d in dic1 orderby d.Value.grade descending select d).ToDictionary(k => k.Key, v => v.Value);
    #endregion
    #endregion

    /// <summary>
    /// 链表分数排序
    /// </summary>
    /// <param name="lisUser">链表数据</param>
    /// <param name="Descending">是否降序</param>
    /// <returns></returns>
    public static List<UserData> ToSort(this List<UserData> lisUser, bool Descending = false)
    {
        return Descending
            ? lisUser.OrderByDescending(i => i.grade).ToList()
            : lisUser.OrderBy(o => o.grade).ToList();
    }

    /// <summary>
    /// 字典分数排序
    /// </summary>
    /// <param name="dicUser">字典数据</param>
    /// <param name="Descending">是否降序</param>
    /// <returns></returns>
    public static Dictionary<string, UserData> ToSort(this Dictionary<string, UserData> dicUser, bool Descending = false)
    {
        return Descending
            ? dicUser.OrderByDescending(o => o.Value.grade).ToDictionary(o => o.Key, p => p.Value)
            : dicUser.OrderBy(o => o.Value.grade).ToDictionary(o => o.Key, p => p.Value);
    }

    /// <summary>
    /// 序列化字典分数排序
    /// </summary>
    /// <param name="sdicUser">序列化字典数据</param>
    /// <param name="Descending">是否降序</param>
    /// <returns></returns>
    private static SerialDictionary<string, UserData> ToSort(this SerialDictionary<string, UserData> sdicUser, bool Descending = false)
    {
        Dictionary<string, UserData> dic = sdicUser;
        dic = Descending
                ? dic.OrderByDescending(o => o.Value.grade).ToDictionary(o => o.Key, p => p.Value)
                : dic.OrderBy(o => o.Value.grade).ToDictionary(o => o.Key, p => p.Value);
        return new SerialDictionary<string, UserData>(dic);
    }

    /// <summary>
    /// 链表根据元素属性排序算法
    /// </summary>
    /// <typeparam name="T">元素</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的链表</returns>
    public static List<T> ToSort<T, TPair>(this List<T> lisUser, Func<T, TPair> keySelector, bool Descending = false)
    {
        return Descending
            ? lisUser.OrderByDescending(keySelector).ToList()
            : lisUser.OrderBy(keySelector).ToList();
    }

    /// <summary>
    /// 链表根据元素属性排序算法
    /// </summary>
    /// <typeparam name="T">元素</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的链表</returns>
    public static List<T> ToSort<T, TPair>(this List<T> lisUser, Func<T, TPair> keySelector, Func<T, TPair> additiona1, bool Descending = false)
    {
        return Descending
            ? lisUser.OrderByDescending(keySelector).ThenBy(additiona1).ToList()
            : lisUser.OrderBy(keySelector).ThenBy(additiona1).ToList();
    }

    /// <summary>
    /// 链表根据元素属性排序算法
    /// </summary>
    /// <typeparam name="T">元素</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="additiona2">附加条件2</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的链表</returns>
    public static List<T> ToSort<T, TPair>(this List<T> lisUser, Func<T, TPair> keySelector, Func<T, TPair> additiona1, Func<T, TPair> additiona2, bool Descending = false)
    {
        return Descending
            ? lisUser.OrderByDescending(keySelector).ThenBy(additiona1).ThenBy(additiona2).ToList()
            : lisUser.OrderBy(keySelector).ThenBy(additiona1).ThenBy(additiona2).ToList();
    }

    /// <summary>
    /// 链表根据元素属性排序算法
    /// </summary>
    /// <typeparam name="T">元素</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="additiona2">附加条件2</param>
    /// <param name="additiona3">附加条件3</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的链表</returns>
    public static List<T> ToSort<T, TPair>(this List<T> lisUser, Func<T, TPair> keySelector, Func<T, TPair> additiona1, Func<T, TPair> additiona2, Func<T, TPair> additiona3, bool Descending = false)
    {
        return Descending
            ? lisUser.OrderByDescending(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ToList()
            : lisUser.OrderBy(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ToList();
    }

    /// <summary>
    /// 链表根据元素属性排序算法
    /// </summary>
    /// <typeparam name="T">元素</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="additiona2">附加条件2</param>
    /// <param name="additiona3">附加条件3</param>
    /// <param name="additiona4">附加条件4</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的链表</returns>
    public static List<T> ToSort<T, TPair>(this List<T> lisUser, Func<T, TPair> keySelector, Func<T, TPair> additiona1, Func<T, TPair> additiona2, Func<T, TPair> additiona3, Func<T, TPair> additiona4, bool Descending = false)
    {
        return Descending
            ? lisUser.OrderByDescending(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ThenBy(additiona4).ToList()
            : lisUser.OrderBy(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ThenBy(additiona4).ToList();
    }

    /// <summary>
    /// 字典转换成序列化字典
    /// </summary>
    /// <returns>序列化字典</returns>
    public static SerialDictionary<TKey, TValue> ToSerialDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dic)
    {
        return new SerialDictionary<TKey, TValue>(dic);
    }

    /// <summary>
    /// 字典根据元素属性排序算法
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的字典</returns>
    public static Dictionary<TKey, TValue> ToSort<TKey, TValue, TPair>(this Dictionary<TKey, TValue> dicUser, Func<KeyValuePair<TKey, TValue>, TPair> keySelector, bool Descending = false)
    {
        return Descending
           ? dicUser.OrderByDescending(keySelector).ToDictionary(o => o.Key, p => p.Value)
           : dicUser.OrderBy(keySelector).ToDictionary(o => o.Key, p => p.Value);
    }

    /// <summary>
    /// 字典根据元素属性排序算法
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的字典</returns>
    public static Dictionary<TKey, TValue> ToSort<TKey, TValue, TPair>(this Dictionary<TKey, TValue> dicUser, Func<KeyValuePair<TKey, TValue>, TPair> keySelector, Func<KeyValuePair<TKey, TValue>, TPair> additiona1, bool Descending = false)
    {
        return Descending
           ? dicUser.OrderByDescending(keySelector).ThenBy(additiona1).ToDictionary(o => o.Key, p => p.Value)
           : dicUser.OrderBy(keySelector).ThenBy(additiona1).ToDictionary(o => o.Key, p => p.Value);
    }

    /// <summary>
    /// 字典根据元素属性排序算法
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="additiona2">附加条件2</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的字典</returns>
    public static Dictionary<TKey, TValue> ToSort<TKey, TValue, TPair>(this Dictionary<TKey, TValue> dicUser, Func<KeyValuePair<TKey, TValue>, TPair> keySelector, Func<KeyValuePair<TKey, TValue>, TPair> additiona1, Func<KeyValuePair<TKey, TValue>, TPair> additiona2, bool Descending = false)
    {
        return Descending
           ? dicUser.OrderByDescending(keySelector).ThenBy(additiona1).ThenBy(additiona2).ToDictionary(o => o.Key, p => p.Value)
           : dicUser.OrderBy(keySelector).ThenBy(additiona1).ThenBy(additiona2).ToDictionary(o => o.Key, p => p.Value);
    }

    /// <summary>
    /// 字典根据元素属性排序算法
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="additiona2">附加条件2</param>
    /// <param name="additiona3">附加条件3</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的字典</returns>
    public static Dictionary<TKey, TValue> ToSort<TKey, TValue, TPair>(this Dictionary<TKey, TValue> dicUser, Func<KeyValuePair<TKey, TValue>, TPair> keySelector, Func<KeyValuePair<TKey, TValue>, TPair> additiona1, Func<KeyValuePair<TKey, TValue>, TPair> additiona2, Func<KeyValuePair<TKey, TValue>, TPair> additiona3, bool Descending = false)
    {
        return Descending
           ? dicUser.OrderByDescending(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ToDictionary(o => o.Key, p => p.Value)
           : dicUser.OrderBy(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ToDictionary(o => o.Key, p => p.Value);
    }

    /// <summary>
    /// 字典根据元素属性排序算法
    /// </summary>
    /// <typeparam name="TKey">键</typeparam>
    /// <typeparam name="TValue">值</typeparam>
    /// <typeparam name="TPair">条件结果</typeparam>
    /// <param name="keySelector">主要条件</param>
    /// <param name="additiona1">附加条件1</param>
    /// <param name="additiona2">附加条件2</param>
    /// <param name="additiona3">附加条件3</param>
    /// <param name="additiona4">附加条件4</param>
    /// <param name="Descending">是否降序</param>
    /// <returns>根据条件排序完成后的字典</returns>
    public static Dictionary<TKey, TValue> ToSort<TKey, TValue, TPair>(this Dictionary<TKey, TValue> dicUser, Func<KeyValuePair<TKey, TValue>, TPair> keySelector, Func<KeyValuePair<TKey, TValue>, TPair> additiona1, Func<KeyValuePair<TKey, TValue>, TPair> additiona2, Func<KeyValuePair<TKey, TValue>, TPair> additiona3, Func<KeyValuePair<TKey, TValue>, TPair> additiona4, bool Descending = false)
    {
        return Descending
           ? dicUser.OrderByDescending(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ThenBy(additiona4).ToDictionary(o => o.Key, p => p.Value)
           : dicUser.OrderBy(keySelector).ThenBy(additiona1).ThenBy(additiona2).ThenBy(additiona3).ThenBy(additiona4).ToDictionary(o => o.Key, p => p.Value);
    }

    /// <summary>
    /// 找到场景中所有指定类型组件（包含被禁用物体）
    /// </summary>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns>所有指定类型组件</returns>
    public static T[] FinSceneObjectsOfTypeAll<T>(this GameObject obj) where T : UnityEngine.Object
    {
        return Resources.FindObjectsOfTypeAll<T>();
    }

    /// <summary>
    /// 根据Tag找到场景中所有对应物体（包含被禁用物体）
    /// </summary>
    /// <param name="tag">标签名</param>
    /// <returns>所有对应标签物体</returns>
    public static GameObject[] FinSceneObjectsWithTag(this GameObject obj, string tag)
    {
        GameObject[] gameObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
        return System.Array.FindAll(gameObjects, a => a.tag == tag);
    }

    /// <summary>
    /// 根据Layer找到场景中所有对应物体（包含被禁用物体）
    /// </summary>
    /// <param name="layer">层次</param>
    /// <returns>所有对应层次物体</returns>
    public static GameObject[] FinSceneObjectsWithLayer(this GameObject obj, int layer)
    {
        GameObject[] gameObjects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
        return System.Array.FindAll(gameObjects, a => a.layer == layer);
    }
}

