﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    [System.Serializable]
    public class NatureObject
    {
        public string name;
        public GameObject prefab;
        public float minThreshold;
        public float maxThreshold;
        public float minScale = 1.0f;
        public float maxScale = 1.0f;
    }

    [System.Serializable]
    public class NatureObjectCategory
    {
        public string name;
        public float minThreshold;
        public float maxThreshold;
        public bool useBiomeGeneration;
        public List<NatureObject> Objects;
    }


    [System.Serializable]
    public class HexTileType
    {
        public string name;
        public GameObject prefab;
        public float minThreshold;
        public float maxThreshold;
        public List<NatureObjectCategory> ObjectCategories;
    }

    public List<HexTileType> hexTileTypes;
    public int mapWidth = 20;
    public int mapHeight = 20;
    public float hexTileSize = 1.0f;
    public float noiseScale = 0.1f;
    public float objectNoiseScale = 0.5f;
    public float biomeNoiseScale = 0.05f; // Échelle du Perlin Noise pour les biomes

    private Dictionary<Vector2, GameObject> hexMap;

    public Population[] initialPopulations;

    [System.Serializable]
    public struct Population
    {
        public LivingEntity prefab;
        public int count;
    }
    void Start()
    {
        Environment.prng = new System.Random();
        Environment.walkableNeighboursMap = new Coord[mapWidth, mapHeight][];

        int numberOfPlants = GameSettings.Instance.numberOfPlants;
        int numberOfNPC1 = GameSettings.Instance.numberOfNPC1;
        int numberOfNPC2 = GameSettings.Instance.numberOfNPC2;

        // Ajuster les populations initiales
        initialPopulations[0].count = numberOfPlants;
        initialPopulations[1].count = numberOfNPC1;
        initialPopulations[2].count = numberOfNPC2;
        GenerateHexMap();
        SpawnAll();

        // Environment.info();
    }

    void GenerateHexMap()
    {
        float hexWidth = Mathf.Sqrt(3) * hexTileSize;
        float hexHeight = 2f * hexTileSize;
        hexMap = new Dictionary<Vector2, GameObject>();

        Environment.tileCentres = new Vector3[mapWidth, mapHeight];
        Environment.walkable = new bool[mapWidth, mapHeight];

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float xPos = x * hexWidth * 1.15f;
                if (y % 2 == 1)
                {
                    xPos += hexWidth * 0.575f;
                }
                float zPos = y * (hexHeight * 0.866f);

                Vector3 pos = new Vector3(xPos, 0, zPos);

                float perlinValue = Mathf.Clamp01(Mathf.PerlinNoise(x * noiseScale, y * noiseScale));
                GameObject tilePrefab = GetTilePrefabByPerlinValue(perlinValue);

                if (tilePrefab != null)
                {
                    GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, this.transform);
                    Vector2 key = new Vector2(x, y);
                    if (!hexMap.ContainsKey(key))
                    {
                        hexMap.Add(key, tile);
                        HexTileType tileType = GetTileTypeByPrefab(tilePrefab);
                        Environment.tileCentres[x, y] = pos;
                        Environment.walkable[x, y] = tilePrefab.name != "water";


                        if (tileType != null && tileType.ObjectCategories != null)
                        {
                            NatureObjectCategory selectedCategory = null;

                            // Pour chaque catégorie d'objet 
                            foreach (NatureObjectCategory category in tileType.ObjectCategories)
                            {
                                float categoryValue;
                                if (category.useBiomeGeneration)
                                {
                                    categoryValue = Mathf.Clamp01(Mathf.PerlinNoise(x * biomeNoiseScale, y * biomeNoiseScale));
                                }
                                else
                                {
                                    categoryValue = perlinValue;
                                }

                                if (categoryValue >= category.minThreshold && categoryValue <= category.maxThreshold)
                                {
                                    selectedCategory = category;
                                    break;
                                }
                            }

                            if (selectedCategory != null)
                            {
                                // Pour chaque objet
                                foreach (NatureObject natureObject in selectedCategory.Objects)
                                {
                                    float objectPerlinValue = Mathf.Clamp01(Mathf.PerlinNoise((x + 100) * objectNoiseScale, (y + 100) * objectNoiseScale));
                                    if (objectPerlinValue >= natureObject.minThreshold && objectPerlinValue <= natureObject.maxThreshold)
                                    {
                                        Vector3 objectPos = pos;

                                        // Instanciation 
                                        GameObject obj = Instantiate(natureObject.prefab, objectPos, Quaternion.identity, tile.transform);

                                        float scale = Random.Range(natureObject.minScale, natureObject.maxScale);
                                        obj.transform.localScale = new Vector3(scale, scale, scale);

                                        // Application d'une rotation aléatoire uniquement sur l'axe Y
                                        float randomYRotation = Random.Range(0f, 360f);
                                        obj.transform.rotation = Quaternion.Euler(obj.transform.rotation.eulerAngles.x, randomYRotation, obj.transform.rotation.eulerAngles.z);

                                    }
                                    else
                                    {
                                        // Vector3 objectPos = pos;
                                        // Debug.LogWarning($"NatureObject '{natureObject.name}' at position {objectPos} did not meet the threshold requirements.");
                                    }
                                }
                            }
                            else
                            {
                                // Debug.LogWarning($"No NatureObjectCategory met the threshold requirements for tile at position {pos}.");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"HexTileType for prefab '{tilePrefab.name}' has no valid ObjectCategories or is null.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Tile at position {pos} already exists in hexMap. Skipping...");
                    }
                }
                else
                {
                    Debug.LogWarning($"No tilePrefab found for Perlin value {perlinValue} at position {pos}.");
                }
            }
        }
        Debug.Log($"HexMap generation completed");
    }

    GameObject GetTilePrefabByPerlinValue(float perlinValue)
    {
        foreach (HexTileType tileType in hexTileTypes)
        {
            if (perlinValue >= tileType.minThreshold && perlinValue <= tileType.maxThreshold)
            {
                return tileType.prefab;
            }
        }

        return null;
    }

    HexTileType GetTileTypeByPrefab(GameObject prefab)
    {
        foreach (HexTileType tileType in hexTileTypes)
        {
            if (tileType.prefab == prefab)
            {
                return tileType;
            }
        }

        return null;
    }

    void SpawnAll()
    {
        // Initialisation des espèces et cartes
        if (initialPopulations.Length > 1 && initialPopulations[1].prefab is Npc)
        {
            int numSpecies = System.Enum.GetNames(typeof(Species)).Length;
            Environment.preyBySpecies = new Dictionary<Species, List<Species>>();
            Environment.predatorsBySpecies = new Dictionary<Species, List<Species>>();
            Environment.speciesMaps = new Dictionary<Species, Map>();

            for (int i = 0; i < numSpecies; i++)
            {
                Species species = (Species)(1 << i);
                Environment.speciesMaps.Add(species, new Map(mapWidth, 10));

                Environment.preyBySpecies.Add(species, new List<Species>());
                Environment.predatorsBySpecies.Add(species, new List<Species>());
            }

            Npc hunter = initialPopulations[1].prefab as Npc;
            Species diet = hunter.diet;

            for (int huntedSpeciesIndex = 0; huntedSpeciesIndex < numSpecies; huntedSpeciesIndex++)
            {
                int bit = ((int)diet >> huntedSpeciesIndex) & 1;
                if (bit == 1)
                {
                    int huntedSpecies = 1 << huntedSpeciesIndex;

                    Environment.preyBySpecies[hunter.species].Add((Species)huntedSpecies);
                    Environment.predatorsBySpecies[(Species)huntedSpecies].Add(hunter.species);
                }
            }
        }

        foreach (Population population in initialPopulations)
        {
            for (int i = 0; i < population.count; i++)
            {
                Vector2Int randomCoord = new Vector2Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
                while (!Environment.walkable[randomCoord.x, randomCoord.y] || IsCoastTile(randomCoord) || !IsCoordValid(randomCoord))
                {
                    randomCoord = new Vector2Int(Random.Range(0, mapWidth), Random.Range(0, mapHeight));
                }
                Vector3 spawnPos = Environment.tileCentres[randomCoord.x, randomCoord.y];
                GameObject ent = Instantiate(population.prefab.gameObject, spawnPos, Quaternion.identity);
                LivingEntity npcScript = ent.GetComponent<LivingEntity>();
                if (npcScript != null)
                {
                    npcScript.isPrefabBase = false;
                    npcScript.Init(new Coord(randomCoord.x, randomCoord.y));
                    Environment.speciesMaps[npcScript.species].Add(npcScript, new Coord(randomCoord.x, randomCoord.y));
                }
            }


            if (population.prefab is Npc)
            {
                int numSpecies = System.Enum.GetNames(typeof(Species)).Length;

                Npc hunter = population.prefab as Npc;
                Species diet = hunter.diet;

                for (int huntedSpeciesIndex = 0; huntedSpeciesIndex < numSpecies; huntedSpeciesIndex++)
                {
                    int bit = ((int)diet >> huntedSpeciesIndex) & 1;
                    if (bit == 1)
                    {
                        int huntedSpecies = 1 << huntedSpeciesIndex;
                        Environment.preyBySpecies[hunter.species].Add((Species)huntedSpecies);
                        Environment.predatorsBySpecies[(Species)huntedSpecies].Add(hunter.species);
                    }
                }
            }
        }
    }

    private bool IsCoordValid(Vector2Int coord)
    {
        return coord.x >= 0 && coord.x < mapWidth && coord.y >= 0 && coord.y < mapHeight;
    }


    bool IsCoastTile(Vector2Int coord)
    {
        int x = coord.x;
        int y = coord.y;

        // Initialiser un générateur de nombres aléatoires
        System.Random random = new System.Random();

        // Vérifie les tuiles adjacentes pour voir si l'une d'entre elles est de l'eau
        bool isAdjacentToCoast =
            (x > 0 && !Environment.walkable[x - 1, y]) ||
            (x < mapWidth - 1 && !Environment.walkable[x + 1, y]) ||
            (y > 0 && !Environment.walkable[x, y - 1]) ||
            (y < mapHeight - 1 && !Environment.walkable[x, y + 1]) ||
            (x > 0 && y > 0 && !Environment.walkable[x - 1, y - 1]) ||
            (x > 0 && y < mapHeight - 1 && !Environment.walkable[x - 1, y + 1]) ||
            (x < mapWidth - 1 && y > 0 && !Environment.walkable[x + 1, y - 1]) ||
            (x < mapWidth - 1 && y < mapHeight - 1 && !Environment.walkable[x + 1, y + 1]);

        if (isAdjacentToCoast)
        {
            // Générer un nombre aléatoire entre 0 et 1
            double chance = random.NextDouble();

            // 10% de chance de retourner true
            if (chance <= 0.1)
            {
                return true;
            }
        }

        return false;
    }

}