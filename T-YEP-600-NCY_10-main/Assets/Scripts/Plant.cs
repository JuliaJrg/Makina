using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : LivingEntity
{
    float amountRemaining = 1;
    const float consumeSpeed = 8;

    public void Consume()
    {
        // amountRemaining -= amount * consumeSpeed;
        if (!dead)
        {
            amountRemaining = 0;
            transform.localScale = Vector3.one * amountRemaining;

            dead = true;
            Environment.RemoveOne(species);
            // Debug.Log("Plant consumed");
        }
        // if (amountRemaining <= 0)
        // {
        //     Die(CauseOfDeath.Eaten);
        // }
    }

    protected virtual void Update()
    {
        if (!isPrefabBase)
        {

            lifeTime += Time.deltaTime;
            if ((int)lifeTime == newgeneration)
            {
                lifeTime = 0;
                if (dead)
                {
                    RegeneratePlant();

                }
                else
                {
                    NewEntity();
                }
            }
        }

    }

    private void RegeneratePlant()
    {
        amountRemaining = 1;
        transform.localScale = Vector3.one;
        dead = false;
        Environment.AddOne(species);
    }

}