using System;
using System.Collections;
using UnityEngine;

public class DataSimulator : MonoBehaviour
{
    private DateTime startTime; // 以 points_info_mini.json 的第一条数据的时间作为基准

    private bool isPosTween = false;
    private bool isBackCar;
    private float posTimer;
    private Vector3 tweenStartPos;
    private Vector3 tweenEndPos;
    private Vector3 tweenStartEulerAngle;
    private Vector3 tweenEndEulerAngle;
    private float tweenPosDuration;

    public Transform[] WheelTrans;

    private float wheelSpeed = 90f;

    private bool isStatusTween = false;
    private float statusTimer;
    private float TweenWheelStartEulerAngle;
    private float TweenWheelEndEulerAngle;
    private float TweenWheelEndRotationAngle;
    private float tweenStatusDuration;

    public Transform FXP;

    private float TweenFXPEndAngle;
    // Start is called before the first frame update
    void Start()
    {
        // 获取开始时间，以 points_info_mini.json 的第一条数据为基准
        startTime = DateTime.Parse(DataInputer.Instance.PointDatas[0].Time);
        // 开始模拟发送数据
        StartCoroutine(SendCarPosData());
        StartCoroutine(SendCarStatusData());
        var origin = GeoConverter.LatLonToLocal(DataInputer.Instance.PointDatas[0].BasePoint.Lat,
            DataInputer.Instance.PointDatas[0].BasePoint.Lng);
        transform.localPosition = origin;
        transform.localEulerAngles = new Vector3(0, (float)DataInputer.Instance.PointDatas[0].CarHeardDirection, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPosTween)
        {
            posTimer += Time.deltaTime / tweenPosDuration;
            if (posTimer <= tweenPosDuration)
                PosLerp(tweenStartPos, tweenEndPos, tweenStartEulerAngle, tweenEndEulerAngle, posTimer);
            else
            {
                isPosTween = false;
                posTimer = 0;
            }

            if (isBackCar)
            {
                WheelBack();
            }
            else
            {
                WheelForward();
            }
        }

        if (isStatusTween)
        {
            statusTimer+= Time.deltaTime / tweenStatusDuration;
            if (statusTimer <= tweenStatusDuration)
            {
                //WheelDireLerp(TweenWheelStartEulerAngle, TweenWheelEndEulerAngle, statusTimer);
            }
            
            else
            {
                isStatusTween = false;
                statusTimer = 0;
            }
            
            float anglePerSecond = TweenFXPEndAngle / tweenStatusDuration;
            FXP.Rotate(new Vector3(0, anglePerSecond, 0));
        }
    }

    private void FixedUpdate()
    {
    }

    IEnumerator SendCarPosData()
    {
        for (int i = 0; i < DataInputer.Instance.PointDatas.Count - 1; i++)
        {
            // 计算下一条数据的时间间隔
            float delay =
                (float)(DateTime.Parse(DataInputer.Instance.PointDatas[i + 1].Time) -
                        DateTime.Parse(DataInputer.Instance.PointDatas[i].Time)).TotalMilliseconds / 1000;
            yield return new WaitForSeconds(delay);
            
            CarPos(DataInputer.Instance.PointDatas[i], delay);
        }
    }

    IEnumerator SendCarStatusData()
    {
        // 找到 device_signal.json 中第一个时间大于等于 startTime 的条目
        int startIndex = 0;
        while (startIndex < DataInputer.Instance.SignalDatas.Count &&
               DateTime.Parse(DataInputer.Instance.SignalDatas[startIndex].Time) < startTime)
        {
            startIndex++;
        }

        // 从找到的条目开始发送数据
        for (int i = startIndex; i < DataInputer.Instance.SignalDatas.Count - 1; i++)
        {
            // 计算下一条数据的时间间隔
            float delay =
                (float)(DateTime.Parse(DataInputer.Instance.SignalDatas[i + 1].Time) -
                        DateTime.Parse(DataInputer.Instance.SignalDatas[i].Time)).TotalMilliseconds / 1000;
            yield return new WaitForSeconds(delay);
            
            CarStatus(DataInputer.Instance.SignalDatas[i],delay);
        }
    }

    void CarPos(PointsInfo data, float duration)
    {
        // 在这里实现 carPos 的逻辑
        //Debug.LogError($"car point{data.BasePoint.Lat},{data.BasePoint.Lng} at {data.Time}");
        var xyzPosition = GeoConverter.LatLonToLocal(data.BasePoint.Lat, data.BasePoint.Lng);
        //Debug.LogError($"{xyzPosition.x} ,{xyzPosition.y},{xyzPosition.z}");
        //Debug.LogError(data.CarHeardDirection);
        isPosTween = true;
        tweenStartPos = transform.localPosition;
        tweenEndPos = xyzPosition;
        tweenStartEulerAngle = transform.localEulerAngles;
        tweenEndEulerAngle = new Vector3(0, (float)data.CarHeardDirection, 0);
        tweenPosDuration = duration;
        
        float dot = Vector3.Dot(transform.forward, xyzPosition - transform.position); 
        if (dot < 0)
        {
            isBackCar = true;
        }
        else
        {
            isBackCar = false;
        }
    }

    void CarStatus(DeviceSignal data,float duration)
    {
        // 在这里实现 carStatus 的逻辑
        //Debug.LogError($"Car status: {data.Fxpzj} at {data.Time}");
        isStatusTween = true;
        TweenWheelStartEulerAngle = WheelTrans[0].localEulerAngles.y;
        var angle = Math.Floor((double)data.Fxpzj / 5400 * 22);

        TweenWheelEndEulerAngle = (float)angle; //(float)(angle * Math.PI / 180);
        Debug.LogError($"度数：{angle}，弧度：{(float)(angle * Math.PI / 180)}");

        foreach (var tran in WheelTrans)
        {
            tran.localEulerAngles =
                new Vector3(tran.localEulerAngles.x, -TweenWheelEndEulerAngle, tran.localEulerAngles.z);
        }
        
        tweenStatusDuration = duration;
        var fxpAngle = Math.Floor((double)-data.Fxpzj / 5400 * 700);
        TweenFXPEndAngle = (float)(fxpAngle * Math.PI / 180);
    }

    private void PosLerp(Vector3 startPos, Vector3 endPos, Vector3 startAngle, Vector3 endAngle, float t)
    {
        transform.localPosition = Vector3.Lerp(startPos, endPos, t);
        transform.localEulerAngles = Vector3.Lerp(startAngle, endAngle, t);
    }
    
    private void WheelForward()
    {
        foreach (var tran in WheelTrans)
        {
            tran.Rotate(new Vector3(Time.deltaTime * wheelSpeed, 0, 0));
        }
    }

    private void WheelBack()
    {
        foreach (var tran in WheelTrans)
        {
            tran.Rotate(new Vector3(-Time.deltaTime * wheelSpeed, 0, 0));
        }
    }

    private void WheelDireLerp(float start, float end, float t)
    {
        if (end >= 0)
        {
            if (WheelTrans[0].localEulerAngles.y >= end)
            {
                return;
            }

            WheelTrans[0].Rotate(0,20*Time.deltaTime,0);
            WheelTrans[1].Rotate(0,20*Time.deltaTime,0);
        }
        else
        {
            if (WheelTrans[0].localEulerAngles.y <= end)
            {
                return;
            }
            WheelTrans[0].Rotate(0,-20*Time.deltaTime,0);
            WheelTrans[1].Rotate(0,-20*Time.deltaTime,0);
        }
    }
}