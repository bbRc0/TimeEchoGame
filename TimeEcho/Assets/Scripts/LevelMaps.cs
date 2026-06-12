// Bolum haritalari (ASCII, 24x12):
//   #    duvar / zemin
//   P    oyuncu dogus noktasi
//   a-c  buton (basili plaka)
//   A-C  kapi — ayni harfin TUM butonlari basiliyken acilir (a -> A)
//   ^    diken (oyuncuyu oldurur)
//   L    lazer (periyodik yanar/soner, yanikken oldurur)
//   E    cikis kapisi
public static class LevelMaps
{
    public static readonly string[][] Maps =
    {
        // 1) Ogretici: tek buton, tek kapi.
        new[]
        {
            "########################",
            "#                      #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "# P       a         A E#",
            "########################",
        },

        // 2) Iki buton, tek kapi: ikisi de basilmali.
        new[]
        {
            "########################",
            "#                      #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#   a               A  #",
            "#  ####             A  #",
            "# P        a        A E#",
            "########################",
        },

        // 3) Diken tanitimi: cukurlarin uzerinden atla.
        new[]
        {
            "########################",
            "#                      #",
            "#                  A   #",
            "#                  A   #",
            "#                  A   #",
            "#                  A   #",
            "#                  A   #",
            "#                  A   #",
            "#                  A   #",
            "#                  A   #",
            "# P  ^^   a  ^^    A  E#",
            "########################",
        },

        // 4) Merdiven hayalet: cikis yuksekte, omuzlara bas.
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

        // 5) Lazer tanitimi: sondugu anda gec.
        new[]
        {
            "########################",
            "#                      #",
            "#                  A   #",
            "#                  A   #",
            "#                  A   #",
            "#      L           A   #",
            "#      L           A   #",
            "#      L           A   #",
            "#      L           A   #",
            "#      L           A   #",
            "# P    L     a     A  E#",
            "########################",
        },

        // 6) Iki ayri kapi: a -> A, b -> B. Basamak + platform + diken.
        new[]
        {
            "########################",
            "#                      #",
            "#                A B   #",
            "#                A B   #",
            "#                A B   #",
            "#                A B   #",
            "#                A B   #",
            "#        b       A B   #",
            "#       ###      A B   #",
            "#  a             A B   #",
            "# P###  ^^^      A B  E#",
            "########################",
        },

        // 7) Kapinin ardinda yuksek cikis: bir hayalet butona, biri basamaga.
        new[]
        {
            "########################",
            "#                      #",
            "#          A           #",
            "#          A           #",
            "#          A           #",
            "#          A           #",
            "#          A           #",
            "#          A      E    #",
            "#          A    ########",
            "#  a       A    ########",
            "# P##      A    ########",
            "########################",
        },

        // 8) Cift lazer: sirayla yanarlar, ritmi yakala.
        new[]
        {
            "########################",
            "#                      #",
            "#                   A  #",
            "#                   A  #",
            "#                   A  #",
            "#       L     L     A  #",
            "#       L     L     A  #",
            "#       L     L     A  #",
            "#       L     L     A  #",
            "#       L     L     A  #",
            "# P     L  a  L     A E#",
            "########################",
        },

        // 9) Lazer + diken + buton + basamak hayaleti.
        new[]
        {
            "########################",
            "#                      #",
            "#             A        #",
            "#             A        #",
            "#             A        #",
            "#     L       A        #",
            "#     L       A        #",
            "#     L       A   E    #",
            "#     L       A ########",
            "#  a  L       A ########",
            "# P## L ^^^   A ########",
            "########################",
        },

        // 10) Final: lazer, iki buton, iki kapi, basamak hayaleti.
        new[]
        {
            "########################",
            "#                      #",
            "#            A B       #",
            "#            A B       #",
            "#            A B       #",
            "#     L      A B       #",
            "#     L      A B       #",
            "#     L      A B   E   #",
            "#     L  b   A B #######",
            "#  a  L ###  A B #######",
            "# P## L      A B #######",
            "########################",
        },
    };

    // Bolum basina hayalet hakki.
    public static readonly int[] GhostBudgets = { 1, 2, 1, 1, 1, 2, 2, 1, 2, 3 };

    public static readonly string[] Hints =
    {
        "Kapı ancak butona basılıyken açık kalıyor.\nButona git ve dalga gelene kadar üstünde dur —\nöldüğün yer, hayaletinin görevi olur.",
        "İki buton, tek kapı.\nHer turda bir butonun üstünde öl;\nüçüncü turda iki hayalet de senin için basacak.",
        "Dikenler ölümcül! Üzerlerinden atla.\nButona ulaş ve orada öl — dikene basarsan\nhayaletin boşa gider.",
        "Çıkış tek zıplamayla çıkılamayacak kadar yüksek.\nDuvarın dibine git ve orada öl —\nsonra hayaletinin omuzlarına basıp tırman.",
        "Lazer belirli aralıklarla yanıp sönüyor.\nSöndüğü anda geç, butonda öl,\nsonraki turda aynı ritimle çık.",
        "İki buton, iki ayrı kapı.\nBasamaktan platforma tırman — her turda\nbir butonda öl, sonra ikisi de açıkken geç.",
        "Buton kapıyı açar ama çıkış yüksekte.\nBir hayalet butona, bir hayalet basamağa —\nüçüncü turda omuzlardan yukarı.",
        "İki lazer sırayla yanıyor.\nRitmi ezberle: geç, bekle, geç.\nTek hayalet hakkın var — dikkatli ol!",
        "Hepsi bir arada: lazer, diken, buton, tırmanış.\nÖnce buton, sonra basamak hayaleti,\nen son sen.",
        "Final! Üç hayalet hakkın var:\nbiri alttaki butona, biri üstteki butona,\nbiri basamağa. Sıranı şaşırma!",
    };
}
