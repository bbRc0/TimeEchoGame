// Bolum haritalari (ASCII):
//   #  duvar / zemin
//   P  oyuncu dogus noktasi
//   o  buton (basili plaka) - bolumdeki TUM butonlar basiliyken kapilar acilir
//   D  kapi (ayni sutundaki bitisik D'ler tek kapi olur)
//   E  cikis
public static class LevelMaps
{
    public static readonly string[][] Maps =
    {
        // Bolum 1: tek buton, tek kapi. Ilk turda butonun ustunde ol;
        // ikinci turda hayaletin basili tutarken kapidan gec.
        // Kapi neredeyse tavana kadar uzun => ustunden ziplanamaz.
        new[]
        {
            "########################",
            "#                      #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "# P       o         D E#",
            "########################",
        },

        // Bolum 2: iki buton, tek kapi. Iki turda iki ayri butonda ol;
        // ucuncu turda iki hayalet birden basarken gec.
        new[]
        {
            "########################",
            "#                      #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#                   D  #",
            "#   o               D  #",
            "#  ####             D  #",
            "# P        o        D E#",
            "########################",
        },

        // Bolum 3: cikis tek ziplamayla ulasilamayacak kadar yuksekte.
        // Ilk turda duvarin dibinde bekleyerek ol; sonra hayaletin
        // omuzlarina basip tirman.
        new[]
        {
            "########################",
            "#                      #",
            "#                      #",
            "#                      #",
            "#                      #",
            "#                      #",
            "#                      #",
            "#                E     #",
            "#              #########",
            "#              #########",
            "# P            #########",
            "########################",
        },
    };

    // Bolum basina hayalet hakki. Bu kadar olumden sonra bolumu bitiremezsen
    // bolum basarisiz sayilir ve R ile bastan baslaman gerekir.
    public static readonly int[] GhostBudgets = { 1, 2, 1 };

    public static readonly string[] Hints =
    {
        "Kapı ancak butona basılıyken açık kalıyor.\nBu turda butona git ve dalga gelene kadar üstünde dur.\nÖldüğün yer, hayaletinin görevi olur.",
        "İki buton, tek kapı.\nHer turda bir butonun üstünde öl —\nüçüncü turda iki hayalet de senin için basacak.",
        "Çıkış tek zıplamayla çıkılamayacak kadar yüksek.\nBu turda duvarın dibine git ve orada öl —\nsonra hayaletinin omuzlarına basıp tırman.",
    };
}
