using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using UnityEngine.UI;
using UnityEngine.Networking;

namespace DataTools
{
    /// <summary>
    /// 音频管理器 V2.0
    /// </summary>
    public class AudioSound : MonoBehaviour
    {
        /// <summary>
        /// 字典存储音乐音效
        /// </summary>
        protected static Dictionary<string, AudioClip> mainSound = new Dictionary<string, AudioClip>();
        /// <summary>
        /// 音效链表
        /// </summary>
        public static List<AudioSource> curSoundList = new List<AudioSource>();
        /// <summary>
        /// 音乐链表
        /// </summary>
        public static List<AudioSource> curBGMList = new List<AudioSource>();
        /// <summary>
        /// 音频挂载根节点
        /// </summary>
        public static Transform audioRoot = null;
        /// <summary>
        /// 默认音效音量
        /// </summary>
        public static float soundVlo = 0.5f;
        /// <summary>
        /// 默认音乐音量
        /// </summary>
        public static float musicVlo = 0.5f;
        /// <summary>
        /// 加载完成监听（注：此事件必须在初始化前完成注册）
        /// </summary>
        public static System.Action<Dictionary<string, AudioClip>> LoadComplete;
        /// <summary>
        /// 主音频容器
        /// </summary>
        public static Dictionary<string, AudioClip> GetMainSound { get { return mainSound; } }
        /// <summary>
        /// 是否初始化
        /// </summary>
        public static bool isInit { get; set; }
        private static bool isPause;//是否暂停
        private static float time = 0;//消失计时 
        private static string _path;//音频保存路径
        private static string _text = string.Empty;//显示文本

        /// <summary>
        /// 初始化音乐
        /// </summary>
        /// <param name="aType">加载方式（注：外置音频格式只支持OGG）</param>
        public static void InitSound(LoadType aType = LoadType.Resources)
        {
            if (Application.platform == RuntimePlatform.Android)
                _path = Application.persistentDataPath;
            else if (Application.platform == RuntimePlatform.WindowsPlayer
                || Application.platform == RuntimePlatform.WindowsEditor)
                _path = Application.streamingAssetsPath;
            if (!System.IO.Directory.Exists(_path))
                System.IO.Directory.CreateDirectory(_path);
            curSoundList.Clear();
            curBGMList.Clear();
            if (!isInit)
                LoadSound(aType);
        }

        /// <summary>
        /// 加载声音
        /// </summary>
        private static void LoadSound(LoadType aType)
        {
            if (aType == LoadType.Resources)
            {
                AudioClip[] audioClips = Resources.LoadAll<AudioClip>("sound");
                if (audioClips!=null&& audioClips.Length>0)
                {
                    foreach (AudioClip sound in audioClips)
                    {
                        if (!mainSound.ContainsKey(sound.name))
                            mainSound.Add(sound.name, sound);
                    }
                    isInit = true;
                    if (LoadComplete != null)
                        LoadComplete(GetMainSound);
                }
                else
                    Debug.LogError("Loading failed. Check if path audio exists");
            }
            else
            {
                audioRoot = new GameObject("AudioSound").transform;
                audioRoot.gameObject.AddComponent<AudioSound>().StartCoroutine(LoadAudioClip());
            }

            if (System.IO.File.Exists(_path + "/music"))
            {
                string v = System.IO.File.ReadAllText(_path + "/music");
                if (!string.IsNullOrEmpty(v))
                {
                    string[] s = v.Split('_');
                    if (s.Length > 1)
                    {
                        if (s[0].Length > 0) { soundVlo = float.Parse(s[0]); ChangeSoundVolume(soundVlo); }

                        if (s[1].Length > 0) { musicVlo = float.Parse(s[1]); ChangeMusicVolume(musicVlo); }
                    }
                }
            }
        }

        /// <summary>
        /// 下载外置音频
        /// </summary>
        /// <returns></returns>
        public static IEnumerator LoadAudioClip()
        {
            if (System.IO.Directory.Exists(_path + "/AudioFile"))
            {
                if (!isInit)
                {
                    string[] allApath = GameData.GetAllURL(_path + "/AudioFile", new string[] { "*.ogg" });

                    for (int i = 0; i < allApath.Length; i++)
                    {
                        using (var uwr = UnityWebRequestMultimedia.GetAudioClip(allApath[i], AudioType.OGGVORBIS))
                        {
                            yield return uwr.SendWebRequest();
                            if (uwr.isNetworkError || uwr.isHttpError)
                            {
                                Debug.LogError(uwr.error);
                                yield break;
                            }

                            AudioClip clip = DownloadHandlerAudioClip.GetContent(uwr);
                            string name = uwr.url.Split(new string[] { "AudioFile/" }, System.StringSplitOptions.None)[1].Split('.')[0];
                            // use audio clip
                            if (!mainSound.ContainsKey(clip.name))
                                mainSound.Add(name, clip);
                        }
                    }
                    isInit = true;
                    AudioSound audioSound = FindObjectOfType<AudioSound>();
                    if (audioSound)
                        Destroy(audioSound);
                    if (LoadComplete != null)
                        LoadComplete(GetMainSound);
                }
                else
                    Debug.LogWarning("Do not repeat initialization!");
            }
            else
            {
                Debug.LogWarning("path not found!");
                System.IO.Directory.CreateDirectory(_path + "/AudioFile");
            }

        }

        /// <summary>
        /// 初始化创建加载音乐
        /// </summary>
        /// <param name="_fileName">音乐名字</param>
        /// <param name="isSound">是否是音效</param>
        /// <param name="loop">是否循环播放</param>
        /// <param name="autoPlay">是否自动播放</param>
        /// <returns>音频长度</returns>
        public static float CreateSoundPlay(string _fileName, bool isSound = true, bool loop = false, bool autoPlay = true)
        {
            if (_fileName == null || !mainSound.ContainsKey(_fileName))
                return 0;

            GameObject go = new GameObject("AudioSound:" + _fileName);

            if (audioRoot == null)
                audioRoot = GameObject.Find("AudioSound").transform;

            go.transform.SetParent(audioRoot);
            go.transform.localPosition = Vector3.zero;
            AudioSource ads = go.AddComponent<AudioSource>();
            ads.clip = mainSound[_fileName];
            ads.loop = loop;
            ads.clip.name = _fileName;
            if (!isSound)
            {
                ads.volume = musicVlo;
                curBGMList.Add(ads);
            }
            else
            {
                ads.volume = soundVlo;
                curSoundList.Add(ads);
            }
            if (!loop)
            {
                curSoundList.Remove(ads);
                Destroy(go, ads.clip.length);
            }
            if (autoPlay)
                ads.Play();
            return ads.clip.length;
        }

        /// <summary>
        /// 无限随机循环
        /// </summary>
        /// <param name="randMusicName">随机歌曲名字数组</param>
        /// <param name="RandLoop">是否无线循环随机</param>
        /// <returns></returns>
        public static IEnumerator loopNext(string[] randMusicName, bool RandLoop)
        {
            AllStop();
            CreateSoundPlay(randMusicName[Random.Range(0, randMusicName.Length)], false);

            while (RandLoop)
            {
                if (audioRoot && audioRoot.childCount > 0)
                {
                    float waitime = 1;
                    for (int i = 0; i < audioRoot.childCount; i++)
                    {
                        for (int j = 0; j < curBGMList.Count; j++)
                        {
                            if (curBGMList[j] && curBGMList[j].name.Equals(audioRoot.GetChild(i).name))
                            {
                                waitime = curBGMList[j].clip.length;
                                break;
                            }
                        }
                    }
                    yield return new WaitForSeconds(waitime);
                    CreateSoundPlay(randMusicName[Random.Range(0, randMusicName.Length)], false);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        /// <summary>
        /// 修改音乐音量
        /// </summary>
        /// <param name="_value">音乐大小</param>
        public static void ChangeMusicVolume(float _value)
        {
            musicVlo = _value;
            if (curBGMList == null)
                return;

            for (int i = 0; i < curBGMList.Count; i++)
            {
                curBGMList[i].volume = _value;
            }
            string s = soundVlo.ToString("0.0") + "_" + musicVlo.ToString("0.0");
            System.IO.File.WriteAllText(_path + "/music", s);
        }

        /// <summary>
        /// 修改音效音量
        /// </summary>
        /// <param name="_value">音效大小</param>
        public static void ChangeSoundVolume(float _value)
        {
            soundVlo = _value;
            if (curSoundList == null)
                return;

            for (int i = 0; i < curSoundList.Count; i++)
            {
                curSoundList[i].volume = soundVlo;
            }
            string s = soundVlo.ToString("0.0") + "_" + musicVlo.ToString("0.0");
            System.IO.File.WriteAllText(_path + "/music", s);
        }

        /// <summary>
        /// 停止播放指定音频
        /// </summary>
        /// <param name="_name">音频名字</param>
        /// <param name="bSound">是否为音频</param>
        public static bool StopAudio(string _name, bool bSound = true)
        {
            AudioSource audioSource = FindAudioByName(_name);
            if (audioSource == null)
                return false;
            else
            {
                audioSource.Stop();
                if (bSound)
                    Destroy(audioSource.gameObject);
            }
            return true;
        }

        /// <summary>
        /// 重置播放指定音频
        /// </summary>
        /// <param name="_name">音频名字</param>
        /// <returns></returns>
        public static bool ReplayAudio(string _name)
        {
            AudioSource audioSource = FindAudioByName(_name);
            if (audioSource == null)
                return false;
            else
                audioSource.time = 0;
            return true;
        }

        /// <summary>
        /// 暂停/播放指定音频
        /// </summary>
        /// <param name="_name">音频名字</param>
        /// <param name="bPause">是否为背景音乐</param>
        public static bool PauseAudio(string _name, bool bPause = true)
        {
            AudioSource audioSource = FindAudioByName(_name);
            if (audioSource == null)
                return false;
            if (bPause)
                audioSource.Pause();
            else
                audioSource.UnPause();
            return true;
        }

        /// <summary>
        /// 关闭所有音乐
        /// </summary>
        public static void AllStop()
        {
            for (int i = 0; i < curBGMList.Count; i++)
            {
                if (curBGMList[i])
                {
                    curBGMList[i].Stop();
                }
            }
        }

        /// <summary>
        /// 暂停所有音乐
        /// </summary>
        public static void AllPause()
        {
            isPause = !isPause;
            for (int i = 0; i < curBGMList.Count; i++)
            {
                if (curBGMList[i] != null)
                    if (isPause)
                        curBGMList[i].Pause();
                    else
                        curBGMList[i].UnPause();
            }
        }

        /// <summary>
        /// 清空主音频容器
        /// </summary>
        public static void ClearSound()
        {
            mainSound.Clear();
        }

        /// <summary>
        /// 音乐音量设置(GUILabel显示。默认操作键↑↓←→调节大小（注：此方法必须在OnGUI方法内执行）)
        /// </summary>
        /// <param name="e_time">存在时间</param>
        /// <param name="x">Label的X坐标</param>
        /// <param name="y">Label的Y坐标</param>
        /// <param name="width">Label的宽度</param>
        /// <param name="height">Label的高度</param>
        public static void ShowVolumeByGUI(float e_time = 1, float x = 0, float y = 0, float width = 500, float height = 300)
        {
            #region 音乐设置
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (musicVlo >= 0.04f)
                {
                    musicVlo -= 0.05f;
                    ChangeMusicVolume(musicVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (musicVlo < 1)
                {
                    musicVlo += 0.05f;
                    ChangeMusicVolume(musicVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (soundVlo < 1)
                {
                    soundVlo += 0.05f;
                    ChangeSoundVolume(soundVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (soundVlo >= 0.04f)
                {
                    soundVlo -= 0.05f;
                    ChangeSoundVolume(soundVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)
              || Input.GetKeyDown(KeyCode.RightArrow))
            {
                string txt = "当前背景音量:" + (musicVlo * 100).ToString("00");
                //<color=#00FF01FF></color>
                _text = "<color=cyan>" + txt + "</color>";
                time = 0;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.DownArrow))
            {
                string txt = "当前音效音量:" + (soundVlo * 100).ToString("00");
                _text = "<color=magenta>" + txt + "</color>";
                time = 0;
            }
            else
            {
                time += Time.deltaTime;
                if (time > e_time)
                    _text = string.Empty;
            }
            //GUI.color = Color.cyan;
            GUI.Label(new Rect(x, y, width, height), _text);
            #endregion
        }

        /// <summary>
        /// 音乐音量设置(本文显示。默认操作键↑↓←→)
        /// </summary>
        /// <param name="e_time">存在时间</param>
        /// <returns>文本内容</returns>
        public static string ShowVolumeByText(float e_time = 1)
        {
            #region 音乐设置
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (musicVlo >= 0.09f)
                {
                    musicVlo -= 0.1f;
                    ChangeMusicVolume(musicVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (musicVlo < 1)
                {
                    musicVlo += 0.1f;
                    ChangeMusicVolume(musicVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (soundVlo < 1)
                {
                    soundVlo += 0.1f;
                    ChangeSoundVolume(soundVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (soundVlo >= 0.09f)
                {
                    soundVlo -= 0.1f;
                    ChangeSoundVolume(soundVlo);
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow)
              || Input.GetKeyDown(KeyCode.RightArrow))
            {
                string txt = "当前背景音量:" + (musicVlo * 100).ToString("00");
                //<color=#00FF01FF></color>
                _text = "<color=cyan>" + txt + "</color>";
                time = 0;
            }
            if (Input.GetKeyDown(KeyCode.UpArrow)
                || Input.GetKeyDown(KeyCode.DownArrow))
            {
                string txt = "当前音效音量:" + (soundVlo * 100).ToString("00");
                _text = "<color=magenta>" + txt + "</color>";
                time = 0;
            }
            else
            {
                time += Time.deltaTime;
                if (time > e_time)
                    _text = string.Empty;
            }
            return _text;
            #region TextUI显示
            //if (_text != null)
            //{
            //    _text.GetComponent<RectTransform>().sizeDelta = new Vector2(1000, 120);
            //    _text.GetComponent<RectTransform>().anchorMax = Vector2.zero;
            //    _text.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            //    _text.GetComponent<RectTransform>().pivot = Vector2.zero;
            //    _text.GetComponent<RectTransform>().localScale = new Vector3(0.3f, 0.3f, 0.3f);
            //    _text.fontStyle = FontStyle.Bold;
            //    _text.fontSize = 100;
            //    _text.name = "Music";
            //    _text.alignment = TextAnchor.LowerLeft;
            //    if (Input.GetKeyDown(KeyCode.LeftArrow)
            //   || Input.GetKeyDown(KeyCode.RightArrow))
            //    {
            //        if (!_text.gameObject.activeInHierarchy)
            //            _text.gameObject.SetActive(true);
            //        _text.CrossFadeAlpha(1, 0.1f, true);
            //        _text.color = Color.cyan;
            //        _text.text = "当前背景音量:" + (musicVlo * 100).ToString("00");
            //        time = 0;
            //    }
            //    if (Input.GetKeyDown(KeyCode.UpArrow)
            //        || Input.GetKeyDown(KeyCode.DownArrow))
            //    {
            //        if (!_text.gameObject.activeInHierarchy)
            //            _text.gameObject.SetActive(true);
            //        _text.CrossFadeAlpha(1, 0.1f, true);
            //        _text.color = Color.magenta;
            //        _text.text = "当前音效音量:" + (soundVlo * 100).ToString("00");
            //        time = 0;
            //    }
            //    else
            //    {
            //        time += Time.deltaTime;
            //        if (time > m_time)
            //            _text.CrossFadeAlpha(0, duration, true);
            //    }
            //}
            //else
            //    Debug.Log("显示文本为空");
            #endregion
            #endregion
        }

        /// <summary>
        /// 根据名字找音频
        /// </summary>
        /// <param name="_name">名字</param>
        /// <returns></returns>
        public static AudioSource FindAudioByName(string _name)
        {
            if (audioRoot == null)
                return null;
            AudioSource[] audioSources = audioRoot.GetComponentsInChildren<AudioSource>();
            int _index = System.Array.FindIndex(audioSources, a => a.clip.name == _name);
            return _index == -1 ? null : audioSources[_index];
        }

        public enum LoadType
        {
            /// <summary>
            /// Resources/sound路径文件夹
            /// </summary>
            Resources,
            /// <summary>
            /// StreamingAssetsPath/AudioFile路径文件夹
            /// </summary>
            StreamingAssets
        }
    }
}

