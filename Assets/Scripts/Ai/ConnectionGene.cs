using UnityEngine;

[System.Serializable]
public class ConnectionGene
{
    [SerializeField]
    private NodeGene inNode;
    [SerializeField]
    private NodeGene outNode;
    [SerializeField]
    private float weight;
    [SerializeField]
    private bool enabled;
    [SerializeField]
    private int innovationNumber;

    public ConnectionGene(NodeGene inNode, NodeGene outNode,float weight,int innovationNumber, bool enabled = true)
    {
        this.inNode = inNode;
        this.outNode = outNode;
        this.weight = weight;
        this.enabled = enabled;
        this.innovationNumber = innovationNumber;
    }

    //Create a copy of connection gene with copies of nodes
    public ConnectionGene Clone()
    {
        return new ConnectionGene(inNode.Clone(), outNode.Clone(), weight,innovationNumber, enabled);
    }

    /*
    ------------------------------------------------------------------------------
    Getters and setters
    */

    public NodeGene GetInNode()
    {
        return inNode;
    }

    public NodeGene GetOutNode()
    {
        return outNode;
    }

    public float GetWeight()
    {
        return weight;
    }

    public bool GetEnabled()
    {
        return enabled;
    }

    public int GetInnovationNumber()
    {
        return innovationNumber;
    }

    public void SetWeight(float newWeight)
    {
        weight = newWeight;
    }

    public void SetEnabled(bool newEnabled)
    {
        enabled = newEnabled;
    }

    public void SetInNode(NodeGene newInNode)
    {
        inNode = newInNode;
    }

    public void SetOutNode(NodeGene newOutNode)
    {
        outNode = newOutNode;
    }
}
