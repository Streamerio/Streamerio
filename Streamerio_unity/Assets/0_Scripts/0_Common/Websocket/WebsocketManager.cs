using System;
using System.Collections.Generic;
using Common;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using R3;
using UnityEngine.Networking;
public class WebsocketManager : SingletonBase<WebsocketManager>
{
  private bool _isConnected = false;
  private WebSocket _websocket;

  private string _roomId = string.Empty;

  private Dictionary<FrontKey, Subject<Unit>> _frontEventDict = new Dictionary<FrontKey, Subject<Unit>>();
  public IDictionary<FrontKey, Subject<Unit>> FrontEventDict => _frontEventDict;

  // アプリケーション終了中フラグ
  private bool _isShuttingDown = false;
  
  [SerializeField]
  private ApiConfigSO _apiConfigSO;
  
  private string _qrCodeURL = string.Empty;

  private string _frontendUrlFormat = null; 

  private string _backendHttpUrl = null;

  private string _backendWsBaseUrl = null;

  protected override void Awake()
  {
    base.Awake();
    
    if (_apiConfigSO != null)
    {
      _frontendUrlFormat = _apiConfigSO.frontendUrlFormat;
      _backendHttpUrl = _apiConfigSO.backendHttpUrl;
      _backendWsBaseUrl = _apiConfigSO.backendWsUrl;
    }
    else
    {
      Debug.LogError("ApiConfigSO is not assigned. Please assign an ApiConfigSO asset in the Inspector.");
    }
    
    foreach (FrontKey key in Enum.GetValues(typeof(FrontKey)))
    {
      _frontEventDict[key] = new Subject<Unit>();
    }
  }
  private void Update()
  {
    if (_isConnected)
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
      _websocket.DispatchMessageQueue();
    #endif
    }
  }

  // websocketのコネクションを確立する
  public async UniTask ConnectWebSocket(string websocketId)
  {
    if (_isConnected)
    {
      Debug.Log("WebSocket is already connected!");
      return;
    }
    
    // WebSocketのインスタンスを生成
    string websocketUrl;
    if (string.IsNullOrEmpty(websocketId))
    {
      websocketUrl = _backendWsBaseUrl;
    }
    else
    {
      websocketUrl = ZString.Format("{0}?room_id={1}", _backendWsBaseUrl, websocketId);
    }
    
    _websocket = new WebSocket(websocketUrl);

    if (_websocket == null)
    {
      Debug.Log("WebSocket is null!");
      return;
    }
    else{
      Debug.Log("WebSocket is ok!");
    }

    // WebSocketのイベントを焼き付け
    _websocket.OnOpen += () =>
    {
      Debug.Log("Connection open!");
      _isConnected = true;
    };

    _websocket.OnError += (e) =>
    {
      Debug.Log("Error! " + e);
    };

    _websocket.OnClose += async (e) =>
    {
      Debug.Log("Connection closed!");
      _isConnected = false;
      
      // アプリケーション終了中は再接続しない
      if (_isShuttingDown) return;
      
      // 再接続を試行
      // 現在のwebsocketIdが空の場合は新しくwebsocketIdを生成して接続
      await ConnectWebSocket(_roomId ?? string.Empty);
    };

    _websocket.OnMessage += (bytes) => ReceiveWebSocketMessage(bytes);

    await _websocket.Connect();
    return;
  }

  ///<summary>
  /// WebSocketからメッセージを受信する
  ///</summary>
  private void ReceiveWebSocketMessage(byte[] bytes)
  {
    var message = System.Text.Encoding.UTF8.GetString(bytes);
    Debug.Log($"Received: {message}");

    BaseMessage baseMessage = null;
    try
    {
      baseMessage = JsonUtility.FromJson<BaseMessage>(message);
    }
    catch (Exception ex)
    {
      Debug.Log($"JSON base parse error: {ex.Message}");
    }

    if (baseMessage == null || string.IsNullOrEmpty(baseMessage.type))
    {
      Debug.Log("No type field in JSON message.");
      return;
    }

    if (baseMessage.type == "room_created")
    {
      try
      {
        var room = JsonUtility.FromJson<RoomCreatedNotification>(message);
        if (room != null)
        {
          _roomId = room.room_id;
          return;
        }
      }
      catch (Exception ex)
      {
        Debug.Log($"room_created parse error: {ex.Message}");
      }
      Debug.Log("Failed to parse room_created message.");
      return;
    }

    if (baseMessage.type == "game_event")
    {
      try
      {
        var gameEvent = JsonUtility.FromJson<GameEventNotification>(message);
        if (gameEvent != null)
        {
          var keyType = (FrontKey)System.Enum.Parse(typeof(FrontKey), gameEvent.event_type, true);
          _frontEventDict[keyType]?.OnNext(Unit.Default);
        }
        
        return;
      }
      catch (Exception ex)
      {
        Debug.Log($"game_event parse error: {ex.Message}");
      }
      Debug.Log("Failed to parse game_event message.");
      return;
    }

    Debug.Log($"Unhandled JSON payload type: {baseMessage.type}");
    return;
  }

  ///<summary>
  /// WebSocketを切断する
  ///</summary>
  private async UniTask DisconnectWebSocket()
  {
    if (!_isConnected)
    {
      Debug.Log("WebSocket is not connected!");
      return;
    }

    if (_websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is already closed!");
      return;
    }

    await _websocket.Close();
  }
  
  ///<summary>
  /// フロントエンドのURLを取得する
  ///</summary>
  public async UniTask<string> GetFrontUrlAsync()
  {
    if (_qrCodeURL != string.Empty)
    {
      return _qrCodeURL;
    }
    
    await UniTask.WaitWhile(() => _roomId == string.Empty);
    _qrCodeURL = ZString.Format(_frontendUrlFormat, _roomId);
    
    return _qrCodeURL;
  }

  ///<summary>
  /// ゲーム終了通知
  ///</summary>
  public async UniTask GameEnd()
  {
    await SendWebSocketMessage("{\"type\": \"game_end\" }");
  }

  ///<summary>
/// ゲームリスタート（既存ルームを再利用）
/// バックエンドに再開通知を送り、ルームを active に戻す
///</summary>
public async UniTask RestartGame()
{
    if (string.IsNullOrEmpty(_roomId))
    {
        Debug.LogError("Cannot restart: room_id is empty");
        return;
    }
    
    string url = ZString.Format("{0}api/rooms/{1}/restart", _backendHttpUrl, _roomId);
    
    Debug.Log($"Restarting game for room: {_roomId}");
    
    using (var request = UnityWebRequest.Post(url, ""))
    {
        request.SetRequestHeader("Content-Type", "application/json");
        
        await request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Restart failed: {request.error}");
            return;
        }
        
        var responseText = request.downloadHandler.text;
        Debug.Log($"Restart response: {responseText}");
    }
    
    Debug.Log($"Game restarted successfully. Room {_roomId} is now active.");
}


  ///<summary>
  /// UnityからWebSocketにメッセージを送信する
  ///</summary>
  private async UniTask SendWebSocketMessage(string message)
  {
    if (_websocket.State == WebSocketState.Closed)
    {
      Debug.Log("WebSocket is not connected!");
      return;
    }

    await _websocket.SendText(message);
  }

  ///<summary>
  /// ヘルスチェック
  ///</summary>
  public void HealthCheck()
  {
    UnityWebRequest.Get(_backendHttpUrl).SendWebRequest();
    Debug.Log("HealthCheck");
  }


  ///<summary>
  /// アプリケーションが終了したときにwebsocketを閉じる
  ///</summary>
  private async void OnApplicationQuit()
  {
    _isShuttingDown = true;
    try
    {
      await DisconnectWebSocket();
    }
    catch (Exception ex)
    {
      Debug.Log($"Error during WebSocket disconnection: {ex.Message}");
    }
  }
  
  ///<summary>
  /// JSONの型定義
  ///</summary>
  private class BaseMessage
  {
    public string type;
  }

  private class RoomCreatedNotification
  {
    public string type;
    public string room_id;
    public string web_url;
  }

  private class GameEventNotification
  {
    public string type;
    public string event_type;
    public int trigger_count;
    
  }
}

public enum FrontKey
{
  skill1,
  skill2,
  skill3,
  enemy1,
  enemy2,
  enemy3,
}
