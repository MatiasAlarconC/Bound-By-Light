using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string newGameScene = "MecanicaPulpo";

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource musicSource;

    private const string KEY_SAVE_EXISTS = "SaveExists";
    private const string KEY_SAVED_LEVEL = "SavedLevel";
    private const string KEY_MASTER = "MasterVolume";
    private const string KEY_MUSIC = "MusicVolume";
    private const string KEY_SFX = "SFXVolume";
    private const string KEY_FULLSCREEN = "Fullscreen";
    private const string KEY_LANG = "Language";

    private VisualElement _root;
    private VisualElement _settingsOverlay;
    private VisualElement _angelEl;
    private Button _btnPlay, _btnContinue, _btnSettings, _btnClose;
    private Slider _sMaster, _sMusic, _sSfx;
    private Label _vMaster, _vMusic, _vSfx;
    private Toggle _fullscreen;
    private Button _langEs, _langEn;

    private float _floatTime;

    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _btnPlay     = _root.Q<Button>("btn-play");
        _btnContinue = _root.Q<Button>("btn-continue");
        _btnSettings = _root.Q<Button>("btn-settings");

        _settingsOverlay = _root.Q<VisualElement>("settings-overlay");
        _btnClose = _root.Q<Button>("btn-close-settings");
        _sMaster  = _root.Q<Slider>("slider-master");
        _sMusic   = _root.Q<Slider>("slider-music");
        _sSfx     = _root.Q<Slider>("slider-sfx");
        _vMaster  = _root.Q<Label>("val-master");
        _vMusic   = _root.Q<Label>("val-music");
        _vSfx     = _root.Q<Label>("val-sfx");
        _fullscreen = _root.Q<Toggle>("toggle-fullscreen");
        _langEs   = _root.Q<Button>("lang-es");
        _langEn   = _root.Q<Button>("lang-en");
        _angelEl  = _root.Q<VisualElement>("angel");

        RegisterCallbacks();
        LoadSettings();
        RefreshContinueButton();
    }

    private void Update()
    {
        if (_angelEl == null) return;
        _floatTime += Time.deltaTime;
        float offset = Mathf.Sin(_floatTime * 1.1f) * 10f;
        _angelEl.style.translate = new StyleTranslate(new Translate(0, offset, 0));
    }

    private void RegisterCallbacks()
    {
        _btnPlay.clicked     += OnPlay;
        _btnContinue.clicked += OnContinue;
        _btnSettings.clicked += OpenSettings;
        _btnClose.clicked    += CloseSettings;

        VisualElement backdrop = _root.Q<VisualElement>("settings-backdrop");
        if (backdrop != null)
            backdrop.RegisterCallback<ClickEvent>(_ => CloseSettings());

        _sMaster.RegisterValueChangedCallback(e => OnVolume(KEY_MASTER, e.newValue, _vMaster));
        _sMusic.RegisterValueChangedCallback(e  => OnVolume(KEY_MUSIC,  e.newValue, _vMusic));
        _sSfx.RegisterValueChangedCallback(e    => OnVolume(KEY_SFX,    e.newValue, _vSfx));
        _fullscreen.RegisterValueChangedCallback(OnFullscreen);

        _langEs.clicked += () => SetLanguage("es");
        _langEn.clicked += () => SetLanguage("en");
    }

    private void OnPlay()
    {
        PlayerPrefs.SetInt(KEY_SAVE_EXISTS, 1);
        PlayerPrefs.SetInt(KEY_SAVED_LEVEL, 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene(newGameScene);
    }

    private void OnContinue()
    {
        int level = PlayerPrefs.GetInt(KEY_SAVED_LEVEL, 1);
        SceneManager.LoadScene("MecanicaPulpo"); // Aquí podrías usar el número de nivel para cargar diferentes escenas si lo deseas.
    }

    private void RefreshContinueButton()
    {
        bool hasSave = PlayerPrefs.GetInt(KEY_SAVE_EXISTS, 0) == 1;
        if (hasSave) _btnContinue.RemoveFromClassList("hidden");
        else         _btnContinue.AddToClassList("hidden");
    }

    private void OpenSettings()  => _settingsOverlay.RemoveFromClassList("hidden");
    private void CloseSettings() => _settingsOverlay.AddToClassList("hidden");

    private void OnVolume(string key, float value, Label lbl)
    {
        lbl.text = Mathf.RoundToInt(value) + "%";
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 80f) / 100f;
        float music  = PlayerPrefs.GetFloat(KEY_MUSIC,  70f) / 100f;
        if (musicSource != null) musicSource.volume = master * music;
    }

    private void OnFullscreen(ChangeEvent<bool> e)
    {
        Screen.fullScreen = e.newValue;
        PlayerPrefs.SetInt(KEY_FULLSCREEN, e.newValue ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SetLanguage(string lang)
    {
        PlayerPrefs.SetString(KEY_LANG, lang);
        PlayerPrefs.Save();
        if (lang == "es")
        {
            _langEs.AddToClassList("lang-active");
            _langEn.RemoveFromClassList("lang-active");
        }
        else
        {
            _langEn.AddToClassList("lang-active");
            _langEs.RemoveFromClassList("lang-active");
        }
    }

    private void LoadSettings()
    {
        float master = PlayerPrefs.GetFloat(KEY_MASTER, 80f);
        float music  = PlayerPrefs.GetFloat(KEY_MUSIC,  70f);
        float sfx    = PlayerPrefs.GetFloat(KEY_SFX,    60f);

        _sMaster.SetValueWithoutNotify(master);
        _sMusic.SetValueWithoutNotify(music);
        _sSfx.SetValueWithoutNotify(sfx);
        _vMaster.text = Mathf.RoundToInt(master) + "%";
        _vMusic.text  = Mathf.RoundToInt(music)  + "%";
        _vSfx.text    = Mathf.RoundToInt(sfx)    + "%";

        bool fs = PlayerPrefs.GetInt(KEY_FULLSCREEN, 0) == 1;
        _fullscreen.SetValueWithoutNotify(fs);

        SetLanguage(PlayerPrefs.GetString(KEY_LANG, "es"));
        ApplyVolumes();
    }
}
