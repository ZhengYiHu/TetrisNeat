using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Simulation : MonoBehaviour
{
    public int size = 200;
    public int lineSize = 20;

    public int width = 10;
    public int height = 20;

    
    public float clockTime = 0.05f;
    float previousTime;
    int generation;

    //Prefabs
    public GameObject gamePrefab;
    public GenomeRenderer genomeRendererPrefab;
    public GameObject displayGenerationUI;
    public GameObject renderSprite;
    public GameObject gameRenderer;


    public float distanceThreshold = 3f;
    
    GameObject[,] spritesMatrix;

    //Memory collections
    List<Specie> species = new List<Specie>();
    List<GameObject> games = new List<GameObject>();
    List<GameObject> brainlessGames = new List<GameObject>();
    Dictionary<GameObject, Genome> gamesDictionary = new Dictionary<GameObject, Genome>();

    List<float> progressScores = new List<float>();
    List<float> averageScores = new List<float>();

    //Renderer
    Genome bestGenome;
    GenomeRenderer genomeRenderer;
    GameObject bestGO;

    //Current stage
    EvolutionStage stage = EvolutionStage.Evolution;

    enum EvolutionStage
    {
        Evolution, Speciation, Selection, Crossover, Mutation, Reset
    }
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("________________________________________________________________________________");
        Debug.Log("Generation " + generation);
        InitializePrefabFields();

        List<GameObject> gamesList = GenerateGames();
        List<Genome> genomesList = GenerateBrains();
        spritesMatrix = new GameObject[width, height];
        InsertInDictionary(gamesList, genomesList);

        bestGenome = gamesDictionary[games[0]];
        genomeRenderer = Instantiate(genomeRendererPrefab);
        genomeRenderer.InitializeOutputNodesPosition();
        genomeRenderer.RenderGenome(bestGenome);
        CreatePrintSprites();
    }



    // Update is called once per frame
    void Update()
    {
        switch (stage)
        {
            case EvolutionStage.Evolution:
                Evolution();
                bestGO = gamesDictionary.FirstOrDefault((KeyValuePair<GameObject, Genome> pair) => pair.Value == bestGenome).Key;
                RenderBinaryMatrix(bestGO);

                if (!games.Any((GameObject arg) => !arg.GetComponent<Game>().GetGameOver()))
                {
                    SetFitnesses();
                    stage = EvolutionStage.Speciation;
                    generation++;
                }
                break;
            case EvolutionStage.Speciation:
                Speciation();
                Debug.Log("species " + species.Count);
                stage = EvolutionStage.Selection;
                break;
            case EvolutionStage.Selection:
                Selection();
                stage = EvolutionStage.Crossover;
                break;
            case EvolutionStage.Crossover:
                Crossover();
                stage = EvolutionStage.Reset;
                break;
            //case EvolutionStage.Mutation:
            //    Mutation();
            //    stage = EvolutionStage.Reset;
            //    break;
            case EvolutionStage.Reset:
                Reset();
                List<Genome> sortedGenomesList = gamesDictionary.Values.OrderBy((Genome genome) => genome.GetFitness()).ToList();
                stage = EvolutionStage.Evolution;
                genomeRenderer.RenderGenome(bestGenome);
                displayGenerationUI.GetComponent<UnityEngine.UI.Text>().text = "Generation: " + generation;
                Debug.Log("________________________________________________________________________________");
                Debug.Log("Generation " + generation);
                break;
            default:
                break;
        }
    }

    public void LoadFromJson(List<Genome> loadedGenomes, Genome loadedBestGenome, int loadedGeneration,List<float> loadedprogressScores, List<float> loadedaverageScores, List<Specie> loadedSpecies)
    {
        generation = loadedGeneration;
        species = loadedSpecies;
        Dictionary<GameObject, Genome> newDictionary = new Dictionary<GameObject, Genome>();
        int i = 0;
        foreach (KeyValuePair<GameObject,Genome> pair in gamesDictionary)
        {
            newDictionary.Add(pair.Key,loadedGenomes[i]);
            i++;
        }
        progressScores = loadedprogressScores;
        averageScores = loadedaverageScores;
        bestGenome = loadedBestGenome;
        gamesDictionary = newDictionary;
        stage = EvolutionStage.Reset;
    }

    
    void Evolution()
    {
        
        if (Time.time - previousTime > clockTime)
        {
            for (int j = 0; j < size; j++)
            {
                PlayGame(games[j]);
            }
            previousTime = Time.time;
        }
}

    void Speciation()
    {
        for (int i = 0; i < games.Count; i++)
        {
            Genome genome = gamesDictionary[games[i]];
            bool foundSimilarSpecie = false;

            //try to find a fitting specie for the current genome
            for (int j = 0; j < species.Count; j++)
            {
               
                float distance = genome.GetDistance(species[j].representative);

                if (distance < distanceThreshold)
                {
                    species[j].AddGenome(genome);
                    foundSimilarSpecie = true;
                    break;
                }
            }

            if (!foundSimilarSpecie)
            {
                Specie newSpecie = new Specie(genome);
                species.Add(newSpecie);
            }
        }
    }


    void Selection()
    {
        //kill half genomes in each specie
        for (int i = 0; i < species.Count; i++)
        {
            List<Genome> genomesToKill = species[i].SpecieSelection();
            for (int j = 0; j < genomesToKill.Count; j++)
            {
                GameObject gameToKill = gamesDictionary.FirstOrDefault((KeyValuePair<GameObject, Genome> pair) => pair.Value == genomesToKill[j]).Key;
                brainlessGames.Add(gameToKill);
                gamesDictionary.Remove(gameToKill);
                games.Remove(gameToKill);
            }
        }

    }

    float GetTotalRelativeFitness()
    {
        float totalSum = 0;
        for (int i = 0; i < species.Count; i++)
        {
            totalSum += species[i].GetSumRelativeFitness();
        }
        return totalSum;
    }


    void Crossover()
    {
        List<Genome> offsprings = new List<Genome>();
        //for each specie
        for (int i = 0; i < species.Count; i++)
        {
            //crossover the genomes in the specie
            int availableOffsprings = Mathf.FloorToInt(species[i].GetSumRelativeFitness() / GetTotalRelativeFitness() * brainlessGames.Count) - 1;
            for (int j = 0; j < availableOffsprings; j++)
            {
                offsprings.Add(species[i].SpecieCrossover());
            }

            //copy the best genome in the specie
            if(availableOffsprings > 0)
            {
                offsprings.Add(species[i].GetRepresentative().Clone());
            }
            
        }
        int brainlessIndex = 0;
        //add the offsprings in the population
        for (int i = 0; i < offsprings.Count; i++)
        {
            games.Add(brainlessGames[brainlessIndex]);
            gamesDictionary.Add(brainlessGames[brainlessIndex], offsprings[i]);
            brainlessIndex++;
        }
        //add more offsprings to fill the gaps
        species = species.OrderByDescending((Specie specie) => specie.GetAverageFitness()).ToList();
        while (games.Count < size)
        {
            games.Add(brainlessGames[brainlessIndex]);
            gamesDictionary.Add(brainlessGames[brainlessIndex], species[0].SpecieCrossover());
            brainlessIndex++;
        }

        brainlessGames = new List<GameObject>();
    }


    private void Reset()
    {
        for (int i = 0; i < size; i++)
        {
            games[i].GetComponent<Game>().ResetGame();
        }
        
        for (int i = 0; i < species.Count; i++)
        {
            species[i].GetGenomes().Clear();
        }
        //Eliminate stale species
        for (int i = 0; i < species.Count; i++)
        {
            species[i].IncreaseStaleGenerations();
            if (species[i].GetStaleGenerations() > 20)
            {
                species.Remove(species[i]);
                i--;
            }
        }

        //Get best performing genome
        bestGenome = GetGenomes().OrderByDescending((Genome genome) => genome.GetFitness()).FirstOrDefault();
        Debug.Log("Avg fitness: " + GetAverageFitness());
        Debug.Log("Max fitness: " + bestGenome.GetFitness());
        progressScores.Add(bestGenome.GetFitness());
        averageScores.Add(GetAverageFitness());
    }

    void InitializePrefabFields()
    {
        Game gameComponent = gamePrefab.GetComponent<Game>();
        gameComponent.SetClockTime(clockTime);
        gameComponent.SetWidthHeight(width, height);
    }

    List<GameObject> GenerateGames()
    {

        for (int i = 0; i < size; i++)
        {
            GameObject newGame = Instantiate(gamePrefab, transform);
            newGame.name = "Game "+ i;
            games.Add(newGame);

        }
        for (int i = 0; i < size; i++)
        {
            Vector2Int offset = new Vector2Int(i % lineSize * 15, (i / lineSize - 1) * 30);
            games[i].transform.position = new Vector2(games[i].transform.position.x + offset.x, games[i].transform.position.y + offset.y);
            games[i].GetComponent<Game>().SetOffset(offset);
        }
        return games;
    }

    List<Genome> GenerateBrains()
    {
        List<Genome> genomes = new List<Genome>();
        for (int i = 0; i < size; i++)
        {
            genomes.Add(GenerateInitialGenome());
        }
        return genomes;
    }

    Genome GenerateInitialGenome()
    {
        int maxNodeIndex = 0;

        List<NodeGene> nodeList = new List<NodeGene>();
        Dictionary<int, ConnectionGene> connectionList = new Dictionary<int, ConnectionGene>();


       

        List<NodeGene> outputNodes = new List<NodeGene>();
        //Generate output nodes
        for (int y = 0; y < 4; y++)
        {
            NodeGene outputNode = new NodeGene(maxNodeIndex, 1);
            outputNodes.Add(outputNode);
            nodeList.Add(outputNode);
            maxNodeIndex++;
        }

        //Generate bias nodes
        NodeGene biasNode = new NodeGene(maxNodeIndex, 0);
        biasNode.SetOutput(0);
        biasNode.setBias(true);
        nodeList.Add(biasNode);
        maxNodeIndex++;


        List<NodeGene> inputNodes = new List<NodeGene>();
        //Generate input nodes
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                NodeGene inputNode = new NodeGene(maxNodeIndex, 0);
                inputNodes.Add(inputNode);
                nodeList.Add(inputNode);
                maxNodeIndex++;
            }
        }

       
        return new Genome(connectionList, nodeList);

    }

    void InsertInDictionary(List<GameObject> gamesList, List<Genome> genomesList)
    {
        for (int i = 0; i < size; i++)
        {
            gamesDictionary.Add(gamesList[i], genomesList[i]);
        }
    }

    float[] InputToArray(GameObject go)
    {
        bool[,] binaryMatrix = go.GetComponent<Game>().GetBinaryMatrix();
        float[] inputArray = new float[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float outputValue = binaryMatrix[x, y] ? 1 : -1;
                inputArray[x + y * width] = outputValue;
            }
        }
        return inputArray;
    }

   public void PlayGame(GameObject go)
    {
        Genome gameGenome = gamesDictionary[go];
        Game gameInstance = go.GetComponent<Game>();
        gameInstance.ClockTickFall();

        gameGenome.FeedForward(InputToArray(go));
        List<int> commandId = gameGenome.GetActiveCommandsId();


        if(commandId.Count > 0)
        {
            gameInstance.ExecuteCommand(commandId[0]);
        }
    }

    public void SetFitnesses()
    {
        for (int i = 0; i < gamesDictionary.Count; i++)
        {
            GameObject currentGame = games[i];
            int fitnessScore = currentGame.GetComponent<Game>().GetScore();
            gamesDictionary[currentGame].ChangeFitness(fitnessScore);
        }
    }

    public float GetAverageFitness()
    {
        float totalFitness = 0;
        for (int i = 0; i < GetGenomes().Count; i++)
        {
            totalFitness += GetGenomes()[i].GetFitness();
        }

        return totalFitness / GetGenomes().Count;
    }

    public void CreatePrintSprites()
    {
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x*2, y*2f, 0);

                spritesMatrix[x, y] = Instantiate(renderSprite, gameRenderer.transform.position+position, Quaternion.identity, gameRenderer.transform);

            }
        }
    }

    public void RenderBinaryMatrix(GameObject gameObject)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (gameObject.GetComponent<Game>().GetBinaryMatrix()[x,y])
                {
                    spritesMatrix[x, y].GetComponent<SpriteRenderer>().color = Color.white;
                }
                else
                {
                    spritesMatrix[x, y].GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
        }

    }

    public List<Genome> GetGenomes()
    {
        return gamesDictionary.Values.ToList();
    }

   public List<Specie> GetSpecies()
   {
        return species;
   }

   public Genome GetBestGenome()
   {
        return bestGenome;
   }

   public int GetGeneration()
   {
        return generation;
   }

   public List<float> GetProgressScores()
   {
        return progressScores;
   }

   public List<float> GetAverageScores()
   {
        return averageScores;
   }
}
