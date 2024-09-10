using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DataSimulator : MonoBehaviour
{
    private DateTime startTime; //以points_info_mini.json的第一条数据的时间作为基准
    private int statusStartIndex;

    public Transform Car; //汽车
    public Transform[] FrontWheelTrans; //汽车前轮
    public Transform[] BackWheelTrans; //汽车后轮
    public Transform SteeringWheel; //方向盘
    public Transform Speedometer; //仪表盘速度指针
    public Transform Tachometer; //仪表盘转速指针
    public GameObject OutsideCamera; //外部摄像头
    public GameObject InsideCamera; //内部摄像头

    private bool isCarForward; //汽车是否向前
    private float wheelRadius = 3.6f; //汽车轮子半径
    private float wheelcircumference;
    
    public class PosTweenData
    {
        public Vector3 TargetPos;
        public Quaternion TargetRot;
        public Quaternion WheelRollRot;
        public bool IsCarForward;
        public float TweenTime;
    }

    private List<PosTweenData> posTweenDatas = new();
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;
        // 获取开始时间，以 points_info_mini.json 的第一条数据为基准
        startTime = DateTime.Parse(DataInputer.Instance.PointDatas[0].Time);
        wheelcircumference = (float)(2 * Math.PI * wheelRadius);
        
        GetTweenData();
        SetFirstFramePos();
        SetFirstFrameStatus();
    
        // 开始模拟发送数据
        StartCoroutine(SendCarPosData());
        // StartCoroutine(SendCarStatusData());
    }
    
    private void SetFirstFramePos()
    {
        var origin = GeoConverter.LatLonToLocal(DataInputer.Instance.PointDatas[0].BasePoint.Lat,
            DataInputer.Instance.PointDatas[0].BasePoint.Lng);
        transform.localPosition = origin;
        transform.localEulerAngles = new Vector3(0, (float)DataInputer.Instance.PointDatas[0].CarHeardDirection, 0);
    }

    private void SetFirstFrameStatus()
    {
        var list = DataInputer.Instance.SignalDatas;
        // 找到 device_signal.json 中第一个时间大于等于 startTime 的条目
        statusStartIndex = 0;
        while (statusStartIndex < list.Count && DateTime.Parse(list[statusStartIndex].Time) < startTime)
        {
            statusStartIndex++;
        }

        var data = list[statusStartIndex];
        var angle = (float)Math.Floor((double)data.Fxpzj / 5400 * 22);
        // Debug.LogError($"前轮转向度数: {angle}");
        foreach (var tran in FrontWheelTrans)
        {
            tran.localRotation = Quaternion.AngleAxis(-angle, Vector3.up);
        }

        var fxpAngle = (float)Math.Floor((double)-data.Fxpzj / 5400 * 700);
        // Debug.LogError($"方向盘度数: {fxpAngle}");
        //SteeringWheel.localRotation = Quaternion.Euler(0, fxpAngle, 0);
        Speedometer.localRotation = Quaternion.Euler(0, data.CarSpeed * 10, 0);
        var tachometerAngle = (float)data.EngineSpeed * 240 / 8000;
        Tachometer.localRotation = Quaternion.Euler(0, tachometerAngle, 0);
    }

    private void GetTweenData()
    {
        var list = DataInputer.Instance.PointDatas;
        for (var i = 0; i < list.Count - 1; i++)
        {
            var data = new PosTweenData();
            data.TweenTime = (float)(DateTime.Parse(list[i + 1].Time) - DateTime.Parse(list[i].Time)).TotalMilliseconds / 1000;
            data.TargetPos = GeoConverter.LatLonToLocal(list[i + 1].BasePoint.Lat, list[i + 1].BasePoint.Lng);
            data.TargetRot = Quaternion.Euler(new Vector3(0, (float)list[i + 1].CarHeardDirection, 0));
            var lastPos = GeoConverter.LatLonToLocal(list[i].BasePoint.Lat, list[i].BasePoint.Lng);
            var rotationAngle = Vector3.Distance(lastPos, data.TargetPos) / wheelcircumference * 360;
            data.WheelRollRot = Quaternion.Euler(new Vector3(rotationAngle,0,0));
            var dot = Vector3.Dot(transform.forward, data.TargetPos - lastPos);
            data.IsCarForward = dot >= 0;
            posTweenDatas.Add(data);
        }
        Debug.Log(posTweenDatas.Count); 
    }
    
    private IEnumerator SendCarPosData()
    {
        var list = DataInputer.Instance.PointDatas;
        for (var i = 0; i < list.Count - 1; i++)
        {
            // 计算下一条数据的时间间隔
            var delay = (float)(DateTime.Parse(list[i + 1].Time) - DateTime.Parse(list[i].Time)).TotalMilliseconds / 1000;
            CarPos(list[i + 1], delay);
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator SendCarStatusData()
    {
        var list = DataInputer.Instance.SignalDatas;
        // 从找到的条目开始发送数据
        for (var i = statusStartIndex; i < list.Count - 1; i++)
        {
            // 计算下一条数据的时间间隔
            var delay = (float)(DateTime.Parse(list[i + 1].Time) - DateTime.Parse(list[i].Time)).TotalMilliseconds / 1000;
            CarStatus(list[i + 1], delay);
            yield return new WaitForSeconds(delay);
        }
    }

    private void CarPos(PointsInfo data, float duration)
    {
        var xyzPosition = GeoConverter.LatLonToLocal(data.BasePoint.Lat, data.BasePoint.Lng);
        // Debug.LogError($"local position: {xyzPosition}");
        // Debug.LogError($"car eulerAngles.y: {data.CarHeardDirection}");

        Car.DOMove(xyzPosition, duration).SetEase(Ease.Linear);;
        var carAngle = new Vector3(0, (float)data.CarHeardDirection, 0);
        //Car.DOLocalRotate(carAngle, duration);
        Car.DOLocalRotateQuaternion(Quaternion.Euler(carAngle), duration).SetEase(Ease.Linear);

        //var rotationAngle = (distance / wheelcircumference) * 2 * Math.PI;
        var dot = Vector3.Dot(transform.forward, xyzPosition - transform.position);
        isCarForward = dot >= 0;
        foreach (var wheel in FrontWheelTrans)
        {
            wheel.GetChild(0).DOLocalRotate(isCarForward ? new Vector3(360, 0, 0) : new Vector3(-360, 0, 0), duration,
                RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(
                -1, LoopType.Restart);
        }

        foreach (var wheel in BackWheelTrans)
        {
            wheel.DOLocalRotate(isCarForward ? new Vector3(360, 0, 0) : new Vector3(-360, 0, 0), duration,
                RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(
                -1, LoopType.Restart);
        }
    }

    private void CarStatus(DeviceSignal data, float duration)
    {
        var angle = (float)Math.Floor((double)data.Fxpzj / 5400 * 22);
        // Debug.LogError($"前轮转向度数: {angle}，弧度: {(float)(angle * Math.PI / 180)}");
        foreach (var tran in FrontWheelTrans)
        {
            tran.localEulerAngles =
                new Vector3(tran.localEulerAngles.x, -angle, 0);
        }

        var fxpAngle = (float)Math.Floor((double)-data.Fxpzj / 5400 * 700);
        SteeringWheel.DOLocalRotate(new Vector3(0, fxpAngle, 0), duration, RotateMode.LocalAxisAdd);
        // Debug.LogError($"方向盘度数: {fxpAngle}，弧度: {(float)(fxpAngle * Math.PI / 180)}");
        // Debug.LogError($"车速: {data.CarSpeed}");
        Speedometer.DOLocalRotate(new Vector3(0, data.CarSpeed * 10, 0), duration);
        // Debug.LogError($"转速: {data.EngineSpeed}");
        var tachometerAngle = (float)data.EngineSpeed * 240 / 8000;
        Tachometer.DOLocalRotate(new Vector3(0, tachometerAngle, 0), duration);
    }

    public void OnChangeViewBtnClick()
    {
        OutsideCamera.SetActive(!OutsideCamera.activeInHierarchy);
        InsideCamera.SetActive(!InsideCamera.activeInHierarchy);
    }
}