using CimpleUI;

public class Program
{
    public const int BENUTZER_NICHT_EINGESETELLT = -1;

    public static void Main(string[] args)
    {
        //Erzeug und probiert datenbank verbindung. Wenn kein verbinden, beendet.
        Database db = new("appuser", args[0]);
        if (!db.TestConnection())
        {
            Console.WriteLine("Kein datenbank verbindung... Programm beendet.");
            return;
        }

        List<LaenderDaten> laender = new();
        List<Benutzer> benutzer = new();
        List<Fragen> lastFragen = new();

        //Holen die Datei aus den Datenbank
        db.Laender_list_populate(laender);
        db.Benutzer_list_populate(benutzer);

        //Kontrol struct von CimpleUI
        CimpleUIController uiController = new(maxWindows: 2, maxFonts: 3);

        uiController.LoadFont("assets/fonts/ACADEROM.ttf", 18);
        uiController.LoadFont("assets/fonts/ARIALMT.ttf", 20);
        uiController.LoadFont("assets/fonts/AXCART.ttf", 24);

        bool running = true;
        Windows.NextScreen screenSelect = Windows.NextScreen.MAIN_MENU;

        int index = BENUTZER_NICHT_EINGESETELLT;
        Quiz_mode mode = Quiz_mode.NICHT_EINGESTELLT;

        // Haupt Kontrol ablauf, zeigt unterschiedlichen Fenstern. Je nach dem entscheidung des Benutzer
        while (running)
        {
            switch (screenSelect)
            {
                case Windows.NextScreen.MAIN_MENU:
                    screenSelect = Windows.MainMenu(uiController, db, laender, benutzer, out index, out mode);
                    break;
                case Windows.NextScreen.QUIZ_SCREEN:
                    if (index == BENUTZER_NICHT_EINGESETELLT || mode == Quiz_mode.NICHT_EINGESTELLT)
                    {
                        Console.WriteLine("FEHLER - einstellung nicht richtig");
                        screenSelect = Windows.NextScreen.MAIN_MENU;
                        break;
                    }
                    if (lastFragen.Count > 0)
                        lastFragen.Clear();
                    screenSelect = Windows.QuizWindow(uiController, db, laender, benutzer[index], mode, out lastFragen);
                    break;
                case Windows.NextScreen.STAT_SCREEN:
                    screenSelect = Windows.StatWindow(uiController, db, laender, lastFragen);
                    break;
                case Windows.NextScreen.HIGH_SCORE_SCREEN:
                    screenSelect = Windows.HighScoreScreen(uiController, db);
                    break;
                case Windows.NextScreen.EXIT:
                    uiController.Dispose();
                    running = false;
                    break;
                case Windows.NextScreen.ERROR_SCREEN:
                    Console.WriteLine("FEHLER - ERROR_SCREEN rueckmeldung. Enden das Program");
                    uiController.Dispose();
                    running = false;
                    break;
                default:
                    Console.WriteLine("FEHLER - sollte nicht hier sein. Enden das Program");
                    uiController.Dispose();
                    running = false;
                    break;
            }
        }
        Console.WriteLine("Exiting application...");
    }
}

/*
   string[] lines = File.ReadAllLines("assets/data_set.csv");
   foreach (string line in lines)
   {
   string[] parts = line.Split(',');

   string filePath = "assets/flags/" + parts[0].ToLower() + ".png";
   bool fileExists = File.Exists(filePath);

   if (fileExists)
   {
   File.AppendAllText("output.txt", $"{line},assets/flags/{parts[0].ToLower()}.png\n");
   }
   else
   {
   File.AppendAllText("output.txt", $"{line},NULL\n");
   System.Console.WriteLine($"File not found: {filePath}");
   }
   }
   */
