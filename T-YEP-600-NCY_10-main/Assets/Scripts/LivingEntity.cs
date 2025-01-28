using UnityEngine;

public class LivingEntity : MonoBehaviour
{
    public bool isPrefabBase;
    public int colourMaterialIndex;
    public Species species;
    public Material material;

    public Coord coord;
    //
    // [HideInInspector] 
    // public int mapIndex;
    [HideInInspector]
    public Coord mapCoord;
    protected float lifeTime;
    protected float newgeneration = 30;

    protected bool dead;

    public virtual void Init(Coord coord)
    {
        this.coord = coord;

        // Vérifier que Environment.tileCentres n'est pas null
        if (Environment.tileCentres == null)
        {
            Debug.LogError("Environment.tileCentres is null");
            return;
        }

        // Vérifier les limites de coord
        if (coord.x < 0 || coord.x >= Environment.tileCentres.GetLength(0) ||
            coord.y < 0 || coord.y >= Environment.tileCentres.GetLength(1))
        {
            Debug.LogError("coord is out of bounds: " + coord);
            return;
        }

        transform.position = Environment.tileCentres[coord.x, coord.y];

        // Set material to the instance material
        var meshRenderer = GetComponentInChildren<MeshRenderer>(true);
        if (meshRenderer != null)
        {
            for (int i = 0; i < meshRenderer.sharedMaterials.Length; i++)
            {
                if (meshRenderer.sharedMaterials[i] is Material)
                {
                    material = meshRenderer.materials[i];
                    break;
                }
            }
        }
        else
        {
            var skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (skinnedMeshRenderer == null)
            {
                Debug.LogError("Neither MeshRenderer nor SkinnedMeshRenderer found on the object or its children");
                return;
            }

            for (int i = 0; i < skinnedMeshRenderer.sharedMaterials.Length; i++)
            {
                if (skinnedMeshRenderer.sharedMaterials[i] is Material)
                {
                    material = skinnedMeshRenderer.materials[i];
                    break;
                }
            }
        }

        if (material == null)
        {
            Debug.LogError("Material not found in MeshRenderer or SkinnedMeshRenderer");
        }
    }

    protected virtual void Die(CauseOfDeath cause)
    {
        if (!dead)
        {
            dead = true;
            // Debug.Log("Dead: " + species + " at " + coord + " by " + cause);
            Destroy(gameObject);
            Environment.RegisterDeath(this);
        }
    }

    protected virtual void NewEntity()
    {
        Environment.RegisterAdd(coord, this.species);

    }
    public virtual void destroid()
    {
        Destroy(gameObject);
    }

    internal bool getDead()
    {
        return dead;
    }
}