using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Purchasable : MonoBehaviour
{
    public int cost;
    public abstract void Purchase();
}
