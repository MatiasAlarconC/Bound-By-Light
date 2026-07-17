using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupBabosaAnimacion
{
    const string CONTROLLER_PATH = "Assets/Animations/0001_0.controller";
    const string CLIP_PATH       = "Assets/Animations/Babosa_Mantaraya.anim";
    const string SPRITES_FOLDER  = "Assets/Characters/DarkBrother/babosa_mantaraya";
    const string PARAM_NAME      = "estaConectadaMantarraya";
    const string STATE_NAME      = "Babosa_Mantaraya";
    const string IDLE_STATE      = "Babosa_Idle";

    [MenuItem("Tools/Setup Animación Babosa-Mantarraya")]
    public static void Setup()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER_PATH);
        if (controller == null) { Debug.LogError($"No se encontró el controller en {CONTROLLER_PATH}"); return; }

        AnimatorStateMachine sm = controller.layers[0].stateMachine;

        // Eliminar estado previo si existe para reconstruirlo limpio
        foreach (var s in sm.states)
        {
            if (s.state.name == STATE_NAME)
            {
                sm.RemoveState(s.state);
                break;
            }
        }

        // Borrar clip previo
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(CLIP_PATH) != null)
            AssetDatabase.DeleteAsset(CLIP_PATH);

        // Cargar sprites ordenados por nombre
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { SPRITES_FOLDER });
        string[] paths = System.Array.ConvertAll(guids, AssetDatabase.GUIDToAssetPath);
        System.Array.Sort(paths);

        if (paths.Length == 0) { Debug.LogError("No se encontraron sprites en " + SPRITES_FOLDER); return; }

        Sprite[] sprites = System.Array.ConvertAll(paths, p => AssetDatabase.LoadAssetAtPath<Sprite>(p));

        // Crear AnimationClip con las sprites
        AnimationClip clip = new AnimationClip();
        clip.frameRate = 12f;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe { time = i / clip.frameRate, value = sprites[i] };
        }
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, CLIP_PATH);
        AssetDatabase.SaveAssets();

        // Asegurarse de que el parámetro existe
        bool paramExists = false;
        foreach (var p in controller.parameters)
            if (p.name == PARAM_NAME) { paramExists = true; break; }
        if (!paramExists)
            controller.AddParameter(PARAM_NAME, AnimatorControllerParameterType.Bool);

        // Añadir estado Babosa_Mantaraya
        AnimatorState mantarayaState = sm.AddState(STATE_NAME, new Vector3(600f, 300f, 0f));
        mantarayaState.motion = clip;

        // Encontrar Babosa_Idle
        AnimatorState idleState = null;
        foreach (var s in sm.states)
            if (s.state.name == IDLE_STATE) { idleState = s.state; break; }

        if (idleState == null) { Debug.LogError(IDLE_STATE + " no encontrado en el controller."); return; }

        // AnyState → Babosa_Mantaraya (cuando estaConectadaMantarraya = true)
        AnimatorStateTransition toManta = sm.AddAnyStateTransition(mantarayaState);
        toManta.hasExitTime          = false;
        toManta.duration             = 0f;
        toManta.canTransitionToSelf  = false;
        toManta.AddCondition(AnimatorConditionMode.If, 0f, PARAM_NAME);

        // Babosa_Mantaraya → Babosa_Idle (cuando estaConectadaMantarraya = false)
        AnimatorStateTransition toIdle = mantarayaState.AddTransition(idleState);
        toIdle.hasExitTime = false;
        toIdle.duration    = 0f;
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, PARAM_NAME);

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=lime>Animación '{STATE_NAME}' añadida al controller con {sprites.Length} frames a {clip.frameRate} FPS.</color>");
    }
}
