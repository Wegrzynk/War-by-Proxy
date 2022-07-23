using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoundtracksWindow : MonoBehaviour
{
    private List<string> soundtrackList = new List<string>{ "Main Menu Theme", "Campaign Map Theme", "Match 1", "Match 2", "Match 3" };
    private int currentlyPlaying;
    public GameObject soundtrackPanelPrefab;
    private GameObject[] instantiatedSoundtracks;
    public GameObject soundtrackListContent;

    public void Start()
    {
        currentlyPlaying = 0;
        instantiatedSoundtracks = new GameObject[soundtrackList.Count];
        for(int i = 0; i < soundtrackList.Count; i++)
        {
            int copy = i;
            instantiatedSoundtracks[i] = Instantiate(soundtrackPanelPrefab, soundtrackListContent.transform);
            instantiatedSoundtracks[i].transform.Find("NumberPanel").Find("SoundtrackNumber").GetComponent<TextMeshProUGUI>().text = (i+1).ToString();
            instantiatedSoundtracks[i].transform.Find("SoundtrackName").GetComponent<TextMeshProUGUI>().text = soundtrackList[i];
            instantiatedSoundtracks[i].transform.Find("PlayStopButton").GetComponent<Button>().onClick.AddListener(() => PlayStop(copy));
        }
        instantiatedSoundtracks[currentlyPlaying].transform.Find("PlayStopButton").Find("PlayStopButtonText").GetComponent<TextMeshProUGUI>().text = "||";
    }

    public void PlayStop(int index)
    {
        instantiatedSoundtracks[index].transform.Find("PlayStopButton").Find("PlayStopButtonText").GetComponent<TextMeshProUGUI>().text = "||";
        instantiatedSoundtracks[currentlyPlaying].transform.Find("PlayStopButton").Find("PlayStopButtonText").GetComponent<TextMeshProUGUI>().text = ">";
        currentlyPlaying = index;
    }
}
