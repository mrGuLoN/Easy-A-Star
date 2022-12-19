using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class MyPathFinder : MonoBehaviour
{
    public List<Vector2> PathToTarget;
    [SerializeField] private GameObject forpost, enemyForpost;

    List<NodeNew> CheckedNodes = new List<NodeNew>();
    List<NodeNew> WaitingNodes = new List<NodeNew>();
    private int[,] _cost;
    public GameObject Target;    
    public int costToMove;
    public GameObject[,] stage;
    public GameObject[] fireZone;
    private GameObject positionS;
    private NavigationMatrixCreate nmc;
       
    // Start is called before the first frame update
    void Start()
    {
        nmc = GetComponent<NavigationMatrixCreate>();
        nmc.GiveNaviPoint(out stage, out fireZone);
        int z = stage.GetLength(0);
        int y = stage.GetLength(1);
        _cost = new int[z, y];
        for (int i = 0; i < nmc.h ; i++)
        {
            for (int j =0; j < nmc.l; j++)
            {               
                _cost[i, j] = stage[i, j].GetComponent<CostGround>().cost;              
            }
        }
    }

    public void UpdateNaviTargetFireZone(GameObject player, bool combat, bool enemy)
    {
        if (combat == true)
        {
            Target = ClothestStageFromThisObject(player);
        }
        else if (enemy == true)
        {
            Target = ClothestStageFromThisObject(forpost);           
        }
        else if (enemy == false)
        {
            Target = ClothestStageFromThisObject(enemyForpost);           
        }
    }

    // Update is called once per frame
    public void FindWay(out List<Vector2> pathToTarget, GameObject mob)
    {        
        pathToTarget = new List<Vector2>();      
        pathToTarget = GetPath(ClothestStageFromThisObject(Target).transform.position, mob);
        Debug.Log(pathToTarget);
    }

    private GameObject ClothestStageFromThisObject(GameObject check)  //Ищем ближайшую ячейку стадии
    {
        float distance = Mathf.Infinity;       
        Vector2 pos = new Vector2(check.transform.position.x, check.transform.position.z);        
        foreach (GameObject go in stage)
        {           
            Vector2 diff = new Vector2(go.transform.position.x, go.transform.position.z) - pos;            
            float cureDistance = diff.sqrMagnitude;
            if (cureDistance < distance && go.GetComponent<CostGround>().cost > 0)
            {              
                positionS = go;
                distance = cureDistance;
            }
        }       
        return positionS;
    }

    /*private GameObject ClothestFireZoneFromThisObject(GameObject check)  //Ищем ближайшую ячейку стадии
    {       
        float distance = Mathf.Infinity;
        Vector2 pos = new Vector2(check.transform.position.x, check.transform.position.z);
        foreach (GameObject go in fireZone)
        {
            Vector2 diff = new Vector2(go.transform.position.x, go.transform.position.z) - pos;
            float cureDistance = diff.sqrMagnitude;
            if (cureDistance < distance)
            {
                positionS = go;
                distance = cureDistance;
            }
        }
        Debug.Log(positionS);
        return positionS;
    }*/


    public List<Vector2> GetPath(Vector2 target, GameObject startPoint) //Создаем лист с координатами движения
    {
        
        PathToTarget = new List<Vector2>();
        CheckedNodes = new List<NodeNew>();
        WaitingNodes = new List<NodeNew>();
       
        GameObject StartPosition = ClothestStageFromThisObject(startPoint);
        GameObject TargetPosition = ClothestStageFromThisObject(Target);
      
        if (StartPosition == TargetPosition)
        {           
            return PathToTarget;
        }
       
        NodeNew startNode = new NodeNew(0, StartPosition, TargetPosition, null, 0);
        CheckedNodes.Add(startNode);
        WaitingNodes.AddRange(GetNeighbourNodes(startNode));       
        while (WaitingNodes.Count > 0)
        {           
            NodeNew nodeToCheck = WaitingNodes.Where(x => x.F == WaitingNodes.Min(z => z.F)).FirstOrDefault();
            if (nodeToCheck.Position == TargetPosition)
            {               
                return CalculatePathFromNode(nodeToCheck);
            }
           
            if (nodeToCheck.Position.GetComponent<CostGround>().cost <= 0)
            {                
                WaitingNodes.Remove(nodeToCheck);
                CheckedNodes.Add(nodeToCheck);
            }
            else if (nodeToCheck.Position.GetComponent<CostGround>().cost >0)
            {                
                WaitingNodes.Remove(nodeToCheck);
                if (!CheckedNodes.Where(x => x.Position == nodeToCheck.Position).Any())
                {                    
                    CheckedNodes.Add(nodeToCheck);
                    WaitingNodes.AddRange(GetNeighbourNodes(nodeToCheck));
                }
            }            
        }
        return PathToTarget;
    }

    public List<Vector2> CalculatePathFromNode(NodeNew node) //когда вышли на финальную точку проверяем стоимости пути
    {       
        var path = new List<Vector2>();
        NodeNew currentNode = node;        
        while (currentNode.PreviouseNode != null)
        {           
            path.Add(new Vector2(currentNode.Position.transform.position.x, currentNode.Position.transform.position.z));
            currentNode = currentNode.PreviouseNode;
            costToMove += currentNode.Position.GetComponent<CostGround>().cost;
        }

        return path;
    }

    List<NodeNew> GetNeighbourNodes(NodeNew node) //добавляем соседние точки в лист проверки
    {        
        var Neighbours = new List<NodeNew>();       
        FindIndex(node.Position, out int x, out int z);
        if (x > 0 && _cost[x - 1, z] > 0)
        Neighbours.Add(new NodeNew(node.G + 1, stage[x-1, z], node.TargetPosition, node, _cost[x - 1, z]));
        if (x < nmc.h-1 && _cost[x + 1, z] > 0)
        Neighbours.Add(new NodeNew(node.G + 1, stage[x+1, z], node.TargetPosition, node, _cost[x + 1, z]));
        if (z > 0 && _cost[x, z - 1] > 0)
        Neighbours.Add(new NodeNew(node.G + 1, stage[x, z-1], node.TargetPosition, node, _cost[x, z-1]));
        if (z < nmc.l-1 && _cost[x, z + 1] > 0)
        Neighbours.Add(new NodeNew(node.G + 1, stage[x, z+1], node.TargetPosition, node, _cost[x, z+1]));
        return Neighbours;
    }

    private void FindIndex(GameObject node, out int xz, out int yz)
    {
                xz = 0;
        yz = 0;        
        for (int i =0; i < nmc.h; i++)
        {
            for (int j = 0; j < nmc.l; j++)
            {
                if (node.transform.position.x == stage[i,j].transform.position.x && node.transform.position.z == stage[i, j].transform.position.z)
                {
                    xz = i;
                    yz = j;                    
                    return;                    
                }
            }
        }
    }   
   
}
public class NodeNew // "стоимость" пути из точки к цели
{
    public GameObject Position, TargetPosition;
    public NodeNew PreviouseNode;
    public int F, G, H; // сумма расстояний, расстояние от старта до Ноды, расстояние от Ноды до цели

    public NodeNew(int g, GameObject nodePosition, GameObject targetPosition, NodeNew previouseNode, int cost)
    {        
        Position = nodePosition;
        TargetPosition = targetPosition;
        PreviouseNode = previouseNode;
       
        G = g;
        H = (int)Mathf.Abs(targetPosition.transform.position.x - Position.transform.position.x) + (int)Mathf.Abs(targetPosition.transform.position.z - Position.transform.position.z);
        F = G + H + cost;        
    }

}

