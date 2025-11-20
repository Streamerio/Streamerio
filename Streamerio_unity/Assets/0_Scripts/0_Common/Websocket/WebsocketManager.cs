using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Common;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using NativeWebSocket;
using R3;
using UnityEngine.Networking;
using VContainer.Unity;

public class WebSocketManager : IWebSocketManager, IDisposable, ITickable
{
  private WebSocket _websocket;

  private ReactiveProperty<bool> _isConnectedProp = new ReactiveProperty<bool>(false);
  public ReadOnlyReactiveProperty<bool> IsConnectedProp => _isConnectedProp;

  private string _roomId = string.Empty;

  private Dictionary<FrontKey, Subject<Unit>> _frontEventDict = Enum.GetValues(typeof(FrontKey))
    .Cast<FrontKey>()
    .ToDictionary(key => key, key => new Subject<Unit>());
  public IReadOnlyDictionary<FrontKey, Subject<Unit>> FrontEventDict => _frontEventDict;
  
  private GameEndSummaryNotification _gameEndSummary = null;
  public GameEndSummaryNotification GameEndSummary => _gameEndSummary;

  private readonly ApiConfigSO _apiConfigSO;
  
  private string _qrCodeURL = string.Empty;

  private float _connectionTimeout = 10f;

  public WebSocketManager(ApiConfigSO apiConfigSO)
  {
    _apiConfigSO = apiConfigSO;
  }
  
  void ITickable.Tick()
  {
    if (_isConnectedProp.Value && _websocket != null)
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
      _websocket.DispatchMessageQueue();
    #endif
    }
  }

  // websocketのコネクションを確立する
  public async UniTask ConnectWebSocketAsync([CanBeNull] string websocketId = null, CancellationToken cancellationToken = default)
  {
    if (_isConnectedProp.Value)
    {
      Debug.Log("WebSocket is already connected!");
      return;
    }
    
    // WebSocketのインスタンスを生成
    string websocketUrl;
    if (string.IsNullOrEmpty(websocketId))
    {
      websocketUrl = _apiConfigSO.backendWsUrl;
    }
    else
    {
      websocketUrl = ZString.Format(_apiConfigSO.frontendQueryParamFormat, _apiConfigSO.backendWsUrl, websocketId);
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
      _isConnectedProp.Value = true;
    };

    _websocket.OnError += (e) =>
    {
      Debug.LogError($"Error! {e}");
      _isConnectedProp.Value = false;
    };

    _websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
      _isConnectedProp.Value = false;
    };

    _websocket.OnMessage += (bytes) =>
    {
      ReceiveWebSocketMessage(bytes);
    };

    _ = _websocket.Connect(); 
    Debug.Log("WebSocket connecting...");
    // _roomIdが設定されるまで待機
    await UniTask.WhenAny(
      UniTask.WaitWhile(() => _roomId == string.Empty, cancellationToken: cancellationToken),
      UniTask.WaitForSeconds(_connectionTimeout, cancellationToken: cancellationToken)
    );
    Debug.Log("WebSocket connected!");
    return;
  }

  ///<summary>
  /// JSON文字列を指定された型に安全にパースする
  /// </summary>
  /// <typeparam name="T">パース対象の型</typeparam>
  /// <param name="json">JSON文字列</param>
  /// <param name="data">パース結果（成功時）</param>
  /// <returns>パースが成功した場合true、失敗した場合false</returns>
  private bool TryJsonParse<T>(string json, out T data)
  {
    data = default(T);
    
    if (string.IsNullOrEmpty(json))
    {
      return false;
    }
    
    try
    {
      data = JsonUtility.FromJson<T>(json);
      return data != null;
    }
    catch (Exception ex)
    {
      Debug.LogError($"JSON parse error for type {typeof(T).Name}: {ex.Message}");
      return false;
    }
  }

  ///<summary>
  /// WebSocketからメッセージを受信する
  ///</summary>
  private void ReceiveWebSocketMessage(byte[] bytes)
  {
    if (bytes == null || bytes.Length == 0)
    {
      Debug.LogWarning("[WebSocket] Received empty or null message");
      return;
    }
    
    var message = System.Text.Encoding.UTF8.GetString(bytes);
    Debug.Log($"Received: {message}");

    BaseMessage baseMessage = null;
    MessageType messageType = MessageType.unknown;
    
    if (!TryJsonParse(message, out baseMessage))
    {
      Debug.Log("No type field in JSON message.");
      return;
    }

    if (string.IsNullOrEmpty(baseMessage.type))
    {
      Debug.Log("No type field in JSON message.");
      return;
    }
    
    if (!Enum.TryParse<MessageType>(baseMessage.type, out messageType))
    {
      Debug.LogError($"MessageType parse error: Unknown message type '{baseMessage.type}'");
      return;
    }

    switch (messageType)
    {
      case MessageType.room_created:
        Debug.Log("room_createdを受け取った");
        if (TryJsonParse(message, out RoomCreatedNotification room))
        {
          _roomId = room.room_id;
        }
        else
        {
          Debug.LogError("Failed to parse room_created message.");
        }
        break;
        
      case MessageType.game_event:
        if (TryJsonParse(message, out GameEventNotification gameEvent))
        {
          if (Enum.TryParse<FrontKey>(gameEvent.event_type, true, out var keyType))
          {
            _frontEventDict[keyType]?.OnNext(Unit.Default);
          }
          else
          {
            Debug.LogError($"FrontKey parse error: Unknown event type '{gameEvent.event_type}'");
          }
        }
        else
        {
          Debug.LogError("Failed to parse game_event message.");
        }
        break;

      case MessageType.game_end_summary:
        try
        {
          var root = MiniJSON.Json.Deserialize(message) as Dictionary<string, object>;
          if (root == null) throw new Exception("root is null");

          _gameEndSummary = new GameEndSummaryNotification();

          // --- team_tops の分解 ---
          if (root.TryGetValue("team_tops", out var teamObj) &&
              teamObj is Dictionary<string, object> teamDict)
          {
            foreach (var kv in teamDict)
            {
              if (kv.Value is Dictionary<string, object> val)
              {
                var detail = new GameEndSummaryNotification.SummaryDetail
                {
                  count       = val.TryGetValue("count", out var c)
                    ? Convert.ToInt32(c, CultureInfo.InvariantCulture)
                    : 0
                  ,
                  viewer_id   = val.TryGetValue("viewer_id", out var vid) ? vid?.ToString() : null,
                  viewer_name = val.TryGetValue("viewer_name", out var vname) ? vname?.ToString() : null,
                };
                _gameEndSummary.SummaryDetails[kv.Key] = detail; // "all" / "enemy" / "skill"
              }
            }
          }
          
          break;
        }
        catch (Exception ex)
        {
          Debug.LogError($"game_end_summary parse error: {ex.Message}");
        }
          
        Debug.LogError("Failed to parse game_end_summary message.");
        break;

      default:
        Debug.LogError($"Unhandled JSON payload type: {messageType}");
        break;
    }

    return;
  }

  ///<summary>
  /// WebSocketを切断する
  ///</summary>
  public void DisconnectWebSocket()
  {
    if (!_isConnectedProp.Value)
    {
      Debug.LogError("WebSocket is not connected!");
      return;
    }
    _websocket.CancelConnection();
    _isConnectedProp.Value = false;
  }
  
  ///<summary>
  /// フロントエンドのURLを取得する
  ///</summary>
  public string GetFrontUrl()
  {
    if (_roomId == string.Empty)
    {
      Debug.LogError("Room ID is not set!");
      return string.Empty;
    }
    _qrCodeURL = ZString.Format(_apiConfigSO.frontendUrlFormat, _roomId);
    
    return _qrCodeURL;
  }

  ///<summary>
  /// ゲーム開始通知
  ///</summary>
  public async UniTask GameStartAsync()
  {
    await SendWebSocketMessageAsync( _apiConfigSO.gameStartResponse );
  }

  ///<summary>
  /// ゲーム終了通知
  ///</summary>
  public async UniTask GameEndAsync()
  {
    await SendWebSocketMessageAsync( _apiConfigSO.gameEndResponse );
  }


  ///<summary>
  /// UnityからWebSocketにメッセージを送信する
  ///</summary>
  private async UniTask SendWebSocketMessageAsync(string message)
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
    UnityWebRequest.Get(_apiConfigSO.backendHttpUrl).SendWebRequest();
    Debug.Log("HealthCheck");
  }


  ///<summary>
  /// リソースを解放する
  ///</summary>
  public void Dispose()
  {
    try
    {
      DisconnectWebSocket();
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

  public  class GameEndSummaryNotification
  {
    public const string AllKey = "all";
    public const string EnemyKey = "enemy";
    public const string SkillKey = "skill";
    
    public Dictionary<string, SummaryDetail> SummaryDetails = new Dictionary<string, SummaryDetail>();
    
    public class SummaryDetail
    {
      public int count;
      public string viewer_id;
      public string viewer_name;
    }
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

///<summary>
/// WebSocketメッセージのタイプを表すEnum
///</summary>
public enum MessageType
{
  room_created,
  game_event,
  game_end_summary,
  unknown,
}

public interface IWebSocketManager
{
  ReadOnlyReactiveProperty<bool> IsConnectedProp { get; }
  IReadOnlyDictionary<FrontKey, Subject<Unit>> FrontEventDict { get; }
  WebSocketManager.GameEndSummaryNotification GameEndSummary { get; }
  UniTask ConnectWebSocketAsync(string websocketId = null, CancellationToken cancellationToken = default);
  void DisconnectWebSocket();
  string GetFrontUrl();
  UniTask GameStartAsync();
  UniTask GameEndAsync();
  void HealthCheck();
}