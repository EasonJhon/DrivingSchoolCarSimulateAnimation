using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

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

        InitialData();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private string JsonLoaderExternal(string jsonName)
    {
#if UNITY_EDITOR
        var reader = new StreamReader(Application.streamingAssetsPath + "/" + jsonName);
#elif UNITY_ANDROID
        var reader = new StreamReader(Application.persistentDataPath + "/" + jsonName); //读取不到
#endif
        var jsonData = reader.ReadToEnd();
        reader.Close();
        reader.Dispose();
        return jsonData;
    }

    private string JsonLoaderInternal(string jsonName)
    {
        var jsonFile = Resources.Load<TextAsset>(jsonName);
        var jsonString = jsonFile.text;
        return jsonString;
    }

    private void InitialData()
    {
        var pointStr = JsonLoaderInternal("points_info_mini"); //定位数据
        var signStr = JsonLoaderInternal("device_signal"); //信号数据
        // var pointStr = JsonLoaderExternal("points_info_mini.Json");
        // var signStr = JsonLoaderExternal("device_signal.Json");
        PointDatas = JsonConvert.DeserializeObject<List<PointsInfo>>(pointStr);
        SignalDatas = JsonConvert.DeserializeObject<List<DeviceSignal>>(signStr);
        
        Debug.Log($"定位数据str:{pointStr}");
        Debug.Log($"定位数据数量:{PointDatas.Count}");
        Debug.Log($"信号数据str:{signStr}");
        Debug.Log($"信号数据数量:{SignalDatas.Count}");
    }
}