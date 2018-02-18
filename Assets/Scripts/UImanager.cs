using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

[System.Serializable]
public class UImanager : MonoBehaviour {
    bool showLoadingScreen = true;

    public AudioMixer Master;
    public Canvas SettingsCanvas;
    public Canvas CreditsCanvas;
    public Slider QualitySlider;
    public float qualitylvl;
    public Text qualityText;
    void Start()
    { 
        Master.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
        Master.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
        Master.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
        QualitySlider.value = PlayerPrefs.GetFloat("Quality");
        ChangeQuality(QualitySlider.value);
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

    void ChangeQuality(float level)
    {
        switch ((int)level)
        {
            case 0:
                QualitySettings.SetQualityLevel(0);
                qualityText.text = "Quality - Very low";
                break;
            case 1:
                QualitySettings.SetQualityLevel(1);
                qualityText.text = "Quality - Low";
                break;
            case 2:
                QualitySettings.SetQualityLevel(2);
                qualityText.text = "Quality - Medium";
                break;
            case 3:
                QualitySettings.SetQualityLevel(4);
                qualityText.text = "Quality - High";
                break;
            case 4:
                QualitySettings.SetQualityLevel(5);
                qualityText.text = "Quality - Very high";
                break;
            case 5:
                QualitySettings.SetQualityLevel(6);
                qualityText.text = "Quality - Ultra";
                break;
        }

        qualitylvl = QualitySlider.GetComponent<Slider>().value;
        PlayerPrefs.SetFloat("Quality", qualitylvl);
        PlayerPrefs.Save();
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
