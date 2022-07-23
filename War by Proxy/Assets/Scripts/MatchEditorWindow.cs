using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchEditorWindow : MonoBehaviour
{
    static List<string> weatherList = new List<string>{ "Clear","Rain","Blizzard","Sandstorm"};
    static List<string> texturesList = new List<string>{ "Regular","Desert","Snowlands","Wastelands"};
    static List<string> aiList = new List<string>{"Balanced","Offensive","Defensive"};
    private int selectedWeatherIndex = 0;
    private int selectedTexturesIndex = 0;
    private int selectedAIIndex = 0;
    public GameObject selectedWeatherText;
    public GameObject selectedTexturesText;
    public GameObject selectedAIText;
    public GameObject resourcesInput;
    public GameObject turnsInput;
    public GameObject fogCheckmark;
    public GameObject dominationCheckmark;
    public GameObject powersCheckmark;
    public GameObject InformationText;
    
    public void Start()
    {
        PrintSelected();
    }

    public void SetIndex(int index)
    {
        selectedWeatherIndex = index;
        selectedTexturesIndex = index;
        selectedAIIndex = index;
    }

    public void Reset()
    {
        selectedWeatherIndex = 0;
        selectedTexturesIndex = 0;
        selectedAIIndex = 0;
        resourcesInput.GetComponent<TMP_InputField>().text = "1000";
        turnsInput.GetComponent<TMP_InputField>().text = "15";
        fogCheckmark.SetActive(false);
        dominationCheckmark.SetActive(false);
        powersCheckmark.SetActive(false);
        PrintSelected();
    }

    public void Description(string info)
    {
        InformationText.GetComponent<TextMeshProUGUI>().text = info;
    }

    public void OptionScrollRight(int mode)
    {
        switch(mode)
        {
            case 1:
                if(selectedWeatherIndex == weatherList.Count - 1)
                {
                    selectedWeatherIndex = 0;
                }
                else
                {
                    selectedWeatherIndex++;
                }
                break;
            case 2:
                if(selectedTexturesIndex == texturesList.Count - 1)
                {
                    selectedTexturesIndex = 0;
                }
                else
                {
                    selectedTexturesIndex++;
                }
                break;
            default:
                if(selectedAIIndex == aiList.Count - 1)
                {
                    selectedAIIndex = 0;
                }
                else
                {
                    selectedAIIndex++;
                }
                break;
        }
        PrintSelected();
    }

    public void OptionScrollLeft(int mode)
    {
        switch(mode)
        {
            case 1:
                if(selectedWeatherIndex == 0)
                {
                    selectedWeatherIndex = weatherList.Count - 1;
                }
                else
                {
                    selectedWeatherIndex--;
                }
                break;
            case 2:
                if(selectedTexturesIndex == 0)
                {
                    selectedTexturesIndex = texturesList.Count - 1;
                }
                else
                {
                    selectedTexturesIndex--;
                }
                break;
            default:
                if(selectedAIIndex == 0)
                {
                    selectedAIIndex = aiList.Count - 1;
                }
                else
                {
                    selectedAIIndex--;
                }
                break;
        }
        PrintSelected();
    }

    public void PrintSelected()
    {
        selectedWeatherText.GetComponent<TextMeshProUGUI>().text = weatherList[selectedWeatherIndex];
        selectedTexturesText.GetComponent<TextMeshProUGUI>().text = texturesList[selectedTexturesIndex];
        selectedAIText.GetComponent<TextMeshProUGUI>().text = aiList[selectedAIIndex];
    }
}
