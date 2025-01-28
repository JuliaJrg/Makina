using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Npc : LivingEntity
{
    public const int maxViewDistance = 10;

    [EnumFlags]
    public Species diet;
    public int PlantsEaten { get; set; }
    public int Age { get; private set; }

    public CreatureAction currentAction;
    public Genes genes;
    public Color maleColour;
    public Color femaleColour;

    public Animator animator;
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int IsRunning = Animator.StringToHash("IsRunning");

    float timeBetweenActionChoices = 1;
    float walkSpeed = 50f;
    float runSpeed = 40.0f;
    //float timeToDeathByHunger = 30;

    //float criticalPercent = 0.7f;

    [Header("State")]
    public float hunger;
    public float thirst;

    protected LivingEntity foodTarget;
    protected Coord waterTarget;

    bool animatingMovement;
    Coord moveFromCoord;
    Coord moveTargetCoord;
    Vector3 moveStartPos;
    Vector3 moveTargetPos;
    float moveTime;
    float moveSpeedFactor;
    float moveArcHeightFactor;
    Coord[] path;
    int pathIndex;

    float lastActionChooseTime;
    const float sqrtTwo = 1.4142f;
    const float oneOverSqrtTwo = 1 / sqrtTwo;

    public float egoisme;
    public float altruisme;
    public float pacifisme;

    public Npc DernierNpcRencontre;

    public override void Init(Coord coord)
    {
        base.Init(coord);
        moveFromCoord = coord;
        genes = Genes.RandomGenes(1);

        egoisme = Mathf.Round(Random.Range(0f, 1f) * 100f) / 100f;
        altruisme = Mathf.Round(Random.Range(0f, 1f) * 100f) / 100f;
        pacifisme = Mathf.Round(Random.Range(0f, 1f) * 100f) / 100f;

        material.color = genes.isMale ? maleColour : femaleColour;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("Animator component not found on NPC.");
        }
        else if (animator.runtimeAnimatorController == null)
        {
            Debug.LogError("Animator does not have an AnimatorController assigned.");
        }
        else
        {
            if (AnimatorHasParameter(animator, "Speed") && AnimatorHasParameter(animator, "IsWalking") && AnimatorHasParameter(animator, "IsRunning"))
            {
                ChooseNextAction();
            }
            else
            {
                Debug.LogError("Animator is missing required parameters.");
            }
        }

        TimeManager.OnDayChanged += ResetHunger;
    }

    protected virtual void Update()
    {
        if (!isPrefabBase)
        {
            hunger += Time.deltaTime * 1 / (newgeneration - 5);
            thirst += Time.deltaTime * 1 / (newgeneration - 5);

            lifeTime += Time.deltaTime;

            if (animatingMovement)
            {
                AnimateMove();
            }
            else
            {
                HandleInteractions();
                float timeSinceLastActionChoice = Time.time - lastActionChooseTime;
                if (timeSinceLastActionChoice > timeBetweenActionChoices)
                {
                    ChooseNextAction();
                }
            }

            if (hunger >= 1)
            {
                Die(CauseOfDeath.Hunger);
            }

            if ((int)lifeTime == newgeneration)
            {
                lifeTime = 0;
                hunger = 0;
                NewEntity();
            }
        }
    }

    protected void ChooseNextAction()
    {
        lastActionChooseTime = Time.time;
        if (hunger > 0)
        {
            FindFood();
        }
        // bool currentlyEating = currentAction == CreatureAction.Eating && foodTarget && hunger > 0;
        // if (hunger >= thirst || currentlyEating && thirst < criticalPercent)
        // {
        //     FindFood();
        // }
        // else
        // {
        //     FindWater();
        // }

        // if (pacifisme > 0.5f)
        // {
        //     AvoidConflicts();
        // }

        Act();
    }

    protected void FindFood()
    {
        LivingEntity foodSource = Environment.SenseFood(coord, this, FoodPreferencePenalty);
        if (foodSource)
        {
            currentAction = CreatureAction.GoingToFood;
            foodTarget = foodSource;
            CreatePath(foodTarget.coord);
        }
        else
        {
            currentAction = CreatureAction.Exploring;
        }
    }

    private void OnDayChanged(int day)
    {
        Age++;
        PlantsEaten = 0;
        ResetHunger(day);
    }

    private void ResetHunger(int day)
    {
        hunger = 0f;
    }

    // protected void FindWater()
    // {
    //     Coord waterTile = Environment.SenseWater(coord);
    //     if (waterTile != Coord.invalid)
    //     {
    //         currentAction = CreatureAction.GoingToWater;
    //         waterTarget = waterTile;
    //         CreatePath(waterTarget);
    //     }
    //     else
    //     {
    //         currentAction = CreatureAction.Exploring;
    //     }
    // }

    protected int FoodPreferencePenalty(LivingEntity self, LivingEntity food)
    {
        return Coord.SqrDistance(self.coord, food.coord);
    }

    protected void Act()
    {
        switch (currentAction)
        {
            case CreatureAction.Exploring:
                if (AnimatorHasParameter(animator, "IsWalking") && AnimatorHasParameter(animator, "Speed"))
                {
                    animator.SetBool(IsWalking, true);
                    animator.SetFloat(Speed, walkSpeed);
                }
                Coord nextTile = Environment.GetNextTileWeighted(coord, moveFromCoord);
                StartMoveToCoord(nextTile);
                break;
            case CreatureAction.GoingToFood:
                if (AnimatorHasParameter(animator, "IsWalking") && AnimatorHasParameter(animator, "Speed"))
                {
                    animator.SetBool(IsWalking, true);
                    animator.SetFloat(Speed, runSpeed);
                }
                if (Coord.AreNeighbours(coord, foodTarget.coord))
                {
                    LookAt(foodTarget.coord);
                    currentAction = CreatureAction.Eating;
                }
                else
                {
                    if (path != null && pathIndex < path.Length)
                    {
                        StartMoveToCoord(path[pathIndex]);
                        pathIndex++;
                    }
                    else
                    {
                        ChooseNextAction();
                    }
                }
                break;
                /*    
                case CreatureAction.GoingToWater:
                    if (AnimatorHasParameter(animator, "IsWalking") && AnimatorHasParameter(animator, "Speed"))
                    {
                        animator.SetBool(IsWalking, true);
                        animator.SetFloat(Speed, runSpeed);
                    }
                    if (Coord.AreNeighbours(coord, waterTarget))
                    {
                        LookAt(waterTarget);
                        currentAction = CreatureAction.Drinking;
                    }
                    else
                    {
                        if (path != null && pathIndex < path.Length)
                        {
                            StartMoveToCoord(path[pathIndex]);
                            pathIndex++;
                        }
                        else
                        {
                            ChooseNextAction();
                        }
                    }
                    break;
                */
        }
    }

    protected void CreatePath(Coord target) // cette fonction sert à trouver le chemin le plus court pour atteindre la cible
    {
        path = EnvironmentUtility.GetPath(coord.x, coord.y, target.x, target.y);
        pathIndex = 0;

        // Vérifiez si le chemin est vide
        if (path == null || path.Length == 0)
        {
            // Si le chemin est vide, passez à l'action d'exploration
            currentAction = CreatureAction.Exploring;
            return;
        }
    }

    protected void StartMoveToCoord(Coord target)
    {
        if (!IsCoordValid(target))
        {
            Debug.LogError($"StartMoveToCoord: Coordonnées en dehors des limites: {target}");
            return;
        }

        moveFromCoord = coord;
        moveTargetCoord = target;
        moveStartPos = transform.position;
        moveTargetPos = Environment.tileCentres[moveTargetCoord.x, moveTargetCoord.y];
        animatingMovement = true;

        bool diagonalMove = Coord.SqrDistance(moveFromCoord, moveTargetCoord) > 1;
        moveArcHeightFactor = (diagonalMove) ? sqrtTwo : 1;
        moveSpeedFactor = (diagonalMove) ? oneOverSqrtTwo : 1;

        LookAt(moveTargetCoord);
    }

    private bool IsCoordValid(Coord coord)
    {
        return coord.x >= 0 && coord.x < Environment.tileCentres.GetLength(0) && coord.y >= 0 && coord.y < Environment.tileCentres.GetLength(1);
    }

    protected void LookAt(Coord target)
    {
        if (target != coord)
        {
            Coord offset = target - coord;
            transform.eulerAngles = Vector3.up * Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg;
        }
    }

    void HandleInteractions()
    {
        if (currentAction == CreatureAction.Eating)
        {
            if (AnimatorHasParameter(animator, "IsWalking") && AnimatorHasParameter(animator, "Speed"))
            {
                animator.SetBool(IsWalking, false);
                animator.SetFloat(Speed, 0);
            }
            if (foodTarget && hunger > 0 && Coord.AreNeighbours(coord, foodTarget.coord) && !foodTarget.getDead())
            {
                ((Plant)foodTarget).Consume();

                hunger = 0;
                PlantsEaten++;
            }
            else
            {
                ChooseNextAction();
            }
        }
    }

    void ShareFoodWithOthers(float eatAmount)
    {
        if (egoisme < 0.5f)
        {
            foreach (var npc in Environment.GetNearbyNPCs(coord))
            {
                if (npc != this && npc.hunger > 0)
                {
                    float shareAmount = eatAmount * altruisme;
                    npc.hunger -= shareAmount;
                    hunger += shareAmount;
                }
            }
        }
    }

    void AvoidConflicts()
    {
        foreach (var npc in Environment.GetNearbyNPCs(coord))
        {
            if (npc != this && npc.currentAction == CreatureAction.GoingToFood)
            {
                CreatePath(Environment.GetAlternativeTile(coord));
            }
        }
    }

    void AnimateMove()
    {
        moveTime = Mathf.Min(1, moveTime + Time.deltaTime * moveSpeedFactor);
        transform.position = Vector3.Lerp(moveStartPos, moveTargetPos, moveTime);

        if (moveTime >= 1)
        {
            Environment.RegisterMove(this, coord, moveTargetCoord);
            coord = moveTargetCoord;

            animatingMovement = false;
            moveTime = 0;
            if (AnimatorHasParameter(animator, "IsWalking") && AnimatorHasParameter(animator, "Speed"))
            {
                animator.SetBool(IsWalking, false);
                animator.SetFloat(Speed, 0);
            }
            ChooseNextAction();
        }
    }

    // void OnDrawGizmosSelected()
    // {
    //     if (Application.isPlaying && !isPrefabBase)
    //     {
    //         var surroundings = Environment.Sense(coord);
    //         Gizmos.color = Color.white;
    //         if (surroundings.nearestFoodSource != null)
    //         {
    //             Gizmos.DrawLine(transform.position, surroundings.nearestFoodSource.transform.position);
    //         }
    //         if (surroundings.nearestWaterTile != Coord.invalid)
    //         {
    //             Gizmos.DrawLine(transform.position, Environment.tileCentres[surroundings.nearestWaterTile.x, surroundings.nearestWaterTile.y]);
    //         }

    //         if (currentAction == CreatureAction.GoingToFood)
    //         {
    //             var path = EnvironmentUtility.GetPath(coord.x, coord.y, foodTarget.coord.x, foodTarget.coord.y);
    //             Gizmos.color = Color.black;
    //             for (int i = 0; i < path.Length; i++)
    //             {
    //                 Gizmos.DrawSphere(Environment.tileCentres[path[i].x, path[i].y], .2f);
    //             }
    //         }
    //     }
    // }

    private bool AnimatorHasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }
}
