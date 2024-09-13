using System;
using System.Collections.Generic;
using UnityEngine;

public class DataSimulator : MonoBehaviour
{
    public Transform Car; //汽车
    public Transform[] FrontWheelTrans; //汽车前轮
    public Transform[] BackWheelTrans; //汽车后轮
    public GameObject OutsideCamera; //外部摄像头
    public GameObject InsideCamera; //内部摄像头
    public float WheelRadius = 0.6f;
    public float WheelCircumference;

    private int currentTweenIndex = -1; //当前正在处理的动画索引，-1表示没有动画正在播放  
    private float currentTweenTime = 0f; //当前动画的已用时间
    private float tweenTime; //动画时间，即到达目标位置所需的时间
    private CarTweenData targetData; //目标数据
    private Action onTweenComplete; //动画完成时要调用的回调

    private class CarTweenData
    {
        public Vector3 TargetPos;
        public Quaternion TargetRot;
        public float WheelAngle;
        public float TweenTime;
    }

    private List<CarTweenData> posTweenDatas = new();

    // Start is called before the first frame update
    void Start()
    {
        WheelCircumference = 2 * Mathf.PI * WheelRadius;
        GetTweenData();

        // 开始播放第一个动画  
        if (posTweenDatas.Count > 0)
        {
            currentTweenIndex = 0;
            SetTargetPos(posTweenDatas[currentTweenIndex], OnTweenComplete);
        }
    }

    private void FixedUpdate()
    {
        if (currentTweenIndex != -1)
        {
            if (currentTweenTime < tweenTime)
            {
                // 计算插值比例  
                //var t = currentTweenTime / tweenTime;
                var t = 0.02f / tweenTime;
                t = Mathf.Clamp01(t);

                // 插值位置和旋转（这里假设 targetTransform 是当前动画的目标）  
                Car.position = Vector3.Lerp(Car.position, targetData.TargetPos, t);
                Car.rotation = Quaternion.Lerp(Car.rotation, targetData.TargetRot, t);

                if (Vector3.Distance(Car.position, targetData.TargetPos) > 0.001f)
                {
                    var dot = Vector3.Dot(targetData.TargetPos - Car.position, new Vector3(0, 0, -1));
                    foreach (var wheel in FrontWheelTrans)
                    {
                        wheel.GetChild(0)
                            .Rotate(new Vector3(dot > 0 ? targetData.WheelAngle / 2 : -targetData.WheelAngle / 2, 0,
                                0));
                    }

                    foreach (var wheel in BackWheelTrans)
                    {
                        wheel.Rotate(
                            new Vector3(dot > 0 ? targetData.WheelAngle / 2 : -targetData.WheelAngle / 2, 0, 0));
                    }
                }

                // 更新当前动画的时间  
                currentTweenTime += Time.deltaTime;
            }
            else
            {
                onTweenComplete?.Invoke();
            }
        }
    }

    private void SetTargetPos(CarTweenData newData, Action onComplete = null)
    {
        targetData = newData;
        tweenTime = newData.TweenTime;
        currentTweenTime = 0f; //重置时间
        onTweenComplete = onComplete;
    }

    private void OnTweenComplete()
    {
        currentTweenIndex++;
        // 检查是否还有动画要播放  
        if (currentTweenIndex >= posTweenDatas.Count)
            currentTweenIndex = -1; //没有更多动画，停止播放  
        else
            SetTargetPos(posTweenDatas[currentTweenIndex], OnTweenComplete);
    }

    private void SetFirstFramePos()
    {
        var origin = GeoConverter.LatLonToLocal(DataInputer.Instance.PointDatas[0].BasePoint.Lat,
            DataInputer.Instance.PointDatas[0].BasePoint.Lng);
        Car.localPosition = origin;
        Car.localRotation =
            Quaternion.Euler(new Vector3(0, (float)DataInputer.Instance.PointDatas[0].CarHeardDirection, 0));
    }

    private void GetTweenData()
    {
        var posList = DataInputer.Instance.PointDatas;
        for (var i = 0; i < posList.Count - 1; i++)
        {
            var data = new CarTweenData();
            data.TweenTime =
                (float)(DateTime.Parse(posList[i + 1].Time) - DateTime.Parse(posList[i].Time)).TotalMilliseconds / 1000;
            data.TargetPos = GeoConverter.LatLonToLocal(posList[i + 1].BasePoint.Lat, posList[i + 1].BasePoint.Lng);
            data.TargetRot = Quaternion.Euler(new Vector3(0, (float)posList[i + 1].CarHeardDirection, 0));
            var lastPos = GeoConverter.LatLonToLocal(posList[i].BasePoint.Lat, posList[i].BasePoint.Lng);
            var distance = Vector3.Distance(lastPos, data.TargetPos);
            var wheelDistance = distance / WheelCircumference;
            data.WheelAngle = wheelDistance * 360;
            posTweenDatas.Add(data);
        }

        SetFirstFramePos();
    }

    public void OnChangeViewBtnClick()
    {
        OutsideCamera.SetActive(!OutsideCamera.activeInHierarchy);
        InsideCamera.SetActive(!InsideCamera.activeInHierarchy);
    }
}