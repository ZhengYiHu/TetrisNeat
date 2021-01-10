using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class Genome
{
    private static int globalInnovationNumber = 1; //keep track of innovation number between the genomes
    private int genomeDeph = 1;
    private float fitness;

    //constants for genome distance calculation
    private float excessC = 2;
    private float disjointC = 2;
    private float averageC = 0.4f;

    //Mutation chances
    public float chanceMutationWeight = 0.8f;
    public float chanceMutationAddNode = 0.03f;
    public float chanceMutationAddConnection = 0.3f;
    public float chanceMutateEnable = 0.1f;

    //chances for weight
    private float chanceUniformPerturbation = 0.9f;
    private float chanceNoInheritance = 0.75f;

    //activation threshold for node to activate
    private float activationThreshold = 0.8f;

    //memory
    [SerializeField]
    private List<ConnectionGene> serializableConnections = new List<ConnectionGene>();
    private Dictionary<int, ConnectionGene> connectionGenes = new Dictionary<int, ConnectionGene>();
    [SerializeField]
    private List<NodeGene> nodeGenes = new List<NodeGene>();
    


    public Genome()
    {
       
    }

    public Genome(Dictionary<int, ConnectionGene> connectionGenes, List<NodeGene> nodeGenes)
    {
        this.connectionGenes = connectionGenes;
        this.nodeGenes = nodeGenes;
        globalInnovationNumber = Mathf.Max(MaxInnovationNumber(connectionGenes)+1,globalInnovationNumber);
    }

    //Create a copy of Genome. lists and objects in lists are copies. connectionsList reference to objects in nodesList.
    public Genome Clone()
    {
        Dictionary<int,ConnectionGene> newConnectionGenes = new Dictionary<int, ConnectionGene>();
        List<NodeGene> newNodes = new List<NodeGene>();

        foreach (NodeGene node in nodeGenes)
        {
            NodeGene clonedNode = node.Clone();
            newNodes.Add(clonedNode);
        }

        foreach(ConnectionGene connection in connectionGenes.Values)
        {
            int inId = connection.GetInNode().GetId();
            int outId = connection.GetOutNode().GetId();
            float weight = connection.GetWeight();
            int innovationNumber = connection.GetInnovationNumber();
            bool enabled = connection.GetEnabled();

            NodeGene inNode = newNodes.Find(node => node.GetId() == inId);
            NodeGene outNode = newNodes.Find(node => node.GetId() == outId);

            ConnectionGene newConnection = new ConnectionGene(inNode, outNode, weight, innovationNumber, enabled);
            newConnectionGenes.Add(newConnection.GetInnovationNumber(),newConnection);
        }
        return new Genome(newConnectionGenes, newNodes);
    }


    //Add a connection between nodes
    public void MutateAddConnection(NodeGene inNode, NodeGene outNode) 
    {
        float weight = Random.Range(-1f, 1f);
        ConnectionGene newConnection = new ConnectionGene(inNode,outNode,weight,globalInnovationNumber++);
        connectionGenes.Add(newConnection.GetInnovationNumber(),newConnection);
    }

    //Add node
    public void MutateAddNode(ConnectionGene oldConnection)
    {
        int maxId = nodeGenes.Max((NodeGene node) => node.GetId());

        NodeGene newNode = new NodeGene(++maxId,oldConnection.GetInNode().GetLayer() + 1);
        //Move all the nodes on the greater layer too
        List<NodeGene> greaterLayerNodes = nodeGenes.FindAll((NodeGene node) => node.GetLayer() >= oldConnection.GetOutNode().GetLayer());
        for (int i = 0; i < greaterLayerNodes.Count; i++)
        {
            greaterLayerNodes[i].SetLayer(greaterLayerNodes[i].GetLayer() + 1);
        }

        //Create the connections
        ConnectionGene newConnectionGene1 = new ConnectionGene(oldConnection.GetInNode(),newNode,1,globalInnovationNumber++);
        ConnectionGene newConnectionGene2 = new ConnectionGene(newNode, oldConnection.GetOutNode(),oldConnection.GetWeight(), globalInnovationNumber++);
        //disable old connection and substitute with new connections
        oldConnection.SetEnabled(false); 
        connectionGenes.Add(newConnectionGene1.GetInnovationNumber(),newConnectionGene1);
        connectionGenes.Add(newConnectionGene2.GetInnovationNumber(), newConnectionGene2);

        nodeGenes.Add(newNode);
    }

    //Change weight
    public void MutateWeight(ConnectionGene connection)
    {
        
        int randomNum = Random.Range(0, 100);
        if(randomNum < chanceUniformPerturbation)
        {
            float oldWeight = connection.GetWeight();
            connection.SetWeight(oldWeight * Random.Range(-2f, 2f));
            if (connection.GetWeight() > 1) connection.SetWeight(1);
            if (connection.GetWeight() < -1) connection.SetWeight(-1);
        }
        else{
            connection.SetWeight(Random.Range(-1f, 1f));
        }
    }

    //Change enabled
    public void MutateEnabled(ConnectionGene connection)
    {
        connection.SetEnabled(!connection.GetEnabled());
    }

    public void MutateGenome()
    {
        //mutate connections only if there are connections
        if (connectionGenes.Count != 0)
        {
            for (int j = 0; j < connectionGenes.Count; j++)
            {
                //mutate connection weights
                if (Random.Range(0f, 1f) < chanceMutationWeight)
                {
                    MutateWeight(connectionGenes.ElementAt(j).Value);
                }

                //mutate connection enable/disable
                if (Random.Range(0f, 1f) < chanceMutateEnable)
                {
                    MutateEnabled(connectionGenes.ElementAt(j).Value);
                }
            }

            //mutate genome by adding a node
            if (Random.Range(0f, 1f) < chanceMutationAddNode)
            {
                if (connectionGenes.Count != 0)
                {
                    ConnectionGene connectionToMutate = connectionGenes.ElementAt(Random.Range(0, connectionGenes.Count)).Value;
                    MutateAddNode(connectionToMutate);
                }
            }
        }

        //mutate genome by adding a connection
        if (Random.Range(0f, 1f) < chanceMutationAddConnection)
        {

            NodeGene node1 = nodeGenes[Random.Range(0, nodeGenes.Count)];
            NodeGene node2 = nodeGenes[Random.Range(0, nodeGenes.Count)];

            while (node1.GetLayer() >= node2.GetLayer())
            {
                node1 = nodeGenes[Random.Range(0, nodeGenes.Count)];
                node2 = nodeGenes[Random.Range(0, nodeGenes.Count)];
            }
            MutateAddConnection(node1, node2);
        }
    }
    //Generate offspring between two parents. This Genome will be the most fit, so disjoints and excesses from the other parent are discarded
    public Genome Crossover(Genome parent2)
    {
        Genome clone1 = Clone();
        Genome clone2 = parent2.Clone();
        Genome offspring = new Genome();
        //crossover with no inheritance
        if (Random.Range(0,1) < chanceNoInheritance)
        {
            if (Random.Range(0, 2) == 0) // 50% for each parent
            {
                offspring = clone1;
            }
            else
            {
                offspring = clone2;
            }      
        }
        //crossover between two parents
        else
        {
            List<NodeGene> newNodeGenes = new List<NodeGene>();
            Dictionary<int, ConnectionGene> newConnectionGenes = new Dictionary<int, ConnectionGene>();
            foreach (int innovationNumber in connectionGenes.Keys)
            {
                //if the other parent's connectionsList contains a connection with same innovation number = The genes match.
                if (parent2.connectionGenes.ContainsKey(innovationNumber))
                {
                    if (Random.Range(0, 2) == 0) // 50% for each parent
                    {
                        ConnectionGene clonedConnection = clone1.connectionGenes[innovationNumber];
                        newConnectionGenes.Add(innovationNumber, clonedConnection);
                        if (!newNodeGenes.Exists(node => node.GetId() == clonedConnection.GetInNode().GetId())) newNodeGenes.Add(clonedConnection.GetInNode());
                        if (!newNodeGenes.Exists(node => node.GetId() == clonedConnection.GetOutNode().GetId())) newNodeGenes.Add(clonedConnection.GetInNode());
                    }
                    else
                    {
                        ConnectionGene clonedConnection = clone2.connectionGenes[innovationNumber];
                        newConnectionGenes.Add(innovationNumber, clonedConnection);
                        if (!newNodeGenes.Exists(node => node.GetId() == clonedConnection.GetInNode().GetId())) newNodeGenes.Add(clonedConnection.GetInNode());
                        if (!newNodeGenes.Exists(node => node.GetId() == clonedConnection.GetOutNode().GetId())) newNodeGenes.Add(clonedConnection.GetOutNode());
                    }

                }
                else //disjoint and excess genes are inherited from this genome (most fit)
                {
                    ConnectionGene clonedConnection = clone1.connectionGenes[innovationNumber];
                    newConnectionGenes.Add(innovationNumber, clonedConnection);
                    if (!newNodeGenes.Exists(node => node.GetId() == clonedConnection.GetInNode().GetId())) newNodeGenes.Add(clonedConnection.GetInNode());
                    if (!newNodeGenes.Exists(node => node.GetId() == clonedConnection.GetInNode().GetId())) newNodeGenes.Add(clonedConnection.GetOutNode());
                }
            }
            offspring = new Genome(newConnectionGenes, newNodeGenes);
        }
        offspring.MutateGenome();
        return offspring;
    }


    //Calculate the distance between two genomes for speciation division
    public float GetDistance(Genome otherGenome)
    {

        DistanceInfo distanceInfo = GetDistanceInfo(this, otherGenome);
        distanceInfo.numberOfGenes = distanceInfo.numberOfGenes == 0 ? 1 : distanceInfo.numberOfGenes;
        float distance = (excessC * distanceInfo.excess / distanceInfo.numberOfGenes)+ (disjointC * distanceInfo.disjoint / distanceInfo.numberOfGenes )+ (averageC * distanceInfo.averageMatching);
        //Debug.Log("DISJOINT: "+ (disjointC * distanceInfo.disjoint / distanceInfo.numberOfGenes) + " EXCESS: " + (excessC * distanceInfo.excess / distanceInfo.numberOfGenes) + " AM: " + (averageC * distanceInfo.averageMatching) + " NUMBER: " + distanceInfo.numberOfGenes + " DISTANCE: "+distance );
        return distance;
    }

    //Get the Highest innovation number among the connections genes
    public int MaxInnovationNumber(Dictionary<int, ConnectionGene> connectionGenes)
    {
        int max = 0;
        foreach(int innovationNumber in connectionGenes.Keys)
        {
            if(max < innovationNumber)
            {
                max = innovationNumber;
            }
        }
        return max;
    }

    //Calculate info to calculate distance: how many disjoint, excess nodes and the average matching between two genomes
    private DistanceInfo GetDistanceInfo(Genome genome1, Genome genome2)
    {
        int disjoint = 0;
        int excess = 0;
        float matchingCount = 0;
        float matchingDifference = 0;
        int numberOfGenes = Mathf.Max(genome1.GetConnectionGenes().Count, genome2.GetConnectionGenes().Count);

        int maxInnovation1 = MaxInnovationNumber(genome2.connectionGenes);
        int maxInnovation2 = MaxInnovationNumber(genome1.connectionGenes);
        //Check which parent is more advanced in innovation. 
        int max = Mathf.Max(maxInnovation1, maxInnovation2);

        for (int innovationNumber = 1; innovationNumber <= max; innovationNumber++)
        {
            
            bool inParent1;
            bool inParent2;
            inParent1 = genome1.connectionGenes.ContainsKey(innovationNumber);
            inParent2 = genome2.connectionGenes.ContainsKey(innovationNumber);
           
            
            if (inParent1 && inParent2)
            {
                ConnectionGene connection1 = genome1.connectionGenes[innovationNumber];
                ConnectionGene connection2 = genome2.connectionGenes[innovationNumber];
                matchingDifference += Mathf.Abs(connection1.GetWeight() - connection2.GetWeight());
                matchingCount++;
            }
            else if(inParent1 && !inParent2)
            {
                ConnectionGene connection1 = genome1.connectionGenes[innovationNumber];
                if(connection1.GetInnovationNumber() < maxInnovation2)
                {
                    excess++;
                }
                else
                {
                    disjoint++;
                }
            }
            else if(!inParent1 && inParent2)
            {
                ConnectionGene connection2 = genome2.connectionGenes[innovationNumber];
                if (connection2.GetInnovationNumber() < maxInnovation1)
                {
                    excess++;
                }
                else
                {
                    disjoint++;
                }
            }
        }
        float averageMatching = matchingCount == 0 ? 0 : matchingDifference / matchingCount;
        return new DistanceInfo(disjoint, excess, averageMatching , numberOfGenes);
    }

   

    public void FeedForward(float[] givenInputs)
    {
        
        List<ConnectionGene> connectionsList = connectionGenes.Values.OrderBy(connection => connection.GetOutNode().GetLayer()).ToList();
        List<NodeGene> inNodes = nodeGenes.FindAll((NodeGene node) => node.GetLayer() == 0 && !node.IsBias());
        for (int i = 0; i < inNodes.Count; i++)
        {
            inNodes[i].SetOutput(givenInputs[i]);
        }

        for (int i = 0; i < connectionsList.Count; i++)
        {
            if (connectionsList[i].GetEnabled())
            {
                NodeGene inNode = connectionsList[i].GetInNode();
                NodeGene outNode = connectionsList[i].GetOutNode();
                //Add the value of input node to the sum of the output node
                outNode.SetInputSum(outNode.GetInputSum() + (inNode.GetOutput() * connectionsList[i].GetWeight()));
                //Calculate Sigmoid of the sum of all inputs
                outNode.SetOutput(Sigmoid(outNode.GetInputSum()));
            }
        }
        //reset inputsums
        nodeGenes.ForEach((NodeGene node) => node.SetInputSum(0));
       
    }

    //Data type for returning innovation number info
    public struct DistanceInfo
    {
        public int disjoint;
        public int excess;
        public float averageMatching;
        public int numberOfGenes;

        public DistanceInfo(int d, int e, float a, int n)
        {
            disjoint = d;
            excess = e;
            averageMatching = a;
            numberOfGenes = n;
        }
    }
    /*
    ------------------------------------------------------------------------------
    Getters and setters
    */

    //Get the Id of the winner output node
    public List<int> GetActiveCommandsId()
    {
        List<int> commandsToRun = new List<int>();
        //find depht
        genomeDeph = nodeGenes.Max((NodeGene node) => node.GetLayer());
        //find ouput nodes
        List<NodeGene> outputs = nodeGenes.FindAll((NodeGene node) => node.GetLayer() == genomeDeph).OrderByDescending((NodeGene node) => node.GetOutput()).ToList();
        for (int i = 0; i < outputs.Count; i++)
        {
            if(outputs[i].GetOutput() > activationThreshold)
            {
                commandsToRun.Add(outputs[i].GetId());
            }
        }
        return commandsToRun;
    }

    //Get Sigmoid function
    public static float Sigmoid(float value)
    {
        return 2/(1 + Mathf.Exp(-4.9f * value)) - 1;
    }

    //Get Fitness
        public float GetFitness()
    {
        return fitness;
    }


    //Set Fitness
    public void ChangeFitness(float newFitness)
    {
        fitness = newFitness;
    }

    //Get Nodes
    public List<NodeGene> GetNodeGenes()
    {
        return nodeGenes;
    }

    //Get Connections
    public Dictionary<int,ConnectionGene> GetConnectionGenes()
    {
        return connectionGenes;
    }

    //Set connection genes for serialization
    public void SerializaConnectionGenes()
    {
        serializableConnections = connectionGenes.Values.ToList();
    }

    //Get Serializable Connections
    public List<ConnectionGene> GetSerializableConnections()
    {
        return serializableConnections;
    }

    //Get Innovation number and increase it
    public static int IncrementInnovationNumber()
    {
        int iNum = globalInnovationNumber;
        globalInnovationNumber++;
        return iNum;
    }

}

