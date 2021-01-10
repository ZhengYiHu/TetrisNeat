using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Specie
{
    [SerializeField]
    static int idCounter;
    [SerializeField]
    int id;
    [SerializeField]
    float bestFitness;
    //number of generations the specie hasn't improved at all
    [SerializeField]
    int staleGenerations;
    [SerializeField]
    List<Genome> genomes = new List<Genome>();
    [SerializeField]
    public Genome representative;

    public Specie(Genome genome)
    {
        genomes.Add(genome);
        representative = genome;
        staleGenerations = 0;
        id = idCounter;
        idCounter++;

    }

    public void AddGenome(Genome genome)
    {
        genomes.Add(genome);
        

        if (genome.GetFitness() > bestFitness)
        {
            bestFitness = genome.GetFitness();
            representative = genome;
            staleGenerations = 0;
        }
    }

    public Genome SpecieCrossover()
    {
        Genome offspring;

        Genome parent1 = genomes[Random.Range(0, genomes.Count)];
        Genome parent2 = genomes[Random.Range(0, genomes.Count)];

        if (parent1.GetFitness() > parent2.GetFitness())
        {
            offspring = parent1.Crossover(parent2);
        }
        else
        {
            offspring = parent1.Crossover(parent2);
        }

        return offspring;

    }

    public List<Genome> SpecieSelection()
    {
        List<Genome> toKill = genomes.OrderBy((Genome genome) => genome.GetFitness()).Take(Mathf.CeilToInt(genomes.Count()/2)).ToList();
        genomes = genomes.Except(toKill).ToList();
        return toKill;
    }

    public float GetSumRelativeFitness()
    {
        float sum = 0;
        for (int i = 0; i < genomes.Count; i++)
        {
            sum += genomes[i].GetFitness() / genomes.Count();
        }
        return sum;
    }

    public float GetAverageFitness()
    {
        float fitnessSum = 0;
        for (int i = 0; i < genomes.Count; i++)
        {
            fitnessSum += genomes[i].GetFitness();
        }
        return genomes.Count() == 0? 0 : fitnessSum / genomes.Count();
    }

    public List<Genome> GetGenomes()
    {
        return genomes;
    }

    public int GetId()
    {
        return id;
    }

    public void IncreaseStaleGenerations()
    {
        staleGenerations++;
    }

    public int GetStaleGenerations()
    {
        return staleGenerations;
    }

    public Genome GetRepresentative()
    {
        return representative;
    }
}
