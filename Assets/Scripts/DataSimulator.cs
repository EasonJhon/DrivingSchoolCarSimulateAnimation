using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class DataSimulator : MonoBehaviour
{
    private DateTime startTime; //以points_info_mini.json的第一条数据的时间作为基准

    public Transform Car; //汽车
    public Transform[] FrontWheelTrans; //汽车前轮
    public Transform[] BackWheelTrans; //汽车后轮
    public Transform SteeringWheel; //方向盘
    public Transform Speedometer; //仪表盘速度指针
    public Transform Tachometer; //仪表盘转速指针
    public GameObject OutsideCamera; //外部摄像头
    public GameObject InsideCamera; //内部摄像头
    
    private bool isCarForward; //汽车是否向前
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
    }

    private void FixedUpdate()
    {
    }

    private IEnumerator SendCarPosData()
    {
        for (var i = 0; i < DataInputer.Instance.PointDatas.Count - 1; i++)
        {
            // 计算下一条数据的时间间隔
            var delay =
                (float)(DateTime.Parse(DataInputer.Instance.PointDatas[i + 1].Time) -
                        DateTime.Parse(DataInputer.Instance.PointDatas[i].Time)).TotalMilliseconds / 1000;
            yield return new WaitForSeconds(delay);

            CarPos(DataInputer.Instance.PointDatas[i], delay);
        }
    }

    IEnumerator SendCarStatusData()
    {
        // 找到 device_signal.json 中第一个时间大于等于 startTime 的条目
        var startIndex = 0;
        while (startIndex < DataInputer.Instance.SignalDatas.Count &&
               DateTime.Parse(DataInputer.Instance.SignalDatas[startIndex].Time) < startTime)
        {
            startIndex++;
        }

        // 从找到的条目开始发送数据
        for (var i = startIndex; i < DataInputer.Instance.SignalDatas.Count - 1; i++)
        {
            // 计算下一条数据的时间间隔
            var delay =
                (float)(DateTime.Parse(DataInputer.Instance.SignalDatas[i + 1].Time) -
                        DateTime.Parse(DataInputer.Instance.SignalDatas[i].Time)).TotalMilliseconds / 1000;
            yield return new WaitForSeconds(delay);

            CarStatus(DataInputer.Instance.SignalDatas[i], delay);
        }
    }

    private void CarPos(PointsInfo data, float duration)
    {
        // Debug.LogError($"car point lat: {data.BasePoint.Lat}, lng: {data.BasePoint.Lng} at {data.Time}");
        var xyzPosition = GeoConverter.LatLonToLocal(data.BasePoint.Lat, data.BasePoint.Lng);
        // Debug.LogError($"local position: {xyzPosition}");
        // Debug.LogError($"car eulerAngles.y: {data.CarHeardDirection}");
        Car.DOKill();
        Car.DOMove(xyzPosition, duration);
        var carAngle = new Vector3(0, (float)data.CarHeardDirection, 0);
        Car.DOLocalRotate(carAngle, duration);

        var dot = Vector3.Dot(transform.forward, xyzPosition - transform.position);
        isCarForward = dot >= 0;
        foreach (var wheel in FrontWheelTrans)
        {
            wheel.GetChild(0).DOKill();
            wheel.GetChild(0).DOLocalRotate(isCarForward ? new Vector3(360, 0, 0) : new Vector3(-360, 0, 0), duration,
                RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(
                -1, LoopType.Restart);
        }
        foreach (var wheel in BackWheelTrans)
        {
            wheel.DOKill();
            wheel.DOLocalRotate(isCarForward ? new Vector3(360, 0, 0) : new Vector3(-360, 0, 0), duration,
                RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(
                -1, LoopType.Restart);
        }
    }

    private void CarStatus(DeviceSignal data, float duration)
    {
        //Debug.LogError($"car status: {data.Fxpzj} at {data.Time}");
        var angle = (float)Math.Floor((double)data.Fxpzj / 5400 * 22);
        // Debug.LogError($"前轮转向度数: {angle}，弧度: {(float)(angle * Math.PI / 180)}");
        foreach (var tran in FrontWheelTrans)
        {
            tran.localEulerAngles =
                new Vector3(tran.localEulerAngles.x, -angle, 0);
        }
        
        var fxpAngle = (float)Math.Floor((double)-data.Fxpzj / 5400 * 700);
        SteeringWheel.DOKill();
        SteeringWheel.DOLocalRotate(new Vector3(0,fxpAngle,0), duration,RotateMode.LocalAxisAdd);
        // Debug.LogError($"方向盘度数: {fxpAngle}，弧度: {(float)(fxpAngle * Math.PI / 180)}");
        // Debug.LogError($"车速: {data.CarSpeed}");
        Speedometer.DOKill();
        Speedometer.DOLocalRotate(new Vector3(0, data.CarSpeed * 10, 0), duration);
        // Debug.LogError($"转速: {data.EngineSpeed}");
        Tachometer.DOKill();
        var tachometerAngle = (float)data.EngineSpeed * 240 / 8000;
        Tachometer.DOLocalRotate(new Vector3(0, tachometerAngle, 0), duration);
    }

    public void OnChangeViewBtnClick()
    {
        OutsideCamera.SetActive(!OutsideCamera.activeInHierarchy);
        InsideCamera.SetActive(!InsideCamera.activeInHierarchy);
    }
}