using System;
using UnityEngine;

public class PointsInfo
{
    public Point BasePoint;
    public double BasePointAngl;
    public double CarHeardDirection;
    public string Time;

    public class Point
    {
        public double Lat;
        public double Lng;
        public int PointBh;
    }
}
