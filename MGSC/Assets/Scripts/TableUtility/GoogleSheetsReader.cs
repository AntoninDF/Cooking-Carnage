using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json.Linq;

namespace TableUtility
{

    public class GoogleSheetReader : MonoBehaviour
    {
        public string apiKey = "AIzaSyDq_y0Ury1MwseRCz_74yjnphff8Zdz660";
        [TextArea(3,5)]
        public string spreadsheetId = "1GTT_VfAFWWEV3J3N5aC4TKfn7XIgHKmxa2iLiuswrmw";
        public string range = "Sheet1!A1:Z100";

        public void Start()
        {
            StartCoroutine(ReadSheet());
        }

        IEnumerator ReadSheet()
        {
            string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}?key={apiKey}";

            using UnityWebRequest req = UnityWebRequest.Get(url);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + req.error);
                yield break;
            }

            Debug.Log("Sheet data: " + req.downloadHandler.text);

            TableData extractedData = JToken.Parse(req.downloadHandler.text).ToObject<TableData>();
            Debug.Log(extractedData);
            Debug.Log(extractedData.range);
            foreach (var row in extractedData.values)
                foreach (var cell in row)
                    Debug.Log(cell);
        }

    
    }

}