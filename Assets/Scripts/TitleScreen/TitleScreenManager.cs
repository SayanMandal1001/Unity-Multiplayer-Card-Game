using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
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

    public GameObject settingsButton;
    public GameObject settingsPanel;
    public GameObject playButton;

    public AudioManager audioManager;

    private void Start()
    {
        startAnimation();
        settingsCloseButton();
        audioManager = AudioManager.instance;
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

    public void settingsOpenButton()
    {
        settingsButton.SetActive(false);
        settingsPanel.SetActive(true);
        playButton.SetActive(false);

        foreach (Sound s in audioManager.sounds)
        {
            if (s.name == "BackgroundMusic")
            {
                settingsPanel.transform.GetChild(0).transform.GetChild(1).GetComponent<UnityEngine.UI.Slider>().value = s.volume;
            }
            else
            {
                settingsPanel.transform.GetChild(1).transform.GetChild(1).GetComponent<UnityEngine.UI.Slider>().value = s.volume;
            }
        }

    }
    public void settingsCloseButton()
    {
        settingsButton.SetActive(true);
        settingsPanel.SetActive(false);
        playButton.SetActive(true);
    }

    public void musicSliderChange(UnityEngine.UI.Slider slider)
    {
        foreach(Sound s in audioManager.sounds)
        {
            if(s.name == "BackgroundMusic")
            {
                s.volume = slider.value;
            }
        }
    }

    public void SFXSliderChange(UnityEngine.UI.Slider slider)
    {
        foreach (Sound s in audioManager.sounds)
        {
            if (s.name != "BackgroundMusic")
            {
                s.volume = slider.value;
            }
        }
    }

}
