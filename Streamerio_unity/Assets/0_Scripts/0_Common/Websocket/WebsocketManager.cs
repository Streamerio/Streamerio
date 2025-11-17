using System;
using System.Collections.Generic;
using Common;
using Cysharp.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;
using R3;
public class WebsocketManager : SingletonBase<WebsocketManager>
{
  private bool _isConnected = false;
  private WebSocket _websocket;

  [SerializeField]
  private string _websocketId = string.Empty;
  public string WebsocketId => _websocketId;
  
  private Dictionary<FrontKey, Subject<Unit>> _frontEventDict = new Dictionary<FrontKey, Subject<Unit>>();
  public IDictionary<FrontKey, Subject<Unit>> FrontEventDict => _frontEventDict;
  
  private string _url = string.Empty;
  
  private string _frontendUrlFormat = "https://streamerio.vercel.app/?streamer_id={0}";
  
  private void Awake()
  {
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
  public async UniTask ConnectWebSocket()
  {
    if (_isConnected)
    {
      Debug.Log("WebSocket is already connected!");
      return;
    }
    
    // WebSocketのインスタンスを生成
    _websocket = new WebSocket("wss://streamerio-282618030957.asia-northeast1.run.app/ws-unity");

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

    _websocket.OnClose += (e) =>
    {
      Debug.Log("Connection closed!");
      _isConnected = false;
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
          _websocketId = room.room_id;
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
  public async void DisconnectWebSocket()
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
    if (_url != string.Empty)
    {
      return _url;
    }
    
    await UniTask.WaitWhile(() => _websocketId == string.Empty);
    _url = ZString.Format(_frontendUrlFormat, _websocketId);
    
    return _url;
  }

  ///<summary>
  /// ゲーム終了通知
  ///</summary>
  public async UniTask GameEnd()
  {
    await SendWebSocketMessage( "{\"type\": \"game_end\" }" );
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
  /// アプリケーションが終了したときにwebsocketを閉じる
  ///</summary>
  private async void OnApplicationQuit()
  {
    await _websocket.Close();
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
