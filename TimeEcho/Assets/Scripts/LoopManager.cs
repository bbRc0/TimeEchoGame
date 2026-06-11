using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Oyunun kalbi: tur dongusu. Tur "R" ile degil, OLUMLE biter.
// Olum aninda o turun hareket kaydi saklanir; sonraki turda bu kayit
// bir hayalet olarak geri oynatilir.
public class LoopManager : MonoBehaviour
{
    public static LoopManager I { get; private set; }

    public enum Phase { MainMenu, Playing, Dying, LevelComplete, AllDone, Failed }

    public Phase CurrentPhase { get; private set; }
    public bool IsPlaying => CurrentPhase == Phase.Playing;

    public int Frame { get; private set; }                 // tur basindan beri fizik karesi
    public float LoopTime => Frame * Time.fixedDeltaTime;
    public PlayerController Player { get; private set; }

    readonly List<List<Vector2>> recordings = new List<List<Vector2>>();
    readonly List<GhostReplay> ghosts = new List<GhostReplay>();

    int levelIndex;
    int loopNumber = 1;
    GameObject levelRoot;
    LevelRefs refs;
    float phaseTimer;
    string deathMsg = "";
    bool winSoundPlayed;

    public bool IsPaused { get; private set; }
    float confettiTimer;
    bool menuSettingsOpen;

    void Awake()
    {
        I = this;
    }

    void Start()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var cgo = new GameObject("Main Camera");
            cgo.tag = "MainCamera";
            cam = cgo.AddComponent<Camera>();
        }
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.065f, 0.10f);

        Sfx.Prime();

        // Ilk acilista oyunu baslatma; once ana menu gosterilir.
        FrameCamera(LevelMaps.Maps[0][0].Length, LevelMaps.Maps[0].Length);
        CurrentPhase = Phase.MainMenu;
    }

    void LoadLevel(int index)
    {
        SetPaused(false);
        menuSettingsOpen = false;
        levelIndex = index;
        recordings.Clear();
        loopNumber = 1;
        deathMsg = "";
        winSoundPlayed = false;

        if (levelRoot != null) Destroy(levelRoot);
        if (Player != null) Destroy(Player.gameObject);
        ghosts.Clear();

        levelRoot = new GameObject("Level_" + (index + 1));
        refs = LevelBuilder.Build(LevelMaps.Maps[index], levelRoot.transform);
        Player = PlayerController.Spawn(refs.Spawn, null);

        FrameCamera(refs.Width, refs.Height);
        StartLoop();
    }

    void FrameCamera(float w, float h)
    {
        var cam = Camera.main;
        cam.transform.position = new Vector3(w * 0.5f, h * 0.5f, -10f);
        cam.orthographicSize = Mathf.Max(h * 0.5f + 0.5f, (w * 0.5f + 0.5f) / cam.aspect);
    }

    void StartLoop()
    {
        Frame = 0;

        foreach (var g in ghosts)
            if (g != null) Destroy(g.gameObject);
        ghosts.Clear();
        foreach (var rec in recordings)
            ghosts.Add(GhostReplay.Spawn(rec, levelRoot.transform));

        Player.ResetTo(refs.Spawn);
        CurrentPhase = Phase.Playing;
    }

    void FixedUpdate()
    {
        if (IsPlaying) Frame++;
    }

    void Update()
    {
        // Ana menude oyun mantigi calismaz; menu OnGUI'de cizilir.
        if (CurrentPhase == Phase.MainMenu) return;

        var kb = Keyboard.current;

        // ESC: duraklat / devam et (bitis ekraninda kapali).
        if (kb != null && kb.escapeKey.wasPressedThisFrame && CurrentPhase != Phase.AllDone)
        {
            SetPaused(!IsPaused);
            Sfx.Ui();
        }

        if (IsPaused) return;

        if (kb != null && kb.rKey.wasPressedThisFrame)
        {
            // R artik turu degil, BOLUMU bastan baslatir (tum hayaletler silinir).
            LoadLevel(CurrentPhase == Phase.AllDone ? 0 : levelIndex);
            return;
        }

        if (CurrentPhase == Phase.Dying)
        {
            phaseTimer -= Time.deltaTime;
            if (phaseTimer <= 0f)
            {
                loopNumber++;
                StartLoop();
            }
        }
        else if (CurrentPhase == Phase.LevelComplete)
        {
            phaseTimer -= Time.deltaTime;
            if (phaseTimer <= 0f)
            {
                if (levelIndex + 1 < LevelMaps.Maps.Length) LoadLevel(levelIndex + 1);
                else CurrentPhase = Phase.AllDone;
            }
        }
        else if (CurrentPhase == Phase.AllDone)
        {
            if (!winSoundPlayed) { Sfx.Win(); winSoundPlayed = true; }

            // Bitis ekraninda surekli konfeti
            confettiTimer -= Time.unscaledDeltaTime;
            if (confettiTimer <= 0f && refs != null)
            {
                confettiTimer = 0.25f;
                var cols = new[]
                {
                    new Color(1f, 0.85f, 0.3f), new Color(0.42f, 0.88f, 1f),
                    new Color(0.95f, 0.4f, 0.5f), new Color(0.45f, 0.95f, 0.55f),
                };
                for (int k = 0; k < 2; k++)
                {
                    float x = Random.Range(1f, refs.Width - 1f);
                    Fx.Burst(new Vector2(x, refs.Height + 0.5f), cols[Random.Range(0, cols.Length)], 6);
                }
            }
        }
    }

    void SetPaused(bool p)
    {
        IsPaused = p;
        Time.timeScale = p ? 0f : 1f;
    }

    // Tek olum girisi: dalga, ezilme vb. hepsi buradan gecer.
    public void PlayerDied(string message)
    {
        if (!IsPlaying) return;

        Fx.Burst(Player.transform.position, new Color(1f, 0.71f, 0.28f));
        Sfx.Death();
        Player.gameObject.SetActive(false);
        deathMsg = message;

        int budget = LevelMaps.GhostBudgets[levelIndex];

        // Hayalet hakki dolduysa bu olum yeni bir hayalet uretmez: bolum basarisiz.
        if (recordings.Count >= budget)
        {
            CurrentPhase = Phase.Failed;
            return;
        }

        recordings.Add(new List<Vector2>(Player.Recording));
        CurrentPhase = Phase.Dying;
        phaseTimer = 1.5f;
    }

    public void ExitReached()
    {
        if (!IsPlaying) return;
        Fx.Burst(Player.transform.position, new Color(0.35f, 0.95f, 0.55f), 20);
        Sfx.LevelClear();
        Player.gameObject.SetActive(false);
        CurrentPhase = Phase.LevelComplete;
        phaseTimer = 2.2f;
    }

    // ----- HUD -----

    GUIStyle MakeStyle(int size, Color c, TextAnchor anchor, FontStyle fs = FontStyle.Bold)
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            fontStyle = fs,
            alignment = anchor,
            normal = { textColor = c }
        };
    }

    void Shadowed(Rect r, string text, GUIStyle style)
    {
        var c = style.normal.textColor;
        style.normal.textColor = new Color(0f, 0f, 0f, 0.8f);
        GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), text, style);
        style.normal.textColor = c;
        GUI.Label(r, text, style);
    }

    static Texture2D solidTex;
    static Texture2D Solid
    {
        get
        {
            if (solidTex == null)
            {
                solidTex = new Texture2D(1, 1);
                solidTex.SetPixel(0, 0, Color.white);
                solidTex.Apply();
            }
            return solidTex;
        }
    }

    void Fill(Rect r, Color c)
    {
        var prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, Solid);
        GUI.color = prev;
    }

    // Basit, tema renkli buton. Tiklaninca UI sesi calar.
    bool Btn(Rect r, string label, int fontSize, Color accent)
    {
        bool hover = r.Contains(Event.current.mousePosition);
        Fill(r, hover ? accent : new Color(accent.r, accent.g, accent.b, 0.55f));
        Fill(new Rect(r.x, r.y, r.width, 2f), new Color(1f, 1f, 1f, 0.25f));
        var style = MakeStyle(fontSize, Color.white, TextAnchor.MiddleCenter);
        GUI.Label(r, label, style);
        if (GUI.Button(r, GUIContent.none, GUIStyle.none))
        {
            Sfx.Ui();
            return true;
        }
        return false;
    }

    void OnGUI()
    {
        float s = Screen.height / 720f;
        int big = Mathf.RoundToInt(34 * s);
        int mid = Mathf.RoundToInt(22 * s);
        int small = Mathf.RoundToInt(17 * s);

        // Ana menu: sadece menuyu ciz, oyun HUD'unu atla.
        if (CurrentPhase == Phase.MainMenu)
        {
            DrawMainMenu(s);
            return;
        }

        // Sol ust: durum (hayalet kullanilan/hak)
        int budget = LevelMaps.GhostBudgets[levelIndex];
        Shadowed(new Rect(16 * s, 10 * s, 700 * s, 40 * s),
            $"Bölüm {levelIndex + 1}/{LevelMaps.Maps.Length}    Tur {loopNumber}    Hayalet: {recordings.Count}/{budget}",
            MakeStyle(mid, Color.white, TextAnchor.UpperLeft));

        // Sag ust: dalga sayaci
        if (IsPlaying && refs != null && refs.Wave != null)
        {
            float t = refs.Wave.TimeUntilStart;
            string txt = t > 0f ? $"Dalga: {t:0.0} sn" : "DALGA GELİYOR!";
            var col = t > 3f ? new Color(0.8f, 0.85f, 1f) : new Color(1f, 0.35f, 0.45f);
            Shadowed(new Rect(Screen.width - 416 * s, 10 * s, 400 * s, 40 * s), txt,
                MakeStyle(mid, col, TextAnchor.UpperRight));
        }

        // Ilk turda bolum ipucu
        if (IsPlaying && loopNumber == 1 && LoopTime < 7f)
        {
            Shadowed(new Rect(0, 70 * s, Screen.width, 120 * s), LevelMaps.Hints[levelIndex],
                MakeStyle(small, new Color(1f, 1f, 1f, 0.85f), TextAnchor.UpperCenter, FontStyle.Normal));
        }

        // Orta mesajlar
        if (CurrentPhase == Phase.Dying)
        {
            Shadowed(new Rect(0, Screen.height * 0.38f, Screen.width, 60 * s), deathMsg,
                MakeStyle(big, new Color(1f, 0.45f, 0.5f), TextAnchor.MiddleCenter));
            Shadowed(new Rect(0, Screen.height * 0.38f + 50 * s, Screen.width, 40 * s),
                $"Hayalet #{recordings.Count} doğdu ({recordings.Count}/{budget}) — tur yeniden başlıyor...",
                MakeStyle(mid, new Color(0.42f, 0.88f, 1f), TextAnchor.MiddleCenter));
        }
        else if (CurrentPhase == Phase.Failed)
        {
            // Once nasil olundugu (kapiya sikisma, dalga vb.), sonra hak bitti uyarisi.
            Shadowed(new Rect(0, Screen.height * 0.32f, Screen.width, 60 * s), deathMsg,
                MakeStyle(big, new Color(1f, 0.45f, 0.5f), TextAnchor.MiddleCenter));
            Shadowed(new Rect(0, Screen.height * 0.32f + 56 * s, Screen.width, 50 * s), "Hayalet hakkın bitti!",
                MakeStyle(mid, new Color(1f, 0.6f, 0.35f), TextAnchor.MiddleCenter));
            Shadowed(new Rect(0, Screen.height * 0.32f + 100 * s, Screen.width, 40 * s),
                "Bölümü geçemedin — R'ye basıp baştan dene.",
                MakeStyle(mid, Color.white, TextAnchor.MiddleCenter));
        }
        else if (CurrentPhase == Phase.LevelComplete)
        {
            Shadowed(new Rect(0, Screen.height * 0.4f, Screen.width, 60 * s), "Bölüm tamamlandı!",
                MakeStyle(big, new Color(0.35f, 0.95f, 0.55f), TextAnchor.MiddleCenter));
        }
        // Alt: kontroller
        Shadowed(new Rect(0, Screen.height - 36 * s, Screen.width, 30 * s),
            "A/D veya ←/→ yürü     Boşluk zıpla     R bölümü sıfırla     ESC duraklat",
            MakeStyle(small, new Color(1f, 1f, 1f, 0.55f), TextAnchor.MiddleCenter, FontStyle.Normal));

        if (CurrentPhase == Phase.AllDone) DrawEndScreen(s);
        if (IsPaused) DrawPauseMenu(s);
    }

    // ----- Bitis ekrani -----
    void DrawEndScreen(float s)
    {
        // Hafif karartma: arkadaki konfeti gorunsun.
        Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0.03f, 0.04f, 0.09f, 0.40f));

        float pw = 560 * s, ph = 360 * s;
        var panel = new Rect((Screen.width - pw) * 0.5f, (Screen.height - ph) * 0.5f, pw, ph);
        Fill(panel, new Color(0.11f, 0.13f, 0.22f, 0.98f));
        Fill(new Rect(panel.x, panel.y, panel.width, 6 * s), new Color(0.35f, 0.95f, 0.55f, 1f));

        // Parlama efekti
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 3f);
        var gold = Color.Lerp(new Color(1f, 0.8f, 0.25f), new Color(1f, 0.95f, 0.6f), pulse);

        Shadowed(new Rect(panel.x, panel.y + 40 * s, panel.width, 70 * s), "KAZANDIN!",
            MakeStyle(Mathf.RoundToInt(56 * s), gold, TextAnchor.MiddleCenter));
        Shadowed(new Rect(panel.x, panel.y + 120 * s, panel.width, 40 * s),
            "Zamanın yankılarıyla üç bölümü de aştın.",
            MakeStyle(Mathf.RoundToInt(20 * s), Color.white, TextAnchor.MiddleCenter, FontStyle.Normal));
        Shadowed(new Rect(panel.x, panel.y + 156 * s, panel.width, 36 * s),
            "Geçmişteki sen, şimdiki sana yardım etti.",
            MakeStyle(Mathf.RoundToInt(16 * s), new Color(0.42f, 0.88f, 1f), TextAnchor.MiddleCenter, FontStyle.Normal));

        float bw = 220 * s, bh = 48 * s, gap = 16 * s;
        float bx = panel.x + (panel.width - bw) * 0.5f;
        float by = panel.y + ph - bh * 2 - gap - 28 * s;

        if (Btn(new Rect(bx, by, bw, bh), "Baştan Oyna", Mathf.RoundToInt(22 * s), new Color(0.2f, 0.6f, 0.35f)))
            LoadLevel(0);
        if (Btn(new Rect(bx, by + bh + gap, bw, bh), "Çıkış", Mathf.RoundToInt(22 * s), new Color(0.45f, 0.2f, 0.28f)))
            Quit();
    }

    // Ses + tam ekran kontrollerini cizer; son kontrolden sonraki y'yi dondurur.
    // Hem duraklatma hem ana menu ayarlarinda ortak kullanilir.
    float DrawSettingsControls(float lx, float lw, float sy, float s)
    {
        var lbl = MakeStyle(Mathf.RoundToInt(17 * s), Color.white, TextAnchor.MiddleLeft, FontStyle.Normal);

        GUI.Label(new Rect(lx, sy, lw, 26 * s), $"Ana Ses: {(int)(Sfx.Master * 100)}%", lbl);
        float newMaster = GUI.HorizontalSlider(new Rect(lx, sy + 28 * s, lw, 20 * s), Sfx.Master, 0f, 1f);
        if (Mathf.Abs(newMaster - Sfx.Master) > 0.001f) Sfx.Master = newMaster;

        sy += 64 * s;
        GUI.Label(new Rect(lx, sy, lw, 26 * s), $"Efekt Sesi: {(int)(Sfx.Volume * 100)}%", lbl);
        float newSfx = GUI.HorizontalSlider(new Rect(lx, sy + 28 * s, lw, 20 * s), Sfx.Volume, 0f, 1f);
        if (Mathf.Abs(newSfx - Sfx.Volume) > 0.001f) Sfx.Volume = newSfx;

        // Tam ekran (elle cizilen onay kutusu)
        sy += 60 * s;
        bool fs = Screen.fullScreen;
        var box = new Rect(lx, sy, 28 * s, 28 * s);
        Fill(box, new Color(0f, 0f, 0f, 0.4f));
        Fill(new Rect(box.x, box.y, box.width, 2f), new Color(1f, 1f, 1f, 0.2f));
        if (fs)
            Fill(new Rect(box.x + 6 * s, box.y + 6 * s, box.width - 12 * s, box.height - 12 * s), new Color(0.42f, 0.88f, 1f));
        GUI.Label(new Rect(lx + 40 * s, sy, lw - 40 * s, 28 * s), "Tam Ekran", lbl);
        if (GUI.Button(new Rect(lx, sy, lw, 28 * s), GUIContent.none, GUIStyle.none))
        {
            Screen.fullScreen = !fs;
            Sfx.Ui();
        }

        return sy + 40 * s;
    }

    // ----- Duraklatma + Ayarlar -----
    void DrawPauseMenu(float s)
    {
        Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0.03f, 0.04f, 0.09f, 0.75f));

        float pw = 520 * s, ph = 470 * s;
        var panel = new Rect((Screen.width - pw) * 0.5f, (Screen.height - ph) * 0.5f, pw, ph);
        Fill(panel, new Color(0.11f, 0.13f, 0.22f, 0.98f));
        Fill(new Rect(panel.x, panel.y, panel.width, 6 * s), new Color(0.42f, 0.88f, 1f, 1f));

        Shadowed(new Rect(panel.x, panel.y + 26 * s, panel.width, 50 * s), "DURAKLATILDI",
            MakeStyle(Mathf.RoundToInt(34 * s), Color.white, TextAnchor.MiddleCenter));

        Shadowed(new Rect(panel.x, panel.y + 84 * s, panel.width, 30 * s), "AYARLAR",
            MakeStyle(Mathf.RoundToInt(18 * s), new Color(0.42f, 0.88f, 1f), TextAnchor.MiddleCenter));

        float lx = panel.x + 40 * s, lw = panel.width - 80 * s;
        DrawSettingsControls(lx, lw, panel.y + 124 * s, s);

        // Butonlar
        float bw = panel.width - 80 * s, bh = 46 * s, gap = 12 * s;
        float by = panel.y + ph - bh * 3 - gap * 2 - 20 * s;
        if (Btn(new Rect(lx, by, bw, bh), "Devam Et", Mathf.RoundToInt(22 * s), new Color(0.2f, 0.5f, 0.6f)))
            SetPaused(false);
        if (Btn(new Rect(lx, by + bh + gap, bw, bh), "Bölümü Yeniden Başlat", Mathf.RoundToInt(20 * s), new Color(0.45f, 0.4f, 0.2f)))
            LoadLevel(levelIndex);
        if (Btn(new Rect(lx, by + (bh + gap) * 2, bw, bh), "Çıkış", Mathf.RoundToInt(22 * s), new Color(0.45f, 0.2f, 0.28f)))
            Quit();
    }

    // ----- Ana Menu (acilis ekrani) -----
    void DrawMainMenu(float s)
    {
        // Koyu arka plan + suzulen "yanki" kareleri
        Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0.05f, 0.06f, 0.11f, 1f));
        for (int i = 0; i < 7; i++)
        {
            float seed = i * 1.37f;
            float t = (Time.unscaledTime * 0.06f + i * 0.15f) % 1f;
            float x = (Mathf.Sin(seed) * 0.5f + 0.5f) * Screen.width;
            float y = Screen.height * (1f - t);
            float sz = (28f + (i % 3) * 18f) * s;
            float a = 0.05f + 0.06f * Mathf.Sin(t * Mathf.PI);
            Fill(new Rect(x, y, sz, sz), new Color(0.42f, 0.88f, 1f, a));
        }

        // Baslik
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.2f);
        var titleCol = Color.Lerp(new Color(0.55f, 0.85f, 1f), new Color(1f, 1f, 1f), pulse);
        Shadowed(new Rect(0, Screen.height * 0.16f, Screen.width, 90 * s), "TIME ECHO",
            MakeStyle(Mathf.RoundToInt(72 * s), titleCol, TextAnchor.MiddleCenter));
        Shadowed(new Rect(0, Screen.height * 0.16f + 84 * s, Screen.width, 40 * s),
            "Zaman Yankısı — geçmişteki sen, şimdiki sana yardım eder",
            MakeStyle(Mathf.RoundToInt(20 * s), new Color(0.42f, 0.88f, 1f), TextAnchor.MiddleCenter, FontStyle.Normal));

        if (menuSettingsOpen)
        {
            DrawMenuSettings(s);
            return;
        }

        // Butonlar
        float bw = 300 * s, bh = 56 * s, gap = 16 * s;
        float bx = (Screen.width - bw) * 0.5f;
        float by = Screen.height * 0.46f;
        if (Btn(new Rect(bx, by, bw, bh), "Başlat", Mathf.RoundToInt(26 * s), new Color(0.2f, 0.55f, 0.35f)))
            LoadLevel(0);
        if (Btn(new Rect(bx, by + bh + gap, bw, bh), "Ayarlar", Mathf.RoundToInt(26 * s), new Color(0.2f, 0.45f, 0.6f)))
            menuSettingsOpen = true;
        if (Btn(new Rect(bx, by + (bh + gap) * 2, bw, bh), "Çıkış", Mathf.RoundToInt(26 * s), new Color(0.45f, 0.2f, 0.28f)))
            Quit();

        Shadowed(new Rect(0, Screen.height - 40 * s, Screen.width, 30 * s),
            "A/D yürü   Boşluk zıpla   R bölümü sıfırla   ESC duraklat",
            MakeStyle(Mathf.RoundToInt(16 * s), new Color(1f, 1f, 1f, 0.5f), TextAnchor.MiddleCenter, FontStyle.Normal));
    }

    void DrawMenuSettings(float s)
    {
        float pw = 520 * s, ph = 360 * s;
        var panel = new Rect((Screen.width - pw) * 0.5f, Screen.height * 0.42f, pw, ph);
        Fill(panel, new Color(0.11f, 0.13f, 0.22f, 0.98f));
        Fill(new Rect(panel.x, panel.y, panel.width, 6 * s), new Color(0.42f, 0.88f, 1f, 1f));

        Shadowed(new Rect(panel.x, panel.y + 22 * s, panel.width, 36 * s), "AYARLAR",
            MakeStyle(Mathf.RoundToInt(26 * s), Color.white, TextAnchor.MiddleCenter));

        float lx = panel.x + 40 * s, lw = panel.width - 80 * s;
        DrawSettingsControls(lx, lw, panel.y + 80 * s, s);

        float bw = panel.width - 80 * s, bh = 50 * s;
        if (Btn(new Rect(lx, panel.y + ph - bh - 20 * s, bw, bh), "Geri", Mathf.RoundToInt(24 * s), new Color(0.3f, 0.35f, 0.45f)))
            menuSettingsOpen = false;
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
