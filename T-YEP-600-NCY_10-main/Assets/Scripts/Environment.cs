using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using TerrainGeneration;
using Unity.VisualScripting;
using UnityEngine;

public class Environment : MonoBehaviour
{
    const int mapRegionSize = 10;

    public static int seed;

    [Header("Trees")]
    public MeshRenderer treePrefab;
    [Range(0, 1)]
    public float treeProbability;

    // [Header("Populations")]
    // public Population[] initialPopulations;

    [Header("Debug")]
    public bool showMapDebug;
    public Transform mapCoordTransform;
    public float mapViewDst;

    // Cached data:
    public static Vector3[,] tileCentres;
    public static bool[,] walkable;
    static int size = 50;
    public static Coord[,][] walkableNeighboursMap;
    static List<Coord> walkableCoords;

    public static Dictionary<Species, List<Species>> preyBySpecies;
    public static Dictionary<Species, List<Species>> predatorsBySpecies;

    // array of visible tiles from any tile; value is Coord.invalid if no visible water tile
    static Coord[,] closestVisibleWaterMap;

    public static System.Random prng;

    public static Dictionary<Species, Map> speciesMaps;

    // Liste de tous les NPCs
    static List<Npc> npcs;

    // void Start()
    // {
    //     prng = new System.Random();
    //     npcs = new List<Npc>();

    //     Init();
    // }

    void OnDrawGizmos()
    {
        /* 
        if (showMapDebug) {
            if (preyMap != null && mapCoordTransform != null) {
                Coord coord = new Coord ((int) mapCoordTransform.position.x, (int) mapCoordTransform.position.z);
                preyMap.DrawDebugGizmos (coord, mapViewDst);
            }
        }
        */
    }

    public static LivingEntity NPCRencontre(LivingEntity entity, Coord coord)
    {
        foreach (Species species in speciesMaps.Keys)
        {
            LivingEntity NPCrencontre = speciesMaps[species].NpcRencontre(entity, coord);
            if (NPCrencontre != null && NPCrencontre.species != Species.Plant)
            {
                return NPCrencontre;
            }


        }
        return null;
    }

    public static void RegisterMove(LivingEntity entity, Coord from, Coord to)
    {
        speciesMaps[entity.species].Move(entity, from, to);
    }

    public static void RegisterDeath(LivingEntity entity)
    {
        speciesMaps[entity.species].Remove(entity, entity.coord);
    }

    public static void RegisterAdd(Coord coord, Species species)
    {

        LivingEntity[] allEntities = FindObjectsOfType<LivingEntity>();
        LivingEntity prefab = allEntities.FirstOrDefault(entity => entity.isPrefabBase && entity.species == species);

        if (prefab != null)
        {
            LivingEntity newEntity = Instantiate(prefab);
            if ((coord + Coord.right).x < size && (coord + Coord.right).x > 0 && walkable[(coord + Coord.right).x, coord.y])
            {
                newEntity.coord = coord + Coord.right;
            }
            else if ((coord + Coord.left).x < size && (coord + Coord.left).x > 0 && walkable[(coord + Coord.left).x, coord.y])
            {
                newEntity.coord = coord + Coord.left;
            }
            else if ((coord + Coord.up).y < size && (coord + Coord.up).y > 0 && walkable[coord.x, (coord + Coord.up).y])
            {
                newEntity.coord = coord + Coord.up;
            }
            else if ((coord + Coord.down).y < size && (coord + Coord.down).y > 0 && walkable[coord.x, (coord + Coord.down).y])
            {
                newEntity.coord = coord + Coord.down;
            }
            else
            {
                newEntity.destroid();
                return;
            }
            walkable[newEntity.coord.x, newEntity.coord.y] = false;
            newEntity.isPrefabBase = false;

            newEntity.Init(newEntity.coord);
            speciesMaps[newEntity.species].Add(newEntity, newEntity.coord);
        }
    }

    // public static Coord SenseWater(Coord coord)
    // {
    //     var closestWaterCoord = closestVisibleWaterMap[coord.x, coord.y];
    //     if (closestWaterCoord != Coord.invalid)
    //     {
    //         float sqrDst = (tileCentres[coord.x, coord.y] - tileCentres[closestWaterCoord.x, closestWaterCoord.y]).sqrMagnitude;
    //         if (sqrDst <= Npc.maxViewDistance * Npc.maxViewDistance)
    //         {
    //             return closestWaterCoord;
    //         }
    //     }
    //     return Coord.invalid;
    // }

    public static LivingEntity SenseFood(Coord coord, Npc self, System.Func<LivingEntity, LivingEntity, int> foodPreference)
    {
        if (preyBySpecies == null)
        {
            Debug.LogError("preyBySpecies is not initialized.");
            return null;
        }

        if (!preyBySpecies.ContainsKey(self.species))
        {
            Debug.LogError($"Species {self.species} is not found in preyBySpecies.");
            return null;
        }

        if (speciesMaps == null)
        {
            Debug.LogError("speciesMaps is not initialized.");
            return null;
        }

        var foodSources = new List<LivingEntity>();
        List<Species> prey = preyBySpecies[self.species];
        for (int i = 0; i < prey.Count; i++)
        {
            if (!speciesMaps.ContainsKey(prey[i]))
            {
                Debug.LogError($"speciesMaps does not contain the key: {prey[i]}");
                continue;
            }
            Map speciesMap = speciesMaps[prey[i]];
            if (speciesMap == null)
            {
                Debug.LogError($"speciesMap for {prey[i]} is null.");
                continue;
            }

            foodSources.AddRange(speciesMap.GetEntities(coord, Npc.maxViewDistance));
        }

        // Sort food sources based on preference function
        foodSources.Sort((a, b) => foodPreference(self, a).CompareTo(foodPreference(self, b)));

        // Return first visible food source
        for (int i = 0; i < foodSources.Count; i++)
        {
            Coord targetCoord = foodSources[i].coord;
            if (EnvironmentUtility.TileIsVisibile(coord.x, coord.y, targetCoord.x, targetCoord.y) && !foodSources[i].getDead())
            {
                return foodSources[i];

                // Debug.Log($"Found food at {targetCoord}");
            }
        }

        return null;
    }

    // Return list of animals of the same species, with the opposite gender, who are also searching for a mate
    public static List<Npc> SensePotentialMates(Coord coord, Npc self)
    {
        Map speciesMap = speciesMaps[self.species];
        List<LivingEntity> visibleEntities = speciesMap.GetEntities(coord, Npc.maxViewDistance);
        var potentialMates = new List<Npc>();

        for (int i = 0; i < visibleEntities.Count; i++)
        {
            var visibleNpc = (Npc)visibleEntities[i];
            if (visibleNpc != self && visibleNpc.genes.isMale != self.genes.isMale)
            {
                if (visibleNpc.currentAction == CreatureAction.SearchingForMate)
                {
                    potentialMates.Add(visibleNpc);
                }
            }
        }

        return potentialMates;
    }

    public static Surroundings Sense(Coord coord)
    {
        var closestPlant = speciesMaps[Species.Plant].ClosestEntity(coord, Npc.maxViewDistance);
        var surroundings = new Surroundings();
        Debug.Log("surroundings: " + surroundings + "\nclosestPlant:" + closestPlant);

        surroundings.nearestFoodSource = closestPlant;
        surroundings.nearestWaterTile = closestVisibleWaterMap[coord.x, coord.y];

        return surroundings;
    }

    public static Coord GetNextTileRandom(Coord current)
    {
        if (current.x < 0 || current.x >= walkableNeighboursMap.GetLength(0) || current.y < 0 || current.y >= walkableNeighboursMap.GetLength(1))
        {
            return current;
        }

        var neighbours = walkableNeighboursMap[current.x, current.y];
        if (neighbours == null || neighbours.Length == 0)
        {
            return current;
        }
        return neighbours[prng.Next(neighbours.Length)];
    }

    /// Get random neighbour tile, weighted towards those in similar direction as currently facing
    public static Coord GetNextTileWeighted(Coord current, Coord previous, double forwardProbability = 0.2, int weightingIterations = 3)
    {
        if (current == previous)
        {
            return GetNextTileRandom(current);
        }

        Coord forwardOffset = (current - previous);
        // Random chance of returning foward tile (if walkable)
        if (prng.NextDouble() < forwardProbability)
        {
            Coord forwardCoord = current + forwardOffset;

            if (IsCoordValid(forwardCoord))
            {
                return forwardCoord;
            }
        }

        // Get walkable neighbours
        if (walkableNeighboursMap == null || walkableNeighboursMap[current.x, current.y] == null)
        {
            return current;
        }
        var neighbours = walkableNeighboursMap[current.x, current.y];
        if (neighbours.Length == 0)
        {
            return current;
        }

        // From n random tiles, pick the one that is most aligned with the forward direction:
        Vector2 forwardDir = new Vector2(forwardOffset.x, forwardOffset.y).normalized;
        float bestScore = float.MinValue;
        Coord bestNeighbour = current;

        for (int i = 0; i < weightingIterations; i++)
        {
            Coord neighbour = neighbours[prng.Next(neighbours.Length)];
            if (IsCoordValid(neighbour))
            {
                Vector2 offset = (Vector2)neighbour - (Vector2)current;
                float score = Vector2.Dot(offset.normalized, forwardDir);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestNeighbour = neighbour;
                }
            }
        }

        return bestNeighbour;
    }

    private static bool IsCoordValid(Coord coord)
    {
        return coord.x >= 0 && coord.x < size && coord.y >= 0 && coord.y < size;
    }

    // Call terrain generator and cache useful info
    public static void info()
    {
        int numSpecies = System.Enum.GetNames(typeof(Species)).Length;
        preyBySpecies = new Dictionary<Species, List<Species>>();
        predatorsBySpecies = new Dictionary<Species, List<Species>>();

        // Init species maps
        speciesMaps = new Dictionary<Species, Map>();

        walkable = new bool[size, size];
        walkableNeighboursMap = new Coord[size, size][];

        // Generate offsets within max view distance, sorted by distance ascending
        // Used to speed up per-tile search for closest water tile
        List<Coord> viewOffsets = new List<Coord>();
        int viewRadius = Npc.maxViewDistance;
        int sqrViewRadius = viewRadius * viewRadius;
        for (int offsetY = -viewRadius; offsetY <= viewRadius; offsetY++)
        {
            for (int offsetX = -viewRadius; offsetX <= viewRadius; offsetX++)
            {
                int sqrOffsetDst = offsetX * offsetX + offsetY * offsetY;
                if ((offsetX != 0 || offsetY != 0) && sqrOffsetDst <= sqrViewRadius)
                {
                    viewOffsets.Add(new Coord(offsetX, offsetY));
                }
            }
        }
        viewOffsets.Sort((a, b) => (a.x * a.x + a.y * a.y).CompareTo(b.x * b.x + b.y * b.y));
        Coord[] viewOffsetsArr = viewOffsets.ToArray();
        closestVisibleWaterMap = new Coord[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool foundWater = false;
                if (walkable[x, y])
                {
                    for (int i = 0; i < viewOffsets.Count; i++)
                    {
                        int targetX = x + viewOffsetsArr[i].x;
                        int targetY = y + viewOffsetsArr[i].y;
                        if (targetX >= 0 && targetX < size && targetY >= 0 && targetY < size)
                        {

                            if (EnvironmentUtility.TileIsVisibile(x, y, targetX, targetY))
                            {
                                closestVisibleWaterMap[x, y] = new Coord(targetX, targetY);
                                foundWater = true;
                                break;
                            }

                        }
                    }
                }
                if (!foundWater)
                {
                    closestVisibleWaterMap[x, y] = Coord.invalid;
                }
            }
        }
    }

    void LogPredatorPreyRelationships()
    {
        int numSpecies = System.Enum.GetNames(typeof(Species)).Length;
        for (int i = 0; i < numSpecies; i++)
        {
            string s = "(" + System.Enum.GetNames(typeof(Species))[i] + ") ";
            int enumVal = 1 << i;
            var prey = preyBySpecies[(Species)enumVal];
            var predators = predatorsBySpecies[(Species)enumVal];

            s += "Prey: " + ((prey.Count == 0) ? "None" : "");
            for (int j = 0; j < prey.Count; j++)
            {
                s += prey[j];
                if (j != prey.Count - 1)
                {
                    s += ", ";
                }
            }

            s += " | Predators: " + ((predators.Count == 0) ? "None" : "");
            for (int j = 0; j < predators.Count; j++)
            {
                s += predators[j];
                if (j != predators.Count - 1)
                {
                    s += ", ";
                }
            }
            print(s);
        }
    }

    public static List<Npc> GetNearbyNPCs(Coord coord)
    {
        List<Npc> nearbyNPCs = new List<Npc>();
        foreach (var npc in npcs)
        {
            if (Coord.SqrDistance(npc.coord, coord) <= Npc.maxViewDistance * Npc.maxViewDistance)
            {
                nearbyNPCs.Add(npc);
            }
        }
        return nearbyNPCs;
    }

    public static Coord GetAlternativeTile(Coord currentCoord)
    {
        Coord[] neighbours = walkableNeighboursMap[currentCoord.x, currentCoord.y];
        if (neighbours.Length == 0)
        {
            return currentCoord;
        }
        return neighbours[prng.Next(neighbours.Length)];
    }

    internal static int GetPopulation(Species species)
    {
        return speciesMaps[species].GetPopulation();
    }

    internal static void RemoveOne(Species species)
    {
        speciesMaps[species].numEntities--;
    }

    internal static void AddOne(Species species)
    {
        speciesMaps[species].numEntities++;
    }

    // [System.Serializable]
    // public struct Population
    // {
    //     public LivingEntity prefab;
    //     public int count;
    // }
}