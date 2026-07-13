using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string newGameScene = "Escena_Cinematica";

    [Header("Audio (opcional)")]
    [SerializeField] private AudioSource musicSource;

    [Header("Animación Babosa")]
    [SerializeField] private Sprite[] babosaFrames;
    [SerializeField] private float babosaFps = 12f;

    private const string KEY_SAVE_EXISTS = "SaveExists";
    private const string KEY_MASTER      = "MasterVolume";
    private const string KEY_MUSIC       = "MusicVolume";
    private const string KEY_SFX         = "SFXVolume";
    private const string KEY_LAST_SCENE  = "LastScene";

    private VisualElement _root;
    private VisualElement _settingsOverlay;
    private VisualElement _angelEl;
    private VisualElement _slugEl;
    private Button _btnPlay, _btnContinue, _btnNewGame, _btnSettings, _btnClose, _btnSave, _btnRewind;
    private Slider _sMaster, _sMusic, _sSfx;
    private Label _vMaster, _vMusic, _vSfx, _lblSaved;

    private float _floatTime;
    private bool _pendingNewGameConfirm;
    private int _babosaFrame;
    private float _babosaTimer;

    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _btnPlay     = _root.Q<Button>("btn-play");
        _btnContinue = _root.Q<Button>("btn-continue");
        _btnNewGame  = _root.Q<Button>("btn-new-game");
        _btnRewind   = _root.Q<Button>("btn-rewind");
        _btnSettings = _root.Q<Button>("btn-settings");

        _settingsOverlay = _root.Q<VisualElement>("settings-overlay");
        _btnClose = _root.Q<Button>("btn-close-settings");
        _sMaster  = _root.Q<Slider>("slider-master");
        _sMusic   = _root.Q<Slider>("slider-music");
        _sSfx     = _root.Q<Slider>("slider-sfx");
        _vMaster  = _root.Q<Label>("val-master");
        _vMusic   = _root.Q<Label>("val-music");
        _vSfx     = _root.Q<Label>("val-sfx");
        _angelEl  = _root.Q<VisualElement>("angel");
        _slugEl   = _root.Q<VisualElement>("slug");
        _btnSave  = _root.Q<Button>("btn-save-settings");
        _lblSaved = _root.Q<Label>("lbl-saved");

        RegisterCallbacks();
        LoadSettings();
        RefreshButtons();
    }

    private void Update()
    {
        AnimateAngel();
        AnimateBabosa();
    }

    private void AnimateAngel()
    {
        if (_angelEl == null) return;
        _floatTime += Time.deltaTime;
        float offset = Mathf.Sin(_floatTime * 1.1f) * 10f;
        _angelEl.style.translate = new StyleTranslate(new Translate(0, offset, 0));
    }

    private void AnimateBabosa()
    {
        if (_slugEl == null || babosaFrames == null || babosaFrames.Length == 0) return;
        _babosaTimer += Time.deltaTime;
        if (_babosaTimer < 1f / babosaFps) return;
        _babosaTimer = 0f;
        _babosaFrame = (_babosaFrame + 1) % babosaFrames.Length;
        _slugEl.style.backgroundImage = new StyleBackground(babosaFrames[_babosaFrame]);
    }

    private void RegisterCallbacks()
    {
        _btnPlay.clicked     += OnPlay;
        _btnContinue.clicked += OnContinue;
        _btnNewGame.clicked  += OnNewGame;
        _btnRewind?.RegisterCallback<ClickEvent>(_ => OnRewind());
        _btnSettings.clicked += OpenSettings;
        _btnClose.clicked    += CloseSettings;
        _btnSave.clicked     += SaveSettings;

        _root.Q<VisualElement>("settings-backdrop")
            ?.RegisterCallback<ClickEvent>(_ => CloseSettings());

        _sMaster.RegisterValueChangedCallback(e => OnVolume(KEY_MASTER, e.newValue, _vMaster));
        _sMusic.RegisterValueChangedCallback(e  => OnVolume(KEY_MUSIC,  e.newValue, _vMusic));
        _sSfx.RegisterValueChangedCallback(e    => OnVolume(KEY_SFX,    e.newValue, _vSfx));
    }

    private void RefreshButtons()
    {
        bool hasSave       = PlayerPrefs.GetInt(KEY_SAVE_EXISTS, 0) == 1;
        bool hasCheckpoint = PlayerPrefs.HasKey("CheckpointX") && PlayerPrefs.HasKey("CheckpointY");

        SetVisible(_btnPlay,     !hasSave);
        SetVisible(_btnContinue, hasSave);
        SetVisible(_btnNewGame,  hasSave);
        if (_btnRewind != null) SetVisible(_btnRewind, hasSave && hasCheckpoint);
    }

    private static void SetVisible(Button btn, bool visible)
    {
        if (visible) btn.RemoveFromClassList("hidden");
        else         btn.AddToClassList("hidden");
    }

    private void OnRewind()
    {
        // Borrar HasExitPos para que GameManager use CheckpointX/Y en lugar de la posición de salida
        PlayerPrefs.DeleteKey("HasExitPos");
        PlayerPrefs.Save();
        string scene = PlayerPrefs.GetString(KEY_LAST_SCENE, newGameScene);
        SceneManager.LoadScene(scene);
    }

    private void OnPlay()
    {
        PlayerPrefs.SetInt(KEY_SAVE_EXISTS, 1);
        PlayerPrefs.SetString(KEY_LAST_SCENE, newGameScene);
        PlayerPrefs.DeleteKey("CheckpointX");
        PlayerPrefs.DeleteKey("CheckpointY");
        PlayerPrefs.DeleteKey("HasExitPos");
        PlayerPrefs.Save();
        SceneManager.LoadScene(newGameScene);
    }

    private void OnContinue()
    {
        string scene = PlayerPrefs.GetString(KEY_LAST_SCENE, newGameScene);
        SceneManager.LoadScene(scene);
    }

    private void OnNewGame()
    {
        if (!_pendingNewGameConfirm)
        {
            _pendingNewGameConfirm = true;
            _btnNewGame.text = "¿confirmar?";
            _btnNewGame.AddToClassList("btn-confirm");
            _root.schedule.Execute(ResetNewGameButton).StartingIn(3000);
        }
        else
        {
            PlayerPrefs.DeleteKey(KEY_SAVE_EXISTS);
            PlayerPrefs.DeleteKey(KEY_LAST_SCENE);
            PlayerPrefs.DeleteKey("CheckpointX");
            PlayerPrefs.DeleteKey("CheckpointY");
            PlayerPrefs.DeleteKey("HasExitPos");
            PlayerPrefs.Save();
            SceneManager.LoadScene(newGameScene);
        }
    }

    private void ResetNewGameButton()
    {
        if (!_pendingNewGameConfirm) return;
        _pendingNewGameConfirm = false;
        _btnNewGame.text = "nueva partida";
        _btnNewGame.RemoveFromClassList("btn-confirm");
    }

    private void OpenSettings()  => _settingsOverlay.RemoveFromClassList("hidden");
    private void CloseSettings() => _settingsOverlay.AddToClassList("hidden");

    private void SaveSettings()
    {
        PlayerPrefs.Save();
        ApplyVolumes();
        if (_lblSaved == null) return;
        _lblSaved.text = "✓ guardado";
        var lbl = _lblSaved;
        _root.schedule.Execute(() => lbl.text = "").StartingIn(2000);
    }

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
        AudioListener.volume = master;
        if (musicSource != null) musicSource.volume = music;
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

        ApplyVolumes();
    }
}
