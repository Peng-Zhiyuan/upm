using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/******************************************************************************** 
** auth：    FengLinyi
** date：    2018/09/01
** desc：    A*算法的实现
** Ver.:     V1.0.0
*********************************************************************************/

public class AStar
{
    /// <summary>
    /// 二维坐标点
    /// </summary>
    public struct Point
    {
        public int x, y;
        public Point(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
    }
    /// <summary>
    /// A*的每个节点
    /// </summary>
    public class ANode
    {
        public Point point;
        public ANode parent;
        public int fn, gn, hn;
    }
    private AStar() { }
    public static AStar Instance { get; } = new AStar();
    private int[,] map = null;
    private Dictionary<Point, ANode> openList = null;
    private HashSet<Point> closedList = null;
    private Point dist;
    private int mapWidth = 0;
    private int mapHeight = 0;
    //private int reachableVal;

    /// <summary>
    /// 执行算法
    /// </summary>
    /// <param name="map">二维网格地图</param>
    /// <param name="srcX">当前点X坐标</param>
    /// <param name="srcY">当前点Y坐标</param>
    /// <param name="distX">目标点X坐标</param>
    /// <param name="distY">目标点Y坐标</param>
    public ANode Execute(int[,] map, int width, int height,int srcX, int srcY, int distX, int distY, bool allowDiagonal = false)
    {
        openList = new Dictionary<Point, ANode>();
        closedList = new HashSet<Point>();
        this.map = map;
        this.mapWidth = width;
        this.mapHeight = height;
        this.dist = new Point(distX, distY);
        //this.reachableVal = reachableVal;
        //将初始节点加入到open列表中
        ANode aNode = new ANode();
        aNode.point = new Point(srcX, srcY);
        aNode.parent = null;
        aNode.gn = 0;
        aNode.hn = ManHattan(aNode.point, dist);
        aNode.fn = aNode.gn + aNode.hn;
        openList.Add(aNode.point, aNode);

        while (openList.Count > 0)
        {
            //从open列表中找到f(n)最小的结点
            ANode minFn = FindMinFn(openList);
            Point point = minFn.point;
            //判断是否到达终点
            if (point.x == dist.x && point.y == dist.y) return minFn;
            //去除minFn，加入到closed列表中
            openList.Remove(minFn.point);
            closedList.Add(minFn.point);
            //将minFn周围的节点加入到open列表中
            AddToOpenList(new Point(point.x - 1, point.y), minFn); //左
            AddToOpenList(new Point(point.x + 1, point.y), minFn); //右
            AddToOpenList(new Point(point.x, point.y - 1), minFn); //上
            AddToOpenList(new Point(point.x, point.y + 1), minFn); //下
            if (allowDiagonal)
            {
                AddToOpenList(new Point(point.x - 1, point.y - 1), minFn); //左上
                AddToOpenList(new Point(point.x + 1, point.y - 1), minFn); //右上
                AddToOpenList(new Point(point.x - 1, point.y + 1), minFn); //左下
                AddToOpenList(new Point(point.x + 1, point.y + 1), minFn); //右下
            }
        }
        return null;
    }

    /// <summary>
    /// 输出最短路径
    /// </summary>
    /// <param name="aNode"></param>
    public void DisplayPath(ANode aNode)
    {
        while (aNode != null)
        {
            Console.WriteLine(aNode.point.x + "," + aNode.point.y);
            aNode = aNode.parent;
        }
    }

    /// <summary>
    /// 获取最短路径
    /// </summary>
    /// <param name="aNode"></param>
    public List<ANode> GetPathList(ANode aNode)
    {
        List<ANode> nodes = new List<ANode>();
        while (aNode != null)
        {
            nodes.Add(aNode);
            Console.WriteLine(aNode.point.x + "," + aNode.point.y);
            aNode = aNode.parent;
        }
        nodes.Reverse();
        return nodes;
    }

    /// <summary>
    /// 判断节点是否可达，可达则将节点加入到open列表中
    /// </summary>
    /// <param name="a"></param>
    /// <param name="parent"></param>
    private void AddToOpenList(Point point, ANode parent)
    {
        if (IsReachable(point) && !closedList.Contains(point))
        {
            ANode aNode = new ANode();
            aNode.point = point;
            aNode.parent = parent;
            aNode.gn = parent.gn + this.map[point.y, point.x];
            aNode.hn = ManHattan(point, dist);
            aNode.fn = aNode.gn + aNode.hn;
            if (openList.ContainsKey(aNode.point))
            {
                if (aNode.fn < openList[aNode.point].fn)
                {
                    openList[aNode.point] = aNode;
                }
            }
            else
                openList.Add(aNode.point, aNode);
        }
    }

    /// <summary>
    /// 判定该点是否可达
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    private bool IsReachable(Point a)
    {
        if (a.y < 0 || a.x < 0 || a.y >= this.mapHeight || a.x >= this.mapWidth) return false;
        return map[a.y, a.x] >= 0;
    }

    /// <summary>
    /// 计算两个点之间的曼哈顿距离
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int ManHattan(Point a, Point b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    /// <summary>
    /// 从open列表中获取最小f(n)的节点
    /// </summary>
    /// <param name="aNodes"></param>
    /// <returns></returns>
    private ANode FindMinFn(Dictionary<Point, ANode> aNodes)
    {
        ANode minANode = null;
        foreach (var e in aNodes)
        {
            if (minANode == null || e.Value.fn < minANode.fn)
            {
                minANode = e.Value;
            }
        }
        return minANode;
    }
}