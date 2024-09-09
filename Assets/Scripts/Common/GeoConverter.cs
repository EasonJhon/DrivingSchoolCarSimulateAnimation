using UnityEngine;

public static class GeoConverter
{
    private const double originLat= 40.606229;
    private const double originLon= 109.842232;
    private const double originHeight = 0;
    private const double R = 6378137; // 地球半径（单位：米）
    
    // 经纬度转换为局部XYZ坐标
    public static Vector3 LatLonToLocal(double lat, double lon, double height = 0)
    {
        // 计算纬度和经度的差异，单位为弧度
        double dLat = (lat - originLat) * Mathf.Deg2Rad;
        double dLon = (lon - originLon) * Mathf.Deg2Rad;

        // 计算局部X、Z坐标
        double x = R * dLon * Mathf.Cos((float)(originLat * Mathf.Deg2Rad));
        double z = R * dLat;
        
        // 高度差在Y轴
        double y = height - originHeight;

        // 返回局部XYZ坐标
        return new Vector3((float)x, (float)y, (float)z);
    }
}
