using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour {
    //fields
    private int lives;

    //properties
    public int Lives { get { return this.lives; } }

    //methods
    private void Start()
    {
        Revive();
    }
    public bool takeDamage(int damageAmount)
    {
        if(lives > 0)
        {
            lives -= damageAmount;
            return true;
        }
        return false;
    }
    public void Revive()
    {
        this.lives = 2;
    }
}
