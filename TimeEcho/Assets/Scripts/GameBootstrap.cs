using UnityEngine;

// Sahneye hicbir sey eklemeye gerek yok: Play'e basildiginda oyunu sifirdan kurar.
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (Object.FindFirstObjectByType<LoopManager>() != null) return;
        var go = new GameObject("TimeEchoGame");
        go.AddComponent<LoopManager>();
    }
}
