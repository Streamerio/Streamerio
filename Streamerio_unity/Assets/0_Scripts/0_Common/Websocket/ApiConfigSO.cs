using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfigSO", menuName = "SO/Common/ApiConfigSO")]
public class ApiConfigSO : ScriptableObject
{
    [Header("フロントのURLフォーマット")]
    public string frontendUrlFormat = "https://streamerio.vercel.app/?streamer_id={0}";

    [Header("フロントのクエリパラメータフォーマット")]
    public string frontendQueryParamFormat = "{0}?room_id={1}";
    
    [Header("バックエンドのWebsocket用URL")]
    public string backendWsUrl = "wss://streamario-web-backend-282618030957.asia-northeast1.run.app/ws-unity";
    
    [Header("バックエンドのHTTP用URL")]
    public string backendHttpUrl = "https://streamario-web-backend-282618030957.asia-northeast1.run.app/";
    
    [Header("ゲーム終了時のレスポンス")]
    public string gameEndResponse = "{\"type\": \"game_end\" }";
}