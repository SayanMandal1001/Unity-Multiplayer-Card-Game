using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdTextManager : MonoBehaviour
{
    [SerializeField] private int PlayerId;

    public int getPlayerId() {  return PlayerId; }
}
