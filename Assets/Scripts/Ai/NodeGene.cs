using UnityEngine;

[System.Serializable]
public class NodeGene
{
    [SerializeField]
    private int id;
    [SerializeField]
    private float outputValue;
    [SerializeField]
    private int layer;
    [SerializeField]
    private float inputSum;
    [SerializeField]
    private bool bias = false;

    public NodeGene(int id,int layer)
    {

        this.id = id;
        this.layer = layer;
    }

   
    //Create a copy of Node
    public NodeGene Clone()
    {
        NodeGene newNode = new NodeGene(id,layer);
        newNode.outputValue = outputValue;
        newNode.bias = bias;
        return newNode;
    }

    /*
    ------------------------------------------------------------------------------
    Getters and setters
    */

    public int GetId()
    {
        return id;
    }

    public float GetOutput()
    {
        return outputValue;
    }

    public int GetLayer()
    {
        return layer;
    }

    public float GetInputSum()
    {
        return inputSum;
    }

    public void SetOutput(float newOutput)
    {
        outputValue = newOutput;
    }

    public void SetLayer(int newLayer)
    {
        layer = newLayer;
    }

    public void SetInputSum(float newInputSum)
    {
        inputSum = newInputSum;
    }

    public bool IsBias()
    {
        return bias;
    }

    public void setBias(bool newBias)
    {
        bias = newBias;
    }
}
