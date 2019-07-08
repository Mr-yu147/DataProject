using UnityEngine;
using UnityEngine.Video;

namespace DataTools
{
    /// <summary>
    /// 视频播放管理器V1.5
    /// </summary>
    public class VideoPlayerMG : MonoBehaviour
    {
        [Tooltip("PC:streamingAssets\nAndroid:persistentDataPath\n目录下外置视频文件夹路径")]
        public string VideoFolderPath = "/Videos";
        [Tooltip("视频播放组件实例")]
        public VideoPlayer vp;
        [Tooltip("是否开始自动播放")]
        public bool AutoAwake = false;
        [Tooltip("循环类型:不循环，单个循环，顺序循环，随机循环")]
        public LoopType loopType = LoopType.None;
        /// <summary>
        /// 获取当前视频进度
        /// </summary>
        public float GetVideoProgress { get { return progress; } }

        /// <summary>
        /// 获取当前视频长度
        /// </summary>
        public float GetVideoLength { get { return videoLength; } }

        /// <summary>
        /// 获取当前视频名字
        /// </summary>
        public string GetVideoName { get { return videoName; } }

        /// <summary>
        /// 获取当前视频大小
        /// </summary>
        public Vector2 GetVideoSize { get { return videoSize; } }

        /// <summary>
        /// 获取所有读取到的视频路径
        /// </summary>
        public string[] GetVideoAllPath { get { return videoUrl; } }
        /// <summary>
        /// 是否初始化
        /// </summary>
        public bool isInit { get; private set; }

        /// <summary>
        /// 实际路径
        /// </summary>
        public string ActualPath { get; private set; }

        /// <summary>
        /// 指定循环下标
        /// </summary>
        public int SpecifyIndex { get;  set; }

        [Range(0, 1)]
        [Tooltip("视频进度")]
        [SerializeField] private float progress;

        [Tooltip("视频长度")]
        [SerializeField] private float videoLength;

        [Tooltip("视频名字")]
        [SerializeField] private string videoName;

        [Tooltip("视频大小")]
        [SerializeField] private Vector2 videoSize;

        /// <summary>
        /// 所有加载到的视频路径
        /// </summary>
        [SerializeField] private string[] videoUrl;
        private string ePath;
        private void Awake()
        {
            ePath = string.Empty;
            if (Application.platform == RuntimePlatform.Android)
                ePath = Application.persistentDataPath;
            else if (Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.WindowsEditor)
                ePath = Application.streamingAssetsPath;
            if (AutoAwake)
                OnPlayVideo();
        }

        /// <summary>
        /// 视频准备完成事件注册
        /// </summary>
        /// <param name="_vp">注册完成的视频</param>
        void Finsh(VideoPlayer _vp)
        {
            _vp.Play();
            videoName = vp.url.Split('\\')[vp.url.Split('\\').Length - 1].Split('.')[0];
            videoSize = new Vector2(_vp.texture.width, _vp.texture.height);
            Debug.Log("Start playing video：" + videoName + "   Size:" + videoSize.x + "/" + videoSize.y);
            videoLength = (_vp.frameCount / _vp.frameRate);
        }

        /// <summary>
        /// 循环点
        /// </summary>
        /// <param name="_vp">视频</param>
        void LPR(VideoPlayer _vp)
        {
            //刷新路径
            GameData.GetAllURL(ActualPath, ref videoUrl, FileType.Video);
            string url = string.Empty;
            if (videoUrl != null)
            {
                if (SpecifyIndex!=-1)
                {
                    url = videoUrl.Length > SpecifyIndex ? videoUrl[SpecifyIndex] : string.Empty;
                }
                else if (videoUrl.Length > 1)
                {
                    if (loopType == LoopType.Sequential)
                    {
                        int nowIndex = System.Array.FindIndex(videoUrl, a => a == _vp.url);
                        nowIndex++;
                        _vp.url = videoUrl[nowIndex >= videoUrl.Length ? 0 : nowIndex];
                    }
                    else if (loopType == LoopType.Random)
                    {
                        _vp.Pause();
                        do
                        {
                            url = videoUrl[Random.Range(0, videoUrl.Length)];
                        } while (_vp.url == url);
                    }
                }
                else if (_vp.url != videoUrl[0])
                    url = videoUrl[0];
            }
           
            if (url != string.Empty)
            {
                _vp.url = url;
                _vp.Play();
            }
        }

        private void Update()
        {
            if (videoLength != 0)
                progress = (float)(vp.time / videoLength);
            if (progress >= 0.99f && loopType == LoopType.None)
                vp.Stop();
        }

        /// <summary>
        /// 根据名字找路径组下标
        /// </summary>
        /// <param name="_videoName"></param>
        /// <returns></returns>
        public int GetVIndexByName(string _videoName)
        {
            return System.Array.FindIndex(videoUrl, a => a.Split('\\')[vp.url.Split('\\').Length - 1].Split('.')[0] == _videoName);
        }

        /// <summary>
        /// 播放视频
        /// </summary>
        /// <param name="playIndex">播放下标</param>
        /// <param name="specifyIndex">循环指定下标</param>
        /// <returns>是否成功播放</returns>
        public bool OnPlayVideo(int playIndex = 0,int specifyIndex = -1)
        {
            ActualPath = ePath + VideoFolderPath;
            if (!System.IO.Directory.Exists(ActualPath))
                System.IO.Directory.CreateDirectory(ActualPath);
            if (vp == null)
                vp = !GetComponent<VideoPlayer>() ? gameObject.AddComponent<VideoPlayer>() : GetComponent<VideoPlayer>();
            else if (vp.isPlaying)
                vp.Stop();

            GameData.GetAllURL(ActualPath, ref videoUrl, FileType.Video);
            //播放视频
            if (videoUrl != null && videoUrl.Length > 0)
            {
                vp.source = VideoSource.Url;
                vp.url = videoUrl[playIndex];
                SpecifyIndex = specifyIndex;
                if (!isInit)
                {
                    vp.prepareCompleted += Finsh;
                    vp.loopPointReached += LPR;
                    isInit = true;
                }
                vp.isLooping = loopType != LoopType.None;
                vp.Prepare();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 暂停当前视频
        /// </summary>
        public void OnPeasue()
        {
            vp.Pause();
        }

        /// <summary>
        /// 播放当前视频
        /// </summary>
        public void OnPlay()
        {
            vp.Play();
        }

        /// <summary>
        /// 停止当前视频
        /// </summary>
        public void OnStop()
        {
            vp.Stop();
        }

        //public void OpenProject()
        //{
        //    OpenFileName openFileName = new OpenFileName();
        //    openFileName.structSize = Marshal.SizeOf(openFileName);
        //    openFileName.filter = "Mp4文件(*.mp4)\0*.mp4";
        //    openFileName.file = new string(new char[256]);
        //    openFileName.maxFile = openFileName.file.Length;
        //    openFileName.fileTitle = new string(new char[64]);
        //    openFileName.maxFileTitle = openFileName.fileTitle.Length;
        //    openFileName.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        //    openFileName.title = "浏览Mp4文件";
        //    openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
        //    if (LocalDialog.GetOpenFileName(openFileName))
        //    {
        //        Debug.Log(openFileName.file);
        //        Debug.Log(openFileName.fileTitle);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("URL选择失败");
        //    }
        //}

        private void OnDestroy()
        {
            vp.prepareCompleted -= Finsh;
            vp.loopPointReached -= LPR;
        }

        public enum LoopType
        {
            /// <summary>
            /// 不循环
            /// </summary>
            None,
            /// <summary>
            /// 单个循环
            /// </summary>
            Single,
            /// <summary>
            /// 顺序循环
            /// </summary>
            Sequential,
            /// <summary>
            /// 随机循环
            /// </summary>
            Random
        }
    }
}

