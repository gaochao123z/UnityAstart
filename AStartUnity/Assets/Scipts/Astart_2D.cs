using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public int X;
    public int Y;
    public int F, G, H;
    public bool isObstacle = false;
    public PathNode parent = null;


    public PathNode(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public void SetParent(PathNode p, int g)
    {
        this.parent = p;
        this.G = g;
        F = G + H;
    }
}


public class Astart_2D : MonoBehaviour
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

    public const int m_Width = 55;
    public const int m_Height = 55;

    PathNode m_StartNode = null;
    PathNode m_EndNode = null;

    PathNode[,] map = new PathNode[m_Height, m_Width];
    SpriteRenderer[,] sprites = new SpriteRenderer[m_Height, m_Width];
    public GameObject mPrefab;

    public Vector2 end = new Vector2(7, 9);
    public void Start()
    {

        InitMap();
        //AddObstacle(2, 4);
        //AddObstacle(2, 3);
        //AddObstacle(2, 2);
        //AddObstacle(2, 0);
        //AddObstacle(6, 4);
        //AddObstacle(8, 4);

        ////for (int i = 1; i < 6; i++)
        ////{
        ////    AddObstacle(6, i);
        ////}



        SetStartPosAndEndPos(23, 3, (int)end.x, (int)end.y);
        FindPath();

        ShowPath(Color.gray);
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            ShowPath(Color.white);
            SetTargetPos(Random.Range(1, m_Height - 2), Random.Range(1, m_Height - 2));

            FindPath();

            ShowPath(Color.gray);
        }
    }

    public void ResetPath()
    {

    }

    public void InitMap()
    {
        for (int i = 0; i < m_Width; i++)
        {
            for (int j = 0; j < m_Height; j++)
            {
                sprites[i, j] = Instantiate(mPrefab, new Vector3(i, j, 0), Quaternion.identity).GetComponent<SpriteRenderer>();
                map[i, j] = new PathNode(i, j);
            }
        }

        Debug.Log("1");
    }

    //设置障碍物
    public void AddObstacle(int x, int y)
    {
        map[x, y].isObstacle = true;
        sprites[x, y].color = Color.black;
    }

    //设置开始点与结束点位置
    public void SetStartPosAndEndPos(int startX, int startY, int endX, int endY)
    {
        m_StartNode = map[startX, startY];
        sprites[startX, startY].color = Color.green;


        m_EndNode = map[endX, endY];
        sprites[endX, endY].color = Color.red;
    }

    public void SetTargetPos(int endX, int endY)
    {
        m_EndNode = map[endX, endY];
        sprites[endX, endY].color = Color.red;
    }

    public void FindPath()
    {
        List<PathNode> m_OpenList = new List<PathNode>();
        List<PathNode> m_CloseList = new List<PathNode>();
        m_OpenList.Add(m_StartNode);
        while (m_OpenList.Count > 0)
        {
            PathNode curNode = FindOpenList_Min_F(m_OpenList);

            m_OpenList.Remove(curNode);
            m_CloseList.Add(curNode);

            //计算周围8个相邻放格
            //List<PathNode> zhouWeiList = Calc_ZhouWeiPoint(curNode.X, curNode.Y);
            List<PathNode> zhouWeiList = getNeibourhood(curNode);

            foreach (PathNode item in m_CloseList)
            {
                if (zhouWeiList.Contains(item))
                {
                    zhouWeiList.Remove(item);
                }
            }

            foreach (PathNode item in zhouWeiList)
            {
                if (item.isObstacle)
                {
                    continue;
                }
                if (m_OpenList.Contains(item))
                {
                    int newG = 1 + curNode.G;
                    if (newG < item.G)
                    {
                        item.SetParent(curNode, newG);
                    }
                }
                else
                {
                    item.parent = curNode;
                    GetF(item);
                    m_OpenList.Add(item);
                }
            }

            if (m_OpenList.Contains(m_EndNode))
            {
                break;
            }
        }
    }

    //找到最小 F
    private PathNode FindOpenList_Min_F(List<PathNode> openList)
    {
        int min = int.MaxValue;
        PathNode point = null;
        foreach (PathNode p in openList)
        {
            if (p.F < min)
            {
                min = p.F;
                point = p;
            }
        }

        return point;
    }

    public List<PathNode> Calc_ZhouWeiPoint(int x, int y)
    {
        List<PathNode> pointList = new List<PathNode>();
        if (x > 0 && !map[x - 1, y].isObstacle)
        {
            pointList.Add(map[x - 1, y]);
        }

        if (y > 0 && !map[x, y - 1].isObstacle)
        {
            pointList.Add(map[x, y - 1]);
        }

        if (x < m_Height - 1 && !map[x + 1, y].isObstacle)
        {
            pointList.Add(map[x + 1, y]);
        }
        if (y < m_Width - 1 && !map[x, y + 1].isObstacle)
        {
            pointList.Add(map[x, y + 1]);
        }
        return pointList;

    }

    public List<PathNode> getNeibourhood(PathNode node)
    {
        List<PathNode> list = new List<PathNode>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // 如果是自己，则跳过
                if (i == 0 && j == 0 && node.isObstacle)
                    continue;
                int x = node.X + i;
                int y = node.Y + j;
                // 判断是否越界，如果没有，加到列表中
                if (x < m_Height - 1 && x >= 0 && y < m_Width - 1 && y >= 0)
                    list.Add(map[x, y]);
            }
        }
        return list;
    }

    /// <summary>
    /// //计算某个点的F值
    /// </summary>
    public void GetF(PathNode point)
    {
        int G = 0;
        int H = Mathf.Abs(m_EndNode.X - point.X) + Mathf.Abs(m_EndNode.Y - point.Y);
        if (point.parent != null)
        {
            G = 1 + point.parent.G;
        }

        int F = H + G;
        point.H = H;
        point.G = G;
        point.F = F;
    }

    public void ShowPath(Color col)
    {
        PathNode temp = m_EndNode.parent;
        if (temp == null)
            return;

        while (temp != m_StartNode)
        {
            if (temp != null)
            {
                sprites[temp.X, temp.Y].color = col;
                temp = temp.parent;
            }

        }
    }
}
