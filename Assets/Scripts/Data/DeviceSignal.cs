using System;

public class DeviceSignal
{
    public int BeanLightSign; //远光灯信号,1 打开，0 关闭
    public int BrakeSign; //脚刹制动器信号,1 踩下，0 未踩
    public string CarKeySign; //车钥匙信号,LOCK/OFF、ACC、START（发动机点火正时）、ON（发动起已启动）
    public int CarSpeed; //速度
    public int CautionLightSign; //警示灯信号（双闪）,1 打开，0 关闭
    public int CluthSign; //离合器信号,1 踩下，0 未踩
    public int DippedHeadLightSign; //近光灯信号,1 打开，0 关闭
    public string Dw; //档位
    public int EngineSpeed; //转速
    public int FrontFogLampSign; //前雾灯,1 打开，0 关闭
    public int Fxpzj; //方向盘转角，左转为正，右转为负,方向盘归正0,方向盘左打死5400.方向盘右打死-5400
    public int HandBrakeSign; //手刹信号,1 拉起，0 未拉
    public int LeftTurnIndicatorSign; //左转向灯信号,1 打开，0 关闭
    public int MainDriverDoorSign; //主架车门信号,1 打开，0 关闭
    public int RearFogLampSign; //后雾灯信号,1 打开，0 关闭
    public int RightTurnSign; //右转向灯信号,1 打开，0 关闭
    public int SafetyBeltSign; //安全带信号,1 插入，0 未插
    public int TrumpetSign; //喇叭信号,1 按下，0 未按
    public int WidthLampSign; //示宽灯信号,1 打开，0 关闭
    public string Time;
}
