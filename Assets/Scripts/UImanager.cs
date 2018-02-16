using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UImanager : MonoBehaviour {
    bool showLoadingScreen = true;

    public AudioMixer Master;

    public Canvas SettingsCanvas;
    public Canvas CreditsCanvas;
    void Start()
    { 
        Master.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
        Master.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
        Master.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
        GameObject.Find("MasterSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("MasterVol");
        GameObject.Find("SFXSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("SFXVol");
        GameObject.Find("MusicSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("MusicVol");
        SettingsCanvas.enabled = false;
        CreditsCanvas.enabled = false;
    }

    void Play()
    {
        SceneManager.LoadSceneAsync(showLoadingScreen ? "Loading" : "Game");
    }

    void ResetProgress()
    {
        PlayerPrefs.DeleteKey("Checkpoint");
        SceneManager.LoadSceneAsync(showLoadingScreen ? "Loading" : "Game");
    }

    void Settings()
    {
        SettingsCanvas.enabled = true;
        CreditsCanvas.enabled = false;
    }

    void Credits()
    {
        CreditsCanvas.enabled = true;
        SettingsCanvas.enabled = false;
    }

    void Cancel()
    {
        SettingsCanvas.enabled = false;
        CreditsCanvas.enabled = false;
    }

   void SetMasterlvl(float Masterlvl)
    {
        Masterlvl = GameObject.Find("MasterSlider").GetComponent<Slider>().value;
        Master.SetFloat("MasterVol", Masterlvl);
        PlayerPrefs.SetFloat("MasterVol", Masterlvl);
        PlayerPrefs.Save();
    }

    void SetSFXlvl(float SFXlvl)
    {
        SFXlvl = GameObject.Find("SFXSlider").GetComponent<Slider>().value;
        Master.SetFloat("SFXVol", SFXlvl);
        PlayerPrefs.SetFloat("SFXVol", SFXlvl);
        PlayerPrefs.Save();
    }

    void SetMusiclvl(float Musiclvl)
    {
        Musiclvl = GameObject.Find("MusicSlider").GetComponent<Slider>().value;
        Master.SetFloat("MusicVol", Musiclvl);
        PlayerPrefs.SetFloat("MusicVol", Musiclvl);
        PlayerPrefs.Save();
    }
}
