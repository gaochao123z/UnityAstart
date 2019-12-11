using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathPoint
{
    public int X;
    public int Y;
    public int F, G, H;
    public PathPoint parent;
    public bool isObstacle = false;
    public Transform m_Self;
    public PathPoint(int x, int y, Transform self)
    {
        this.X = x;
        this.Y = y;
        m_Self = self;
    }

    public void SetParent(PathPoint p, int g)
    {
        parent = p;
        this.G = g;
        this.F = G + H;
    }
}

public class AStarController_3D : MonoBehaviour
{
    //A星 核心算法 公式 F =G+H   F:通过路径总代价 G:起始点到当前点代价 H：当前点到目标点击代价
    //查找周围8个方向 找到最小路径点
    //用2个list openList 表示未计算过代价点  closeList表示已经计算过的代价点
    //1.         把起点加入 open list 。

    // 2.         重复如下过程：

    // a.遍历 open list ，查找 F 值最小的节点，把它作为当前要处理的节点。

    //    b.把这个节点移到 close list 。

    //c.对当前方格的 8 个相邻方格的每一个方格？

    //◆     如果它是不可抵达的或者它在 close list 中，忽略它。否则，做如下操作。

    //◆     如果它不在 open list 中，把它加入 open list ，并且把当前方格设置为它的父亲，记录该方格的 F ， G 和 H 值。

    //◆     如果它已经在 open list 中，检查这条路径(即经由当前方格到达它那里 ) 是否更好，用 G 值作参考。更小的 G 值表示这是更好的路径。如果是这样，把它的父亲设置为当前方格，并重新计算它的 G 和 F 值。如果你的 open list 是按 F 值排序的话，改变后你可能需要重新排序。

    //          d.停止，当你

    //◆     把终点加入到了 open list 中，此时路径已经找到了，或者

    //◆     查找终点失败，并且 open list 是空的，此时没有路径。

    //3.         保存路径。从终点开始，每个方格沿着父节点移动直至起点，这就是你的路径。
    public int width = 10;
    public int height = 10;

    public int girdWidth = 5;
    public int girdHeight = 5;
    public GameObject prefab;
    public GameObject PathRoot;
    PathPoint[,] map = null;

    public Transform startTrans;
    public Transform targetTrans;
    PathPoint startPoint;
    PathPoint endPoint;
    PathPoint last_endPoint;

    Renderer[,] mPathMaterial;

    //寻路路径
    List<PathPoint> pathList;

    public Button mBtnMove;
    public void Start()
    {
        mBtnMove.onClick.AddListener(OnBtnStartMove);
        InitMap();


    }

    public void InitMap()
    {


        map = new PathPoint[width, height];
        mPathMaterial = new MeshRenderer[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float x = PathRoot.transform.localPosition.x + i * girdWidth;
                float y = PathRoot.transform.localPosition.z + j * girdHeight;
                var Obj = Instantiate(prefab, new Vector3(x, PathRoot.transform.localPosition.y, y), Quaternion.identity);
                Obj.transform.parent = PathRoot.transform;
                map[i, j] = new PathPoint(i, j, Obj.transform);
                mPathMaterial[i, j] = Obj.GetComponent<MeshRenderer>();
            }
        }

        for (int i = 10; i < 46; i++)
        {

            SetObjstacle(i, 31);

        }

        for (int i = 20; i < 50; i++)
        {

            SetObjstacle(i, 20);

        }

        SetPathStartPos((int)startTrans.localPosition.x, (int)startTrans.localPosition.z);
        SetPathEndPos((int)targetTrans.localPosition.x, (int)targetTrans.localPosition.z);
        pathList = FindPath();
        if (pathList == null)
        {
            return;
        }

        ShowPath(pathList);
    }

    public void SetPathStartPos(int x, int y)
    {
        startPoint = map[x, y];

        mPathMaterial[x, y].material.color = Color.green;
    }

    public void SetPathEndPos(int x, int y)
    {
        endPoint = map[x, y];
        mPathMaterial[x, y].material.color = Color.red;

    }


    public List<PathPoint> FindPath()
    {
        List<PathPoint> openList = new List<PathPoint>();
        List<PathPoint> closeList = new List<PathPoint>();



        openList.Add(startPoint);
        while (openList.Count > 0)
        {
            //查找openlist最小节点
            PathPoint curPoint = FindOpenList_Min_F(openList);
            openList.Remove(curPoint);
            closeList.Add(curPoint);


            //查找周围路径点
            List<PathPoint> aroundList = FindAround_Point(curPoint);
            foreach (PathPoint p in aroundList)
            {
                //先判断closeList 是否包含已经计算过的点:有就忽略
                if (closeList.Contains(p) || p.isObstacle)
                {
                    continue;
                }

                if (!openList.Contains(p))
                {
                    p.parent = curPoint;
                    //计算F,G,H
                    p.H = Mathf.Abs(endPoint.X - p.X) + Mathf.Abs(endPoint.Y - p.Y);
                    if (p.parent != null)
                    {
                        p.G = 1 + p.parent.G;
                    }
                    p.F = p.G + p.H;

                    openList.Add(p);
                }
                else
                {
                    int newG = 1 + curPoint.G;
                    if (newG < p.G)
                        p.SetParent(curPoint, newG);
                }

            }


            if (openList.Contains(endPoint))
            {
                last_endPoint = endPoint;
                return GeneratePath();

            }
        }

        return null;
    }

    //查找 openlist 代价最小的F 既当前节点
    private PathPoint FindOpenList_Min_F(List<PathPoint> list)
    {
        PathPoint node = list[0];
        foreach (PathPoint item in list)
        {
            if (item.F < node.F)
            {
                node = item;
            }
        }
        return node;

    }

    //查找周围相邻节点列表
    private List<PathPoint> FindAround_Point(PathPoint node)
    {
        List<PathPoint> list = new List<PathPoint>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                int x = node.X + i;
                int y = node.Y + j;

                if (node.isObstacle)
                    continue;

                if (x >= 0 && x < width - 1 && y >= 0 && y < height - 1)
                    list.Add(map[x, y]);
            }
        }
        return list;
    }

    public List<PathPoint> GeneratePath()
    {
        List<PathPoint> pathList = new List<PathPoint>();

        PathPoint temp = endPoint;
        if (temp != null)
        {
            while (temp != startPoint)
            {
                pathList.Add(temp);
                temp = temp.parent;

            }
        }
        pathList.Reverse();
        return pathList;
    }


    public List<Renderer> clearPathGridList = new List<Renderer>();
    public float speed = 3;

    //显示路径
    public void ShowPath(List<PathPoint> pathList)
    {
        for (int i = 0; i < clearPathGridList.Count; i++)
        {
            clearPathGridList[i].material.color = Color.white;
        }
        clearPathGridList.Clear();

        for (int i = 0; i < pathList.Count; i++)
        {

            mPathMaterial[pathList[i].X, pathList[i].Y].material.color = Color.blue;
            clearPathGridList.Add(mPathMaterial[pathList[i].X, pathList[i].Y]);

            //startTrans.localPosition = new Vector3(pathList[i].Y, startTrans.localPosition.y, pathList[i].X);
        }
    }

    public Transform Player;

    IEnumerator IE_Fn(List<PathPoint> pathList)
    {
        if (pathList != null)
        {
            Debug.Log("移动中");
            int cnt = pathList.Count - 1;
            int index = 0;

            //while (startPoint.m_Self.localPosition != endPoint.m_Self.localPosition)
            while (index != cnt)
            {
                yield return new WaitForEndOfFrame();

                Vector3 nextPoint = new Vector3(pathList[index].X, Player.localPosition.y, pathList[index].Y);

                Vector3 dir = (nextPoint - Player.localPosition).normalized;

                //计算Player当前位置到下一个目标点的距离
                float offset = Vector3.Distance(Player.localPosition, nextPoint);

                if (offset < 0.5f)
                {
                    Player.localPosition = nextPoint;
                    index++;
                }
                else
                {
                    //计算一帧移动多少距离
                    Vector3 distance = dir * speed * Time.deltaTime;
                    Player.localPosition += distance;
                }

                if (index == cnt)
                {
                    Debug.LogError("终点坐标:" + pathList[index].X + "--" + pathList[index].Y);
                }


            }
        }
    }

    public void Update()
    {

        SetPathEndPos((int)targetTrans.localPosition.x, (int)targetTrans.localPosition.z);
        if (last_endPoint != null && last_endPoint == endPoint & endPoint != null)
        {
            return;
        }

        pathList = FindPath();
        if (pathList == null)
        {
            return;
        }
        ShowPath(pathList);



    }


    #region 设置障碍物
    public void SetObjstacle(int x, int y)
    {
        map[x, y].isObstacle = true;
        mPathMaterial[x, y].material.color = Color.cyan;
    }

    #endregion

    //开始寻路
    public void OnBtnStartMove()
    {
        StartCoroutine(IE_Fn(pathList));
    }

}
