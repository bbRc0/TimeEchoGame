using UnityEngine;

// Tum ses efektleri kodla sentezlenir (asset yok). Basit osilator + zarf.
// Ayarlar PlayerPrefs'te saklanir.
public static class Sfx
{
    const int SR = 44100;

    enum Wave { Sine, Square, Saw, Triangle, Noise }

    static AudioSource src;
    static bool inited;

    static float master = 0.8f;
    static float sfxVol = 0.9f;

    public static float Master
    {
        get { return master; }
        set { master = Mathf.Clamp01(value); PlayerPrefs.SetFloat("te_master", master); }
    }
    public static float Volume
    {
        get { return sfxVol; }
        set { sfxVol = Mathf.Clamp01(value); PlayerPrefs.SetFloat("te_sfx", sfxVol); }
    }

    // Onbellege alinan klipler
    static AudioClip jump, land, step, button, death, ghostVanish, levelClear, win, ui;

    // Sesleri ve ayarlari erkenden hazirlar (ornegin oyun basinda).
    public static void Prime() => Init();

    static void Init()
    {
        if (inited) return;
        inited = true;

        master = PlayerPrefs.GetFloat("te_master", 0.8f);
        sfxVol = PlayerPrefs.GetFloat("te_sfx", 0.9f);

        var go = new GameObject("SfxPlayer");
        Object.DontDestroyOnLoad(go);
        src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.spatialBlend = 0f;

        if (Object.FindAnyObjectByType<AudioListener>() == null)
            go.AddComponent<AudioListener>();

        jump = Tone("jump", 0.14f, 380f, 760f, Wave.Square, 0.35f, 0.004f, 0.06f);
        land = Tone("land", 0.07f, 200f, 120f, Wave.Noise, 0.25f, 0.001f, 0.05f);
        step = Tone("step", 0.045f, 160f, 110f, Wave.Noise, 0.16f, 0.001f, 0.03f);
        button = Mix("button",
            Tone(null, 0.08f, 620f, 940f, Wave.Square, 0.30f, 0.002f, 0.05f),
            Tone(null, 0.08f, 310f, 470f, Wave.Square, 0.16f, 0.002f, 0.05f));
        death = Mix("death",
            Tone(null, 0.45f, 480f, 70f, Wave.Saw, 0.40f, 0.004f, 0.20f),
            Tone(null, 0.45f, 240f, 50f, Wave.Noise, 0.18f, 0.004f, 0.25f));
        // Hayalet kayboluş: oyuncu ölümünden farkli, yumusak ve eterik iniş.
        ghostVanish = Mix("ghostVanish",
            Tone(null, 0.40f, 1000f, 520f, Wave.Sine, 0.30f, 0.01f, 0.30f),
            Tone(null, 0.40f, 1480f, 790f, Wave.Triangle, 0.15f, 0.01f, 0.30f));
        levelClear = Arp("levelClear", new[] { 523f, 659f, 784f, 1047f }, 0.10f, Wave.Square, 0.32f);
        win = Mix("win",
            Arp(null, new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.13f, Wave.Square, 0.30f),
            Arp(null, new[] { 262f, 330f, 392f, 523f, 659f }, 0.13f, Wave.Triangle, 0.20f));
        ui = Tone("ui", 0.05f, 720f, 820f, Wave.Square, 0.25f, 0.002f, 0.03f);
    }

    static void Play(AudioClip clip, float scale = 1f)
    {
        Init();
        if (clip == null) return;
        src.PlayOneShot(clip, Mathf.Clamp01(master * sfxVol * scale));
    }

    public static void Jump() => Play(jump);
    public static void Land() => Play(land, 0.8f);
    public static void Step() => Play(step, Random.Range(0.7f, 1f));
    public static void Button() => Play(button);
    public static void Death() => Play(death);
    public static void GhostVanish() => Play(ghostVanish, 0.8f);
    public static void LevelClear() => Play(levelClear);
    public static void Win() => Play(win);
    public static void Ui() => Play(ui);

    // ---- sentez ----

    static AudioClip Tone(string name, float dur, float f0, float f1, Wave wf,
        float vol, float attack, float release)
    {
        int n = Mathf.Max(1, (int)(dur * SR));
        var data = new float[n];
        float phase = 0f;
        var rng = new System.Random(1337);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float prog = (float)i / n;
            float freq = Mathf.Lerp(f0, f1, prog);
            phase += 2f * Mathf.PI * freq / SR;
            float ph = Mathf.Repeat(phase, 2f * Mathf.PI);
            float s;
            switch (wf)
            {
                case Wave.Square: s = Mathf.Sin(phase) >= 0f ? 1f : -1f; break;
                case Wave.Saw: s = ph / Mathf.PI - 1f; break;
                case Wave.Triangle: s = 1f - 2f * Mathf.Abs(ph / Mathf.PI - 1f); break;
                case Wave.Noise: s = (float)(rng.NextDouble() * 2.0 - 1.0); break;
                default: s = Mathf.Sin(phase); break;
            }
            float env = 1f;
            if (t < attack) env = t / attack;
            else if (t > dur - release) env = Mathf.Max(0f, (dur - t) / release);
            data[i] = s * env * vol;
        }
        var clip = AudioClip.Create(name ?? "tone", n, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    // Iki klibi ust uste bindirir (ayni anda calar).
    static AudioClip Mix(string name, AudioClip a, AudioClip b)
    {
        int n = Mathf.Max(a.samples, b.samples);
        var da = new float[a.samples]; a.GetData(da, 0);
        var db = new float[b.samples]; b.GetData(db, 0);
        var outp = new float[n];
        for (int i = 0; i < n; i++)
        {
            float v = 0f;
            if (i < da.Length) v += da[i];
            if (i < db.Length) v += db[i];
            outp[i] = Mathf.Clamp(v, -1f, 1f);
        }
        var clip = AudioClip.Create(name, n, 1, SR, false);
        clip.SetData(outp, 0);
        return clip;
    }

    // Notalari arka arkaya dizer (arpej).
    static AudioClip Arp(string name, float[] freqs, float noteDur, Wave wf, float vol)
    {
        AudioClip acc = null;
        var parts = new System.Collections.Generic.List<float[]>();
        int total = 0;
        foreach (var f in freqs)
        {
            var c = Tone(null, noteDur, f, f, wf, vol, 0.006f, noteDur * 0.5f);
            var d = new float[c.samples]; c.GetData(d, 0);
            parts.Add(d);
            total += d.Length;
        }
        var outp = new float[total];
        int off = 0;
        foreach (var p in parts) { System.Array.Copy(p, 0, outp, off, p.Length); off += p.Length; }
        acc = AudioClip.Create(name ?? "arp", total, 1, SR, false);
        acc.SetData(outp, 0);
        return acc;
    }
}
