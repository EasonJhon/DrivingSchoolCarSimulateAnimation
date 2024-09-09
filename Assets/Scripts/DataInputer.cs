using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class DataInputer : MonoBehaviour
{
    public static DataInputer Instance;

    public List<PointsInfo> PointDatas;
    public List<DeviceSignal> SignalDatas;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        //var pointStr = JsonLoader("points_info_mini.Json"); //定位数据
        var pointStr = JsonLoader("points_info_mini");
        //Debug.Log($"定位数据str:{pointStr}");
       
        PointDatas = JsonConvert.DeserializeObject<List<PointsInfo>>(pointStr);
        //Debug.Log($"定位数据数量:{PointDatas.Count}");
        
        //var signStr = JsonLoader("device_signal.Json"); //信号数据
        var signStr = JsonLoader("device_signal"); //信号数据
        //Debug.Log($"信号数据str:{signStr}");
        SignalDatas = JsonConvert.DeserializeObject<List<DeviceSignal>>(signStr);
        //Debug.Log($"信号数据数量:{SignalDatas.Count}");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }

    private string JsonLoader(string jsonName)
    {
#if UNITY_EDITOR
        //StreamReader reader = new StreamReader(Application.streamingAssetsPath + "/" + jsonName);
        TextAsset jsonFile = Resources.Load<TextAsset>(jsonName);
        string jsonString = jsonFile.text;
#elif UNITY_ANDROID

        TextAsset jsonFile = Resources.Load<TextAsset>(jsonName);
        string jsonString = jsonFile.text;
        //StreamReader reader = new StreamReader(Application.persistentDataPath + "/" + jsonName);
#endif
        // string jsonData = reader.ReadToEnd();
        // reader.Close();
        // reader.Dispose();
        // return jsonData;
        return jsonString;
    }
}