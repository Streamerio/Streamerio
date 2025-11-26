using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDebugClickLogger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current == null)
            {
                Debug.LogWarning("EventSystem が存在しません。");
                return;
            }

            var pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results); // ←ここポイント

            if (results.Count == 0)
            {
                Debug.Log("UIヒットなし");
            }
            else
            {
                Debug.Log("UIヒット:");
                foreach (var r in results)
                    Debug.Log($"- {r.gameObject.name}");
            }
        }
    }
}