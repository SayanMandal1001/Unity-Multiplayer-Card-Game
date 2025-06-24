using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class TitleScreenManager : MonoBehaviour
{
    public float screenWidth;
    public float screenHeight;

    public float maxTranslationSpeed = 3f;

    public int maxNumberOfInstanceSpawnable = 5;
    private int numberOfInstances=0;
    public GameObject[] Instances;

    private bool shouldInitialise = false;

    private void Start()
    {
        startAnimation();
    }

    public void startAnimation()
    {
        shouldInitialise = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        screenHeight = Camera.main.orthographicSize;
        screenWidth = screenHeight * Screen.width / Screen.height;
        if (shouldInitialise)
        {
            Instances = GameObject.FindGameObjectsWithTag("Cards");
            for (int i = 0; i < Instances.Length; i++)
            {
                Instances[i].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                Instances[i].transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            shouldInitialise = false;
        }

        if (numberOfInstances < maxNumberOfInstanceSpawnable)
        {
            int e = Random.Range(0, 100);
            if (e < 5)
            {
                int i = Random.Range(0, 4);
                float x = -(screenWidth + 1), y = (screenHeight) + 1;
                Vector2 dir = new Vector2(1f, -1f);
                switch (i)
                {
                    case 0:
                        x = -(screenWidth + 1);
                        y = Random.Range(-(screenHeight + 1), (screenHeight) + 1);
                        dir = new Vector2(Random.Range(0f, 1f), Random.Range(-0.5f, 0.5f));
                        break;
                    case 1:
                        x = Random.Range(-(screenWidth + 1), (screenWidth) + 1);
                        y = -(screenHeight + 1);
                        dir = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0f, 1f));
                        break;
                    case 2:
                        x = (screenWidth) + 1;
                        y = Random.Range(-(screenHeight + 1), (screenHeight) + 1);
                        dir = new Vector2(Random.Range(-1f, 0f), Random.Range(-0.5f, 0.5f));
                        break;
                    case 3:
                        x = Random.Range(-(screenWidth + 1), (screenWidth) + 1);
                        y = (screenHeight) + 1;
                        dir = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-1f, 0f));
                        break;
                }
                numberOfInstances++;
                int index = Random.Range(0, Instances.Length);
                while (Instances[index].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled)
                {
                    index = Random.Range(0, Instances.Length);
                }
                Instances[index].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = true;
                Instances[index].transform.position = new Vector2(x, y);
                Instances[index].AddComponent<SpinCard>();
                Instances[index].GetComponent<SpinCard>().setProjectingDirection(dir);
                Instances[index].GetComponent<SpinCard>().setRotationSpeed(Random.Range(80f, 140f));
                Instances[index].GetComponent<SpinCard>().setTranslationSpeed(Random.Range(maxTranslationSpeed / 2, maxTranslationSpeed));
            }
        }
        for (int i = 0; i < Instances.Length; i++)
        {
            if (Instances[i].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled)
            {
                float x = Instances[i].transform.position.x;
                float y = Instances[i].transform.position.y;
                if (x > (screenWidth) + 2 || x < -((screenWidth) + 2) || y > (screenHeight) + 2 || y < -((screenHeight) + 2))
                {
                    Instances[i].transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    Instances[i].transform.position = new Vector2(0, 0);
                    Instances[i].transform.eulerAngles = new Vector3(0, 0, 0);
                    Destroy(Instances[i].GetComponent<SpinCard>());
                    numberOfInstances--;
                }
            }
        }
    }
}
