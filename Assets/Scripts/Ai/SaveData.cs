using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveData : MonoBehaviour
{
    [SerializeField] private SimulationData simulationData = new SimulationData();
    

    public void SaveSimulationData()
    {
        Simulation simulation = GameObject.Find("Simulation").GetComponent<Simulation>();

        List <Genome> currentGenomes = simulation.GetGenomes();
        simulationData.progressScores = simulation.GetProgressScores();
        simulationData.averageScores = simulation.GetAverageScores();
        simulationData.species = simulation.GetSpecies();
        simulationData.bestGenome = simulation.GetBestGenome();
        simulationData.generation = simulation.GetGeneration();

        for (int i = 0; i < currentGenomes.Count; i++)
        {
            currentGenomes[i].SerializaConnectionGenes();
        }

        simulationData.genomes = currentGenomes;
    }

   
    public void SaveIntoJson()
    {
        SaveSimulationData();

        string storedData = JsonUtility.ToJson(simulationData);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/GameData.json", storedData);

    }

    public void LoadFromJson()
    {
        string jsonString = System.IO.File.ReadAllText(Application.persistentDataPath + "/GameData.json");
        SimulationData simulationData = JsonUtility.FromJson<SimulationData>(jsonString);
        List<Genome> loadedGenomes = CreateGenomes(simulationData);
        Genome bestGenome = BuildGenomeFromJson(simulationData.bestGenome);
        List<float> progressScores = simulationData.progressScores;
        List<float> averageScores = simulationData.averageScores;
        int generation = simulationData.generation;
        List<Specie> species = simulationData.species;
        GameObject.Find("Simulation").GetComponent<Simulation>().LoadFromJson(loadedGenomes, bestGenome, generation,progressScores,averageScores,species);
    }

    public List<Genome> CreateGenomes(SimulationData jsonData)
    {
        List<Genome> genomes = new List<Genome>();

        //retrieve json genomes
        List<Genome> jsonGenomes = jsonData.genomes;
        for (int i = 0; i < jsonGenomes.Count; i++)
        {
            genomes.Add(BuildGenomeFromJson(jsonGenomes[i]));
        }

        return genomes;
    }

    public Genome BuildGenomeFromJson(Genome jsonGenome)
    {
        List<NodeGene> nodes = jsonGenome.GetNodeGenes();

        //get the connections from json
        List<ConnectionGene> connections = jsonGenome.GetSerializableConnections();
        for (int i = 0; i < connections.Count; i++)
        {
            //find the right inNode from the nodes list
            NodeGene inNode = nodes.Find((NodeGene node) => node.GetId() == connections[i].GetInNode().GetId());
            connections[i].SetInNode(inNode);

            //find the right outNode from the nodes list
            NodeGene outNode = nodes.Find((NodeGene node) => node.GetId() == connections[i].GetOutNode().GetId());
            connections[i].SetOutNode(outNode);
        }
        //Create dictionary for connection nodes
        Dictionary<int, ConnectionGene> connectionsDictionary = new Dictionary<int, ConnectionGene>();
        for (int i = 0; i < connections.Count; i++)
        {
            connectionsDictionary.Add(connections[i].GetInnovationNumber(), connections[i]);
        }
        //Create the genome
        return new Genome(connectionsDictionary, nodes);
    }

    [System.Serializable]
    public class SimulationData
    {
        public List<float> progressScores;
        public List<float> averageScores;
        public List<Specie> species;
        public List<Genome> genomes;
        public Genome bestGenome;
        public int generation;
    }
}
