using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public static class CrearAnimacionMantarraya
{
    const string RUTA_TRANSFORM_SPRITES = "Assets/Characters/LightBrother/StingrayTransformation";
    const string RUTA_IDLE_SPRITES      = "Assets/Characters/LightBrother/StingrayIdle";
    const string RUTA_ANIM_TRANSFORM    = "Assets/Animations/StingrayTransformation.anim";
    const string RUTA_ANIM_IDLE         = "Assets/Animations/StingrayIdle.anim";
    const string RUTA_CONTROLLER        = "Assets/Animations/0017_0.controller";
    const float  FPS_TRANSFORM           = 18f;
    const float  FPS_IDLE                = 18f;
    const float  SPEED_TRANSFORM         = 0.5f;

    [MenuItem("Tools/Crear Animación Mantarraya")]
    static void Crear()
    {
        // ── 1. Cargar sprites ──────────────────────────────────────────────────
        Sprite[] spritesTransform = CargarSprites(RUTA_TRANSFORM_SPRITES);
        Sprite[] spritesIdle      = CargarSprites(RUTA_IDLE_SPRITES);

        if (spritesTransform.Length == 0 || spritesIdle.Length == 0)
        {
            Debug.LogError("[Mantarraya] No se encontraron sprites. Revisa las rutas:\n" +
                           RUTA_TRANSFORM_SPRITES + "\n" + RUTA_IDLE_SPRITES);
            return;
        }

        // ── 2. Crear clips ─────────────────────────────────────────────────────
        AnimationClip clipTransform = CrearClipSprites("StingrayTransformation", spritesTransform, FPS_TRANSFORM, loop: false);
        AnimationClip clipIdle      = CrearClipSprites("StingrayIdle",           spritesIdle,      FPS_IDLE,      loop: true);

        GuardarClip(clipTransform, RUTA_ANIM_TRANSFORM);
        GuardarClip(clipIdle,      RUTA_ANIM_IDLE);

        // ── 3. Cargar controlador existente ────────────────────────────────────
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(RUTA_CONTROLLER);
        if (controller == null)
        {
            Debug.LogError("[Mantarraya] No se encontró el controlador en: " + RUTA_CONTROLLER);
            return;
        }

        var sm = controller.layers[0].stateMachine;

        // Si ya existen los estados, eliminarlos para recrear con configuración actualizada
        var estadosAEliminar = new System.Collections.Generic.List<AnimatorState>();
        foreach (var cs in sm.states)
            if (cs.state.name == "StingrayTransformation" || cs.state.name == "StingrayIdle")
                estadosAEliminar.Add(cs.state);
        foreach (var s in estadosAEliminar) sm.RemoveState(s);

        // ── 4. Agregar parámetro estaMontado ──────────────────────────────────
        bool yaExisteParam = false;
        foreach (var p in controller.parameters)
            if (p.name == "estaMontado") { yaExisteParam = true; break; }
        if (!yaExisteParam)
            controller.AddParameter("estaMontado", AnimatorControllerParameterType.Bool);

        // ── 5. Agregar estados ─────────────────────────────────────────────────
        var stateTransform = sm.AddState("StingrayTransformation", new Vector3(540, 350, 0));
        stateTransform.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_ANIM_TRANSFORM);
        stateTransform.speed  = SPEED_TRANSFORM;

        var stateIdle = sm.AddState("StingrayIdle", new Vector3(800, 350, 0));
        stateIdle.motion = AssetDatabase.LoadAssetAtPath<AnimationClip>(RUTA_ANIM_IDLE);

        // ── 6. Buscar BigBrother_Idle ──────────────────────────────────────────
        AnimatorState bigBrotherIdle = null;
        foreach (var cs in sm.states)
            if (cs.state.name == "BigBrother_Idle") { bigBrotherIdle = cs.state; break; }

        // ── 7. Transiciones ────────────────────────────────────────────────────
        // BigBrother_Idle → StingrayTransformation (cuando estaMontado = true)
        if (bigBrotherIdle != null)
        {
            var t1 = bigBrotherIdle.AddTransition(stateTransform);
            t1.AddCondition(AnimatorConditionMode.If, 0, "estaMontado");
            t1.hasExitTime       = false;
            t1.duration          = 0f;
            t1.hasFixedDuration  = true;
        }

        // StingrayTransformation → StingrayIdle (al terminar la animación)
        var t2 = stateTransform.AddTransition(stateIdle);
        t2.hasExitTime      = true;
        t2.exitTime         = 1.0f;
        t2.duration         = 0f;
        t2.hasFixedDuration = true;

        // StingrayIdle → BigBrother_Idle (cuando estaMontado = false)
        if (bigBrotherIdle != null)
        {
            var t3 = stateIdle.AddTransition(bigBrotherIdle);
            t3.AddCondition(AnimatorConditionMode.IfNot, 0, "estaMontado");
            t3.hasExitTime      = false;
            t3.duration         = 0f;
            t3.hasFixedDuration = true;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Mantarraya] ¡Listo! StingrayTransformation ({FPS_TRANSFORM}fps, speed={SPEED_TRANSFORM}) y StingrayIdle ({FPS_IDLE}fps) actualizados.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static Sprite[] CargarSprites(string carpeta)
    {
        var archivos = Directory.GetFiles(Application.dataPath + carpeta.Substring("Assets".Length), "*.png");
        System.Array.Sort(archivos);
        var sprites = new System.Collections.Generic.List<Sprite>();
        foreach (string ruta in archivos)
        {
            string assetPath = "Assets" + ruta.Replace(Application.dataPath, "").Replace('\\', '/');
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (s != null) sprites.Add(s);
        }
        return sprites.ToArray();
    }

    static AnimationClip CrearClipSprites(string nombre, Sprite[] sprites, float fps, bool loop)
    {
        AnimationClip clip = new AnimationClip();
        clip.frameRate = fps;

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type         = typeof(SpriteRenderer),
            path         = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[sprites.Length + 1];
        for (int i = 0; i < sprites.Length; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };
        }
        // Último frame igual al primero para evitar glitch de fin de clip
        keys[sprites.Length] = new ObjectReferenceKeyframe { time = sprites.Length / fps, value = sprites[sprites.Length - 1] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        if (loop)
        {
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        return clip;
    }

    static void GuardarClip(AnimationClip clip, string ruta)
    {
        AnimationClip existente = AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
        if (existente != null)
            EditorUtility.CopySerialized(clip, existente);
        else
            AssetDatabase.CreateAsset(clip, ruta);
    }
}
