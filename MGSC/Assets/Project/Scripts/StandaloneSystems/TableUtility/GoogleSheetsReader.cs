using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace TableUtility
{
    [CreateAssetMenu(fileName = "GoogleSheetsReader", menuName = "Scriptable Objects/GoogleSheetsReader")]
    public class GoogleSheetsReader : TableReader
    {
        public string apiKey = "AIzaSyDq_y0Ury1MwseRCz_74yjnphff8Zdz660";
        [TextArea(3, 5)]
        public string URL = "https://docs.google.com/spreadsheets/d/1GTT_VfAFWWEV3J3N5aC4TKfn7XIgHKmxa2iLiuswrmw/edit?gid=1107546989#gid=1107546989";
        [HideInInspector] public string spreadsheetId = "1GTT_VfAFWWEV3J3N5aC4TKfn7XIgHKmxa2iLiuswrmw";
        public string range = "Sheet1!A1:C10";

        public override TableData ReadSheet()
        {
            ReadSpreadsheet();
            return Result;
        }

        public async void ReadSpreadsheet()
        {
            spreadsheetId = GetIDFromURL(URL);
            string url = $"https://sheets.googleapis.com/v4/spreadsheets/{spreadsheetId}/values/{range}?key={apiKey}";

            using UnityWebRequest req = UnityWebRequest.Get(url);

            var operation = req.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + req.error +
                    "\nIf it is a 400 error, make sure your URL and sheet ranges are correct.");
                return;
            }

            string json = req.downloadHandler.text;

            Result = JToken.Parse(json).ToObject<TableData>();
            OnReadingComplete?.Invoke();
            return;
        }
        
        public string GetIDFromURL(string url)
        {
            var elements = url.Split("/");
            if (elements.Length >= 6 )
                return elements[5];
            else return "";
        }
    }

}