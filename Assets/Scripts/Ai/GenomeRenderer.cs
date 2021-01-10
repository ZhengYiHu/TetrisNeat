using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GenomeRenderer : MonoBehaviour
{
    //prefabs
    public GameObject input;
    public GameObject node;
    public GameObject connection;

    //Dictionaries to map each gene with the game object represented 
    private GameObject inputSprite;
    private Dictionary<NodeGene, GameObject> nodeDictionary = new Dictionary<NodeGene, GameObject>();
    private Dictionary<ConnectionGene, GameObject> connectionDictionary = new Dictionary<ConnectionGene, GameObject>();

    //nodesPositions memory
    private Dictionary<int, Vector3> nodePositionsY = new Dictionary<int, Vector3>();

    public GameObject DrawLine(Vector3 pointA, Vector3 pointB)
    {
        Transform parentTransform = GetComponentInParent<Transform>();
        GameObject lineGo = Instantiate(connection, transform);
        LineRenderer lineRenderer = lineGo.GetComponent<LineRenderer>();
        lineRenderer.widthMultiplier = 0.6f;
        lineRenderer.SetPosition(0, pointA - parentTransform.position);
        lineRenderer.SetPosition(1, pointB - parentTransform.position);

        return lineGo;
    }


   
    public void InitializeOutputNodesPosition()
    {
        nodePositionsY.Add(0, new Vector3(150, -20, 0));
        nodePositionsY.Add(1, new Vector3(150, 10, 0));
        nodePositionsY.Add(2, new Vector3(150, 40, 0));
        nodePositionsY.Add(3, new Vector3(150, 70, 0));
    }

    public void DrawNodes(List<NodeGene> nodeList)
    {
        inputSprite = Instantiate(input, transform);
        Transform parentTransform = GetComponentInParent<Transform>();
        NodeGene biasNode = nodeList.Find((NodeGene node) => node.IsBias());
        List<NodeGene> inputNodes = nodeList.FindAll((NodeGene node) => node.GetLayer() == 0 && !node.IsBias());
        List<NodeGene> otherNodes = nodeList.FindAll((NodeGene node) => node.GetLayer() != 0);

        //Draw bias Node
        GameObject biasNodeGo = Instantiate(node, new Vector3(-10, -10, 0) + transform.position, Quaternion.identity, transform);
        nodeDictionary.Add(biasNode, biasNodeGo);
        biasNodeGo.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = biasNode.GetId() + "";

        //Draw input Nodes
        int genomeDeph = nodeList.Max((NodeGene node) => node.GetLayer());
        for (int i = 0; i < inputNodes.Count; i++)
        {
            nodeDictionary.Add(inputNodes[i], inputSprite);
        }
        //Draw hidden and output nodes
        for (int i = 0; i < otherNodes.Count; i++)
        {
            //find layer of the node to determine x position
            int layer = otherNodes[i].GetLayer();

            Vector3 position;

            if (!nodePositionsY.ContainsKey(otherNodes[i].GetId()))
            {
                //if node doesn't exist yet, assign a new position with random y
                int yPosition = Random.Range(-7, 13) * 10;
                position = new Vector3(layer * 7, yPosition, 0);
                //if position is not unique, assign a new position
                while (nodePositionsY.ContainsValue(position))
                {
                    yPosition = Random.Range(-7, 13) * 10;
                    position = new Vector3(layer * 7, yPosition, 0);

                }
                //Add node position into memory
                nodePositionsY.Add(otherNodes[i].GetId(), position);
            }
            else
            {
                position = nodePositionsY[otherNodes[i].GetId()];
            }

            position += parentTransform.position;

            GameObject nodeGo = Instantiate(node, position, Quaternion.identity, transform);
            nodeDictionary.Add(otherNodes[i], nodeGo);
            nodeGo.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = otherNodes[i].GetId() + "";
        }
    }

    public void DrawConnections(Dictionary<int, ConnectionGene> connectionList)
    {
        //Get active connections
        List<ConnectionGene> activeConnections = connectionList.Values.Where(connection => connection.GetEnabled() == true).ToList();
        for (int i = 0; i < activeConnections.Count; i++)
        {
            //Get input node
            NodeGene inNode = activeConnections[i].GetInNode();

            if (inNode.GetLayer() == 0 && !inNode.IsBias())
            {
                nodeDictionary.Remove(inNode);
                nodeDictionary.Add(inNode,inputSprite);
            }

            GameObject inNodeGo = nodeDictionary[activeConnections[i].GetInNode()];
            GameObject outNodeGo = nodeDictionary[activeConnections[i].GetOutNode()];

            //Create line game object connecting the 2 nodes and insert it in the dictionary
            GameObject lineGo = DrawLine(inNodeGo.transform.position, outNodeGo.transform.position);
            connectionDictionary.Add(activeConnections[i], lineGo);
        }
    }

    public void RenderGenome(Genome genome)
    {
        Reset();
        List<NodeGene> nodes = genome.GetNodeGenes();
        Dictionary<int, ConnectionGene> connections = genome.GetConnectionGenes();
        DrawNodes(nodes);
        DrawConnections(connections);
    }

    public void Reset()
    {
        foreach (GameObject nodeGo in nodeDictionary.Values)
        {
            Destroy(nodeGo);
        }
        nodeDictionary = new Dictionary<NodeGene, GameObject>();

        foreach(GameObject lineGo in connectionDictionary.Values)
        {
            Destroy(lineGo);
        }
        connectionDictionary = new Dictionary<ConnectionGene, GameObject>();
    }
    
}
