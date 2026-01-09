/* --- Datei typen --- */

public record LaenderDaten(string ISO_code, string Land, string Hauptstadt, string FlaggePfad);

public record Benutzer(int BenutzerId, string Alias)
{
    //Bei DropdowmMenu generieren ein string mit '\n' zwischen ergebnisse
    public static string DDM_populate(List<Benutzer> benutzer)
    {
        string result = "";
        foreach (Benutzer b in benutzer)
        {
            result += $"{b.Alias}";
            if (b != benutzer.Last())
                result += "\n";
        }

        return result;
    }
}

public enum Quiz_mode
{
    NICHT_EINGESTELLT = 0,
    FLAGGE_VOM_LAENDER = 1,
    FLAGGE_VOM_HAUPTSTADT = 2,
    LAENDER_VOM_FLAGGE = 3,
    LAENDER_VOM_HAUPTSTADT = 4,
    HAUPTSTADT_VOM_FLAGGE = 5,
    HAUPTSTADT_VOM_LAENDER = 6
}

public class Quiz
{
    public int Score { get; set; }
    public int BenutzerId { get; init; }
    public string BenutzerAlias { get; init; }
    public Quiz_mode Mode { get; init; }

    public Quiz(Benutzer benutzer, Quiz_mode mode)
    {
        Score = 0;
        BenutzerId = benutzer.BenutzerId;
        BenutzerAlias = benutzer.Alias;
        Mode = mode;
    }

    public Quiz(int score, int benutzerID, string benutzerAlias, Quiz_mode mode)
    {
        Score = score;
        BenutzerId = benutzerID;
        BenutzerAlias = benutzerAlias;
        Mode = mode;
    }

    public override string ToString()
    {
        string mode = Mode.ToString().ToLower();
        mode = mode.Replace('_', ' ');
        return $"{BenutzerAlias}   -    Modus: {mode}   -    Score: {Score}";
    }
}

public class Fragen
{
    /*
       Erklaerung zum variablen:
    Ich habe die so gelassen, weil ich wolte, dass die loesungen im eine unterschiedlich stellen platziert werden koennen. 
    Ich muss ein variable trotzdem speichen um zu wissen welche platz ist die richtiger loesung. 
    Deswegen habe ich so gelassen.
    */

    public string Frage { get; init; }
    public string Loesung1 { get; init; }
    public string Loesung2 { get; init; }
    public string Loesung3 { get; init; }
    public string Loesung4 { get; init; }
    public string? BenutzerLoesung { get; set; }

    public Fragen(string codeFrage, string l1, string l2, string l3, string l4)
    {
        Frage = codeFrage;
        Loesung1 = l1;
        Loesung2 = l2;
        Loesung3 = l3;
        Loesung4 = l4;
        BenutzerLoesung = null;
    }


    // Generiert ein fragen list fuer das Quiz
    public static List<Fragen> Generate_fragen(int questionCount, List<LaenderDaten> laender)
    {
        List<Fragen> fragen = new();

        int[] randIndex = Random_Index(questionCount * 4, laender.Count);

        for (int i = 0; i < questionCount; ++i)
        {
            int[] loesungen = new int[4];
            int k = 0;
            for (int j = i * 4; j < ((i + 1) * 4); ++j)
                loesungen[k++] = randIndex[j];

            int frage = Random.Shared.Next(0, 4);

            Fragen f = new(laender[loesungen[frage]].ISO_code, laender[loesungen[0]].ISO_code, laender[loesungen[1]].ISO_code, laender[loesungen[2]].ISO_code, laender[loesungen[3]].ISO_code);
            fragen.Add(f);
        }

        return fragen;

    }

    // function das macht ein eindeutiger array vom angegeben int
    private static int[] Random_Index(int amount, int max)
    {
        int[] randIndex = new int[amount];
        for (int i = 0; i < amount; ++i)
        {
            bool set = false;
            while (!set)
            {
                int randI = Random.Shared.Next(0, max);
                bool unique = true;
                for (int j = 0; j < i; ++j)
                {
                    if (randIndex[j] == randI)
                        unique = false;
                }

                if (unique)
                {
                    randIndex[i] = randI;
                    set = true;
                }
            }
        }
        return randIndex;
    }
}
