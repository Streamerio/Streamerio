using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using MiniJSON;

namespace Common.GAS
{
    public static class SpreadSheetClient
    {
        private const string _getParameterKey = "dataType";
        
        // スプレッドシートのデータを辞書型で取得
        public static async UniTask<Dictionary<string, List<object>>> GetRequestAsync(SheetType sheetType, CancellationToken ct, int timeout = 0)
        {
            var uriBuilder = new UriBuilder(Env.Parameter.GasApiUrl)
            {
                Query = CreateGetQuery(sheetType)
            };
            
            using (UnityWebRequest request = UnityWebRequest.Get(uriBuilder.Uri))
            {
                try
                {
                    if(timeout > 0)
                    {
                        request.timeout = timeout;
                    }
                    
                    await request.SendWebRequest().ToUniTask(cancellationToken: ct);
                    
                    Debug.Log("GET Request Success");
                    Debug.Log(request.downloadHandler.text);
                    var data = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
                    
                    var result = new Dictionary<string, List<object>>();
                    foreach (var kv in data)
                    {
                        result[kv.Key] = kv.Value as List<object>;
                    }
                    
                    return result;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    Console.WriteLine(e);
                    return new Dictionary<string, List<object>>();
                }
            }
        }

        private static string CreateGetQuery(SheetType sheetType)
        {
            var query = System.Web.HttpUtility.ParseQueryString("");
            query[_getParameterKey] = sheetType.ToString();
            
            return query.ToString();
        }
    }
}