using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class SetupStingrayDetransform
{
    const string CONTROLLER      = "Assets/Animations/0017_0.controller";
    const string FORWARD_CLIP    = "Assets/Animations/StingrayTransformation.anim";
    const string OLD_DETR_CLIP   = "Assets/Animations/StingrayDetransformation.anim";
    const string STATE_NAME      = "StingrayDetransformation";
    const string FROM_STATE      = "StingrayIdle";
    const string TO_STATE        = "BigBrother_Idle";
    const string PARAM           = "estaMontado";

    [MenuItem("Tools/Setup Stingray Detransformation (Speed -1)")]
    public static void Setup()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(CONTROLLER);
        if (controller == null) { Debug.LogError("Controller no encontrado: " + CONTROLLER); return; }

        // Borrar el clip invertido que creó la versión anterior (si existe)
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(OLD_DETR_CLIP) != null)
            AssetDatabase.DeleteAsset(OLD_DETR_CLIP);

        var forwardClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(FORWARD_CLIP);
        if (forwardClip == null) { Debug.LogError("No se encontró: " + FORWARD_CLIP); return; }

        var sm = controller.layers[0].stateMachine;

        // Eliminar estado previo si existe
        foreach (var s in sm.states)
            if (s.state.name == STATE_NAME) { sm.RemoveState(s.state); break; }

        // Buscar estados origen y destino
        AnimatorState fromState = null, toState = null;
        foreach (var s in sm.states)
        {
            if (s.state.name == FROM_STATE) fromState = s.state;
            if (s.state.name == TO_STATE)   toState   = s.state;
        }
        if (fromState == null) { Debug.LogError(FROM_STATE + " no encontrado."); return; }
        if (toState   == null) { Debug.LogError(TO_STATE   + " no encontrado."); return; }

        // Crear estado con el mismo clip en reversa (Speed = -1) — igual que BigBrother_Distransformation 0
        var detrState = sm.AddState(STATE_NAME, new Vector3(640f, 440f, 0f));
        detrState.motion = forwardClip;
        detrState.speed  = -1f;          // reproduce el clip al revés

        // StingrayIdle → StingrayDetransformation cuando estaMontado = false
        var toDetr = fromState.AddTransition(detrState);
        toDetr.hasExitTime = false;
        toDetr.duration    = 0f;
        toDetr.AddCondition(AnimatorConditionMode.IfNot, 0f, PARAM);

        // StingrayDetransformation → BigBrother_Idle al terminar (exitTime = 1, sin condiciones)
        var toIdle = detrState.AddTransition(toState);
        toIdle.hasExitTime      = true;
        toIdle.exitTime         = 1f;
        toIdle.duration         = 0f;
        toIdle.hasFixedDuration = true;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        Debug.Log("<color=lime>StingrayDetransformation: Speed=-1 sobre StingrayTransformation.anim — igual al método del Octopus.</color>");
    }
}
