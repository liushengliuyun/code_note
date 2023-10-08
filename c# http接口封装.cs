//c# http接口封装


 public class HttpRequestHandler : IDisposable
    {
        private readonly UnityWebRequest _unityWebRequest;

        private Action<bool, string> _callBack;

        public int TimeOut { set; get; } = 5;

        private readonly string _httpMethod;

        private string _url = "";

        public bool IsDecompressText;

        public HttpRequestHandler(string url, string method, Action<bool, string> callBack)
        {
            _url = url;
            method = method.ToUpper();
            _httpMethod = method;

            if (string.IsNullOrEmpty(_httpMethod))
            {
                CarbonLogger.LogError("创建HTTP没有传入Method");
                return;
            }

            _callBack = callBack;

            switch (method)
            {
                case "GET":
                    _unityWebRequest = UnityWebRequest.Get(url);
                    break;
                case "POST":
                    _unityWebRequest = UnityWebRequest.Post(url, new WWWForm());
                    break;
                case "HEAD":
                    _unityWebRequest = UnityWebRequest.Head(url);
                    break;
            }
            reLoginHandler = (sender, args) => { OnDispose(); };
            
            //添加监听
            EventDispatcher.Root.AddListener(GlobalEvent.RELOGIN, reLoginHandler);
        }

        private void OnDispose()
        {
            if (coroutine != null)
            {
                Framework.Instance.StopCoroutine(coroutine);
            }

            
         //依赖cat lib的协程   
           Framework.Instance.StartCoroutine(TryRemoveListener());
        }

        private EventHandler reLoginHandler;

        private Coroutine coroutine;

        //设置Http为json。
        public void SetPostJson(string jsonData)
        {
            byte[] body = Encoding.UTF8.GetBytes(jsonData);
            _unityWebRequest.uploadHandler = new UploadHandlerRaw(body);
            _unityWebRequest.disposeDownloadHandlerOnDispose = true;
            _unityWebRequest.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        }

        //设置HTTP的头部信息
        public void SetHeader(string headKey, string headValue)
        {
            _unityWebRequest.SetRequestHeader(headKey, headValue);
        }

        public void Start()
        {
            coroutine = Framework.Instance.StartCoroutine(OnStartRequest());
        }

        private IEnumerator OnStartRequest()
        {
            if (_unityWebRequest == null)
            {
                CarbonLogger.LogError("UnityWebRequest 为空");
                yield break;
            }

            _unityWebRequest.timeout = TimeOut;

            yield return _unityWebRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(_unityWebRequest.error))
            {
                CarbonLogger.LogError($"请求失败 {_unityWebRequest.error} {_url}");
                _callBack?.Invoke(false, _unityWebRequest.error);
                _unityWebRequest.Dispose();
                yield break;
            }

            if (_httpMethod.Equals("HEAD"))
            {
                if (int.TryParse(_unityWebRequest.GetResponseHeader("Content-Length"), out int len))
                {
                    _callBack?.Invoke(true, len.ToString());
                }
                else
                {
                    _callBack?.Invoke(false, "");
                }
            }
            else
            {
                var downloadHandlerText = _unityWebRequest.downloadHandler.text;
                YZLog.LogColor(
                    $"请求的url是 {_url} " +
                    $" 请求的数据长度是 {_unityWebRequest.downloadHandler.data.Length} " +
                    $"请求的内容是 {downloadHandlerText}");

                if (IsDecompressText)
                {
                    var content = FileUtils.GZipDecompressString(downloadHandlerText);
                    _callBack?.Invoke(true, content);
                }
                else
                {
                    _callBack?.Invoke(true, downloadHandlerText);
                }
            }

            _unityWebRequest.Dispose();
            _callBack = null;
            RemoveListener();
        }

        /// <summary>
        /// 移除监听
        /// </summary>
        /// <returns></returns>
        private void RemoveListener()
        {
            EventDispatcher.Root.RemoveListener(GlobalEvent.RELOGIN, reLoginHandler);
        }

        public void Dispose()
        {
            OnDispose();
        }

        IEnumerator TryRemoveListener()
        {
            //等待一帧后 ， 理由：
            yield return new WaitForEndOfFrame();
            RemoveListener();
        }
    }
