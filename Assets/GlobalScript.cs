using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalScript : MonoBehaviour
{
    private static string lobbyCode;

    public void setCode(string code) { lobbyCode = code; }
    public string getCode() { return lobbyCode; }
}
