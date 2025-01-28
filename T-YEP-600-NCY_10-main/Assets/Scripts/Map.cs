using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// The map is divided into n x n regions, with a list of entities for each region
// This allows an entity to more quickly find other nearby entities
public class Map
{
    public readonly List<LivingEntity>[,] map;
    readonly Vector2[,] centres;
    readonly int regionSize;
    readonly int numRegions;
    public int numEntities;

    public Map(int size, int regionSize)
    {
        this.regionSize = regionSize;
        numRegions = Mathf.CeilToInt(size / (float)regionSize);
        map = new List<LivingEntity>[numRegions, numRegions];
        centres = new Vector2[numRegions, numRegions];

        for (int y = 0; y < numRegions; y++)
        {
            for (int x = 0; x < numRegions; x++)
            {
                Coord regionBottomLeft = new Coord(x * regionSize, y * regionSize);
                Coord regionTopRight = new Coord(x * regionSize + regionSize, y * regionSize + regionSize);
                Vector2 centre = (Vector2)regionBottomLeft + (Vector2)((Vector2)regionTopRight / 2f);
                centres[x, y] = centre;
                map[x, y] = new List<LivingEntity>();
            }
        }
    }

    public List<LivingEntity> GetEntities(Coord origin, float viewDistance)
    {
        List<RegionInfo> visibleRegions = GetRegionsInView(origin, viewDistance);
        float sqrViewDst = viewDistance * viewDistance;
        var visibleEntities = new List<LivingEntity>();

        for (int i = 0; i < visibleRegions.Count; i++)
        {
            Coord regionCoord = visibleRegions[i].coord;

            for (int j = 0; j < map[regionCoord.x, regionCoord.y].Count; j++)
            {
                LivingEntity entity = map[regionCoord.x, regionCoord.y][j];
                float sqrDst = Coord.SqrDistance(entity.coord, origin);
                if (sqrDst < sqrViewDst)
                {
                    if (EnvironmentUtility.TileIsVisibile(origin.x, origin.y, entity.coord.x, entity.coord.y))
                    {
                        visibleEntities.Add(entity);
                    }
                }
            }
        }

        return visibleEntities;
    }

    public LivingEntity ClosestEntity(Coord origin, float viewDistance)
    {
        List<RegionInfo> visibleRegions = GetRegionsInView(origin, viewDistance);
        LivingEntity closestEntity = null;
        float closestSqrDst = viewDistance * viewDistance + 0.01f;

        for (int i = 0; i < visibleRegions.Count; i++)
        {
            // Stop searching if current closest entity is closer than the dst to the region edge
            // All remaining regions will be further as well, since sorted by dst
            if (closestSqrDst <= visibleRegions[i].sqrDstToClosestEdge)
            {
                break;
            }

            Coord regionCoord = visibleRegions[i].coord;

            for (int j = 0; j < map[regionCoord.x, regionCoord.y].Count; j++)
            {
                LivingEntity entity = map[regionCoord.x, regionCoord.y][j];
                float sqrDst = Coord.SqrDistance(entity.coord, origin);
                if (sqrDst < closestSqrDst)
                {
                    if (EnvironmentUtility.TileIsVisibile(origin.x, origin.y, entity.coord.x, entity.coord.y))
                    {
                        closestSqrDst = sqrDst;
                        closestEntity = entity;
                    }
                }
            }
        }

        return closestEntity;
    }

    public List<RegionInfo> GetRegionsInView(Coord origin, float viewDistance)
    {
        List<RegionInfo> regions = new List<RegionInfo>();
        int originRegionX = origin.x / regionSize;
        int originRegionY = origin.y / regionSize;
        float sqrViewDst = viewDistance * viewDistance;
        Vector2 viewCentre = (Vector2)origin + Vector2.one * .5f;

        int searchNum = Mathf.Max(1, Mathf.CeilToInt(viewDistance / regionSize));
        // Loop over all regions that might be within the view dst to check if they actually are
        for (int offsetY = -searchNum; offsetY <= searchNum; offsetY++)
        {
            for (int offsetX = -searchNum; offsetX <= searchNum; offsetX++)
            {
                int viewedRegionX = originRegionX + offsetX;
                int viewedRegionY = originRegionY + offsetY;

                if (viewedRegionX >= 0 && viewedRegionX < numRegions && viewedRegionY >= 0 && viewedRegionY < numRegions)
                {
                    // Calculate distance from view coord to closest edge of region to test if region is in range
                    float ox = Mathf.Max(0, Mathf.Abs(viewCentre.x - centres[viewedRegionX, viewedRegionY].x) - regionSize / 2f);
                    float oy = Mathf.Max(0, Mathf.Abs(viewCentre.y - centres[viewedRegionX, viewedRegionY].y) - regionSize / 2f);
                    float sqrDstFromRegionEdge = ox * ox + oy * oy;
                    if (sqrDstFromRegionEdge <= sqrViewDst)
                    {
                        RegionInfo info = new RegionInfo(new Coord(viewedRegionX, viewedRegionY), sqrDstFromRegionEdge);
                        regions.Add(info);
                    }
                }
            }
        }

        // Sort the regions list from nearest to farthest
        regions.Sort((a, b) => a.sqrDstToClosestEdge.CompareTo(b.sqrDstToClosestEdge));

        return regions;
    }

    public void Add(LivingEntity e, Coord coord)
    {
        if (!IsCoordValid(coord))
        {
            Debug.LogError($"Add: Coordonnées en dehors des limites: {coord}");
            return;
        }

        int regionX = coord.x / regionSize;
        int regionY = coord.y / regionSize;

        LivingEntity entity = Environment.NPCRencontre(e, coord);

        if (entity != null && entity != e && entity.species != Species.Plant && e.species != Species.Plant)
        {
            Rencontre((Npc)e, coord, regionX, regionY, (Npc)entity);
        }
        else
        {
            e.mapCoord = coord;
            map[regionX, regionY].Add(e);
            numEntities++;
        }
    }

    public LivingEntity NpcRencontre(LivingEntity e, Coord coord)
    {
        int regionX = coord.x / regionSize;
        int regionY = coord.y / regionSize;
        return map[regionX, regionY].Find(x => x.coord == coord);
    }

    void Rencontre(Npc e, Coord coord, int regionX, int regionY, Npc entityRencontre)
    {
        if (e.DernierNpcRencontre != entityRencontre)
        {
            // if (e.altruisme < entityRencontre.altruisme)
            // {
            //     Debug.Log("Salut t'as pas une pièce " + coord);

            // }
            // else if (e.altruisme > entityRencontre.altruisme)
            // {
            //     Debug.Log("Salut tu veux une pièce " + coord);
            // }
            // else
            // {
            //     Debug.Log("Salut ça va ? " + coord);
            // }
            if (e.species != entityRencontre.species)
            {
                if (e.pacifisme < 0.3f)
                {
                    e.PlantsEaten += entityRencontre.PlantsEaten;
                    entityRencontre.destroid();
                    Environment.RemoveOne(entityRencontre.species);
                    entityRencontre.DernierNpcRencontre = null;
                    Debug.Log("Le " + e.species + " a tué le " + entityRencontre.species + " car pas pacifiste " + e.pacifisme);
                }
                else if (entityRencontre.pacifisme < 0.3f)
                {
                    entityRencontre.PlantsEaten += e.PlantsEaten;
                    e.destroid();
                    Environment.RemoveOne(e.species);
                    e.DernierNpcRencontre = null;
                    Debug.Log("Le " + entityRencontre.species + " a tué le " + e.species + " car la rencontre n'était pas bien " + entityRencontre.pacifisme);
                }
                //     if (e.species == Species.Npc1)
                //     {
                //         Debug.Log("Le Npc1 intéragi avec le Npc2 à " + coord);
                //     }
                //     else if (e.species == Species.Npc2)
                //     {
                //         Debug.Log("Le Npc2 intéragi avec le Npc1 à " + coord);
                //     }
            }
            // else
            // {
            //     Debug.Log("Deux " + e.species + " se rencontrent à " + coord);
            // }
            // Debug.Log("Rencontre entre " + e.GetInstanceID() + " avec " + e.altruisme + " et " + entity.GetInstanceID() + " avec " + entity.altruisme + " à " + coord);
            e.DernierNpcRencontre = entityRencontre;
            entityRencontre.DernierNpcRencontre = e;
        }
        // else
        // {
        //     Debug.Log("Rencontre déjà effectuée");
        // }
        e.mapCoord = coord;
        map[regionX, regionY].Add(e);
        numEntities++;
        // Destruction de l'un ou de l'autre (pour la bagarre)
        // if (entity.GetInstanceID() < e.GetInstanceID())
        // {
        //     map[regionX, regionY].RemoveAt(map[regionX, regionY].Count - 1);
        //     e.mapCoord = coord;
        //     map[regionX, regionY].Add(e);
        // }
        // else
        // {
        //     e.destroid();
        // }
    }

    public void Remove(LivingEntity e, Coord coord)
    {
        if (!IsCoordValid(coord))
        {
            Debug.LogError($"Remove: Coordonnées en dehors des limites: {coord}");
            return;
        }

        int regionX = coord.x / regionSize;
        int regionY = coord.y / regionSize;

        int lastElementIndex = map[regionX, regionY].Count - 1;

        map[regionX, regionY].RemoveAt(lastElementIndex);
        numEntities--;
    }

    public void Move(LivingEntity e, Coord fromCoord, Coord toCoord)
    {
        if (!IsCoordValid(fromCoord) || !IsCoordValid(toCoord))
        {
            Debug.LogError($"Move: Coordonnées en dehors des limites: from {fromCoord} to {toCoord}");
            return;
        }

        Remove(e, fromCoord);
        Add(e, toCoord);
    }

    private bool IsCoordValid(Coord coord)
    {
        int x = coord.x / regionSize;
        int y = coord.y / regionSize;
        return x >= 0 && x < numRegions && y >= 0 && y < numRegions;
    }

    public struct RegionInfo
    {
        public readonly Coord coord;
        public readonly float sqrDstToClosestEdge;

        public RegionInfo(Coord coord, float sqrDstToClosestEdge)
        {
            this.coord = coord;
            this.sqrDstToClosestEdge = sqrDstToClosestEdge;
        }
    }

    public void DrawDebugGizmos(Coord coord, float viewDst)
    {
        // Settings:
        bool showViewedRegions = true;
        bool showOccupancy = false;
        float height = Environment.tileCentres[0, 0].y + 0.1f;
        Gizmos.color = Color.black;

        // Draw:
        int regionX = coord.x / regionSize;
        int regionY = coord.y / regionSize;

        // Draw region lines
        for (int i = 0; i <= numRegions; i++)
        {
            Gizmos.DrawLine(new Vector3(i * regionSize, height, 0), new Vector3(i * regionSize, height, regionSize * numRegions));
            Gizmos.DrawLine(new Vector3(0, height, i * regionSize), new Vector3(regionSize * numRegions, height, i * regionSize));
        }

        // Draw region centres
        for (int y = 0; y < numRegions; y++)
        {
            for (int x = 0; x < numRegions; x++)
            {
                Vector3 centre = new Vector3(centres[x, y].x, height, centres[x, y].y);
                Gizmos.DrawSphere(centre, .3f);
            }
        }
        // Highlight regions in view
        if (showViewedRegions)
        {
            List<RegionInfo> regionsInView = GetRegionsInView(coord, viewDst);

            for (int y = 0; y < numRegions; y++)
            {
                for (int x = 0; x < numRegions; x++)
                {
                    Vector3 centre = new Vector3(centres[x, y].x, height, centres[x, y].y);
                    for (int i = 0; i < regionsInView.Count; i++)
                    {
                        if (regionsInView[i].coord.x == x && regionsInView[i].coord.y == y)
                        {
                            var prevCol = Gizmos.color;
                            Gizmos.color = new Color(1, 0, 0, 1 - i / Mathf.Max(1, regionsInView.Count - 1f) * .5f);
                            Gizmos.DrawCube(centre, new Vector3(regionSize, .1f, regionSize));
                            Gizmos.color = prevCol;
                        }
                    }
                }
            }
        }

        if (showOccupancy)
        {
            int maxOccupants = 0;
            for (int y = 0; y < numRegions; y++)
            {
                for (int x = 0; x < numRegions; x++)
                {
                    maxOccupants = Mathf.Max(maxOccupants, map[x, y].Count);
                }
            }
            if (maxOccupants > 0)
            {
                for (int y = 0; y < numRegions; y++)
                {
                    for (int x = 0; x < numRegions; x++)
                    {
                        Vector3 centre = new Vector3(centres[x, y].x, height, centres[x, y].y);
                        int numOccupants = map[x, y].Count;
                        if (numOccupants > 0)
                        {
                            var prevCol = Gizmos.color;
                            Gizmos.color = new Color(1, 0, 0, numOccupants / (float)maxOccupants);
                            Gizmos.DrawCube(centre, new Vector3(regionSize, .1f, regionSize));
                            Gizmos.color = prevCol;
                        }
                    }
                }
            }
        }
    }

    internal int GetPopulation()
    {
        return numEntities;
    }

}