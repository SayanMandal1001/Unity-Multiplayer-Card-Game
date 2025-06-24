using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinCard : MonoBehaviour
{
    public float rotationSpeed;
    public float translationSpeed;
    public Vector2 projectingDirection;

    private void Start()
    {
        float e = Random.Range(0f, 1f);
        if(e < 0.5) rotationSpeed*=-1;
    }
    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0f,0f,(rotationSpeed * Time.deltaTime));
        transform.position += new Vector3(translationSpeed * projectingDirection.x * Time.deltaTime, translationSpeed * projectingDirection.y * Time.deltaTime, 0f);
    }
    public void setProjectingDirection(Vector2 dir)
    {
        projectingDirection=dir;
    }
    public void setRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    public void setTranslationSpeed(float speed)
    {
        translationSpeed = speed;
    }
}
