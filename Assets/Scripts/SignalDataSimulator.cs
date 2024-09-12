using System;
using System.Collections.Generic;
using UnityEngine;

public class SignalDataSimulator : MonoBehaviour
{
    public Transform[] FrontWheelTrans; //汽车前轮
    public Transform SteeringWheel; //方向盘
    public Transform Speedometer; //仪表盘速度指针
    public Transform Tachometer; //仪表盘转速指针
    public Transform LeftTurnIndicator; //左转向灯
    public Transform RightTurnIndicator; //右转向灯
    
    private int currentTweenIndex = -1; //当前正在处理的动画索引，-1表示没有动画正在播放  
    private float currentTweenTime = 0f; //当前动画的已用时间
    private float tweenTime; //动画时间，即到达目标位置所需的时间
    private StatusTweenData targetData;//目标数据
    private Action onTweenComplete;//动画完成时要调用的回调

    private class StatusTweenData
    {
        public float SteerLastAngle;
        public float SteerNextAngle;
        public Quaternion FrontWheelRot;
        public Quaternion SpeedometerRot;
        public Quaternion TachometerRot;
        public int LeftTurnLastSign;
        public int LeftTurnNextSign;
        public int RightTurnLastSign;
        public int RightTurnNextSign;
        public float TweenTime;
    }
    
    private List<StatusTweenData> statusTweenDatas = new();

    private float timer; 
    // Start is called before the first frame update
    void Start()
    {
        GetTweenData();
        
        // 开始播放第一个动画  
        if (statusTweenDatas.Count > 0)  
        {  
            currentTweenIndex = 0;
            SetTargetPos(statusTweenDatas[currentTweenIndex], OnTweenComplete);
        }
    }

    private void LateUpdate()
    {
        if (currentTweenIndex != -1)  
        {
            if (currentTweenTime < tweenTime)
            {
                // 计算插值比例  
                //var t = currentTweenTime / tweenTime;  
                var t = 0.02f /tweenTime;
                t = Mathf.Clamp01(t);  
  
                // 插值位置和旋转（这里假设 targetTransform 是当前动画的目标）  
                foreach (var tran in FrontWheelTrans)
                {
                    tran.localRotation = Quaternion.Lerp(tran.localRotation,targetData.FrontWheelRot, t);
                }

                var t2 = currentTweenTime / tweenTime; 
                float newAngle = Mathf.LerpAngle(targetData.SteerLastAngle, targetData.SteerNextAngle, t2);
                SteeringWheel.localRotation = Quaternion.Euler(0,newAngle, 0);
                Speedometer.localRotation = Quaternion.Lerp(Speedometer.localRotation,targetData.SpeedometerRot, t);
                Tachometer.localRotation = Quaternion.Lerp(Tachometer.localRotation,targetData.TachometerRot, t);
                
                // 更新当前动画的时间  
                currentTweenTime += Time.deltaTime;  
            }
            else
            {
                onTweenComplete?.Invoke();
            }
        }
    }

    private void SetTargetPos(StatusTweenData newData, Action onComplete = null)
    {
        targetData = newData;
        tweenTime = newData.TweenTime;
        currentTweenTime = 0f; //重置时间
        onTweenComplete = onComplete;
        var lGlint = LeftTurnIndicator.GetComponent<Glinting>();
        var rGlint = RightTurnIndicator.GetComponent<Glinting>();
        if (lGlint)
        {
            if (targetData.LeftTurnLastSign != targetData.LeftTurnNextSign)
            {
                if (targetData.LeftTurnNextSign == 1)
                    lGlint.StartGlinting();
                else
                    lGlint.StopGlinting();
            }
        }
        if (rGlint)
        {       
            if (targetData.RightTurnLastSign != targetData.RightTurnNextSign)
            {
                if (targetData.RightTurnNextSign == 1)
                {
                    rGlint.StartGlinting();
                }
                else
                    rGlint.StopGlinting();
            }
        }
    }
    
    private void OnTweenComplete()
    {
        currentTweenIndex++;
        // 检查是否还有动画要播放  
        if (currentTweenIndex >= statusTweenDatas.Count)  
            currentTweenIndex = -1;//没有更多动画，停止播放  
        else
            SetTargetPos(statusTweenDatas[currentTweenIndex],OnTweenComplete);
    }
    
    private void SetFirstFrameStatus(int index)
    {
        var list = DataInputer.Instance.SignalDatas;
        var data = list[index];
        
        var wheelAngle = (float)Math.Floor((double)-data.Fxpzj / 5400 * 22);
        var fxpAngle = (float)Math.Floor((double)-data.Fxpzj / 5400 * 700);
        var tachometerAngle = (float)data.EngineSpeed * 240 / 8000;
        foreach (var tran in FrontWheelTrans)
        {
            tran.localRotation = Quaternion.Euler(0,wheelAngle,0);
        }
        SteeringWheel.localRotation = Quaternion.Euler(0, fxpAngle, 0);
        Speedometer.localRotation = Quaternion.Euler(0, data.CarSpeed * 10, 0);
        Tachometer.localRotation = Quaternion.Euler(0, tachometerAngle, 0);
    }

    private void GetTweenData()
    {
        //获取开始时间，以points_info_mini.json的第一条数据为基准
        var startTime = DateTime.Parse(DataInputer.Instance.PointDatas[0].Time);
        var statusList = DataInputer.Instance.SignalDatas;
        //找到device_signal.json中第一个时间大于等于 startTime 的条目
        var statusStartIndex = 0;
        while (statusStartIndex < statusList.Count && DateTime.Parse(statusList[statusStartIndex].Time) < startTime)
        {
            statusStartIndex++;
        }
        
        for (var i = statusStartIndex; i < statusList.Count - 1; i++)
        {
            var data = new StatusTweenData();
            data.TweenTime = (float)(DateTime.Parse(statusList[i + 1].Time) - DateTime.Parse(statusList[i].Time)).TotalMilliseconds / 1000;
            data.SteerLastAngle = (float)Math.Floor((double)-statusList[i].Fxpzj / 5400 * 700);
            data.SteerNextAngle = (float)Math.Floor((double)-statusList[i + 1].Fxpzj / 5400 * 700);
            data.FrontWheelRot = Quaternion.Euler(new Vector3(0, (float)(Math.Floor((double)-statusList[i + 1].Fxpzj / 5400 * 22)), 0));
            data.SpeedometerRot = Quaternion.Euler(new Vector3(0, statusList[i + 1].CarSpeed * 10, 0));
            data.TachometerRot = Quaternion.Euler(new Vector3(0, (float)statusList[i + 1].EngineSpeed * 240 / 8000, 0));
            data.LeftTurnLastSign = statusList[i].LeftTurnIndicatorSign;
            data.LeftTurnNextSign = statusList[i + 1].LeftTurnIndicatorSign;
            data.RightTurnLastSign = statusList[i].RightTurnSign;
            data.RightTurnNextSign = statusList[i + 1].RightTurnSign;
            statusTweenDatas.Add(data);
        }

        SetFirstFrameStatus(statusStartIndex);
    }
}