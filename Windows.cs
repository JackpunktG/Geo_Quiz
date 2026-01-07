using CimpleUI;


public static class Windows
{
    public const int SCREEN_WIDTH = 1200;
    public const int SCREEN_HEIGHT = 800;
    public const int FRAGEN_MENGE = 10;
    public const float DELTATIME = 0.16f;

    public enum NextScreen
    {
        EXIT,
        MAIN_MENU,
        QUIZ_SCREEN,
        HIGH_SCORE_SCREEN,
        STAT_SCREEN,
        ERROR_SCREEN
    }

    public static NextScreen MainMenu(CimpleUIController uiController, Database db, List<Benutzer> benutzer, out int benutzerIndex, out Quiz_mode mode)
    {
        //Program Kontrollvariablen
        NextScreen nextScreen = NextScreen.ERROR_SCREEN;
        int _benutzerIndex = Program.BENUTZER_NICHT_EINGESETELLT;
        Quiz_mode _mode = Quiz_mode.NICHT_EINGESTELLT;
        bool running = true;


        //Haupt Fenster elemente
        WindowController mainWindow = new(uiController, "Geo Quiz", SCREEN_WIDTH, SCREEN_HEIGHT);

        Label titleL = new(mainWindow, 100, 50, 950, 50, "Jack's Geo Quiz", fontSize: 50, color: ColorRGBA.Olive);
        Label blurL = new(mainWindow, 550, 80, 300, 100, "Lern Geografie mit spass :)", color: ColorRGBA.Olive);

        Button exitB = new(mainWindow, 1050, 50, 100, 30, "Beenden", color: ColorRGBA.Red);
        Button addB = new(mainWindow, 50, 450, 150, 50, "Neu Benutzer", color: ColorRGBA.Maroon);
        Button highScoreB = new(mainWindow, 50, 700, 180, 80, "High Scores", color: ColorRGBA.Purple);


        Button startB = new(mainWindow, 180, 260, 150, 50, "Spiel starten", color: ColorRGBA.Green);

        TextBox addingTb = new(mainWindow, 50, 400, 300, 100);

        DropdownMenu modeDdm = new(mainWindow, "Modus", 50, 180, 280, 50);
        modeDdm.Populate("Flag vom Laender\nFlagge vom Kapital\nLaender vom Flagge\nLaender vom Kapital\nKapital vom Flagge\nKapital vom Laender");
        DropdownMenu userDdm = new(mainWindow, "Benutzer", 50, 100, 280, 50);
        userDdm.Populate(Benutzer.DDM_populate(benutzer));

        //Haupt Fenster logik
        exitB.Clicked += () =>
        {
            running = false;
            nextScreen = NextScreen.EXIT;
        };

        startB.Clicked += () =>
        {
            if (modeDdm.State == DropdownMenu_State.DROPDOWN_SELECTED && userDdm.State == DropdownMenu_State.DROPDOWN_SELECTED)
            {
                _benutzerIndex = userDdm.SelectedIndex;
                _mode = (Quiz_mode)(modeDdm.SelectedIndex + 1);
                //Console.WriteLine($"{(int)_mode}");
                if (!Enum.IsDefined(typeof(Quiz_mode), _mode))
                {
                    throw new ArgumentException("FEHLER - mode ist ausserhalb des bereichs");
                }

                running = false;
                nextScreen = NextScreen.QUIZ_SCREEN;
            }
            else
                PopupNotice.Create(mainWindow, "Waehlen Sie ein Benutzer und Quiz modus aus", "x");

        };

        addB.Clicked += () =>
        {
            string text = addingTb.ToText();
            if (string.IsNullOrWhiteSpace(text))
            {
                PopupNotice.Create(mainWindow, "Alias ist leer", "x");
                addingTb.Clear();
                return;
            }

            if (db.Benutzer_hinfuegen(text) == Database.DATABANK_FEHLER)
                PopupNotice.Create(mainWindow, "Fehler beim Benutzer hinfuegen", "x");
            else
            {
                db.Benutzer_list_populate(benutzer);
                userDdm.Populate(Benutzer.DDM_populate(benutzer));
                userDdm.Select((byte)(benutzer.Count - 1));
            }
            addingTb.Clear();
        };

        highScoreB.Clicked += () =>
        {
            running = false;
            nextScreen = NextScreen.HIGH_SCORE_SCREEN;
        };

        while (running)
        {
            uiController.Event();
            uiController.Update(DELTATIME);
            uiController.Render(ColorRGBA.Black);
        }

        //Zuweisung von einstellung
        benutzerIndex = _benutzerIndex;
        mode = _mode;

        mainWindow.Dispose();
        return nextScreen;
    }

    public static NextScreen QuizWindow(CimpleUIController uiController, Database db, List<LaenderDaten> laender, Benutzer benutzer, Quiz_mode mode, out List<Fragen> outFragen)
    {
        int gameID = db.Quiz_neu_hinzufuegen(benutzer, mode);
        if (gameID == Database.DATABANK_FEHLER)
        {
            Console.WriteLine("FEHLER - quiz nicht im Databank gespeichter");
            outFragen = new List<Fragen>();
            return NextScreen.EXIT;
        }

        //Generate fragen
        List<Fragen> fragen = Fragen.Generate_fragen(FRAGEN_MENGE, laender);

        //element possition konstanten
        const int frageX = 200, frageY = 130, frageW = 800, frageH = 300;
        const int loesung1X = 200, loesung1Y = 450, loesungW = 360, loesungH = 150;
        const int loesung2X = 640, loesung2Y = 450;
        const int loesung3X = 200, loesung3Y = 625;
        const int loesung4X = 640, loesung4Y = 625;
        const byte frageFontSize = 160;
        const byte loesungFontSize = 55;
        ColorRGBA loesungColor = ColorRGBA.Magenta;
        ColorRGBA frageColor = ColorRGBA.Orange;
        const int BUFFER = 10;

        //Program Kontrollvariablen
        NextScreen nextScreen = NextScreen.ERROR_SCREEN;
        bool lastAnswerCorrect = false;

        for (int frage = 0; frage < FRAGEN_MENGE; ++frage)
        {
            //Extrahieren LaenderDaten
            LaenderDaten landFrage = laender.First(l => l.ISO_code == fragen[frage].Frage);
            LaenderDaten loesung1 = laender.First(l => l.ISO_code == fragen[frage].Loesung1);
            LaenderDaten loesung2 = laender.First(l => l.ISO_code == fragen[frage].Loesung2);
            LaenderDaten loesung3 = laender.First(l => l.ISO_code == fragen[frage].Loesung3);
            LaenderDaten loesung4 = laender.First(l => l.ISO_code == fragen[frage].Loesung4);

            //Quiz window elemente
            WindowController quizWindow = new(uiController, $"Geo Quiz - frage: {frage + 1}", SCREEN_WIDTH, SCREEN_HEIGHT, 100);
            Label titleL = new(quizWindow, 100, 50, 950, 50, "Jack's Geo Quiz", fontSize: 50, color: ColorRGBA.Olive);

            //Popup fuer letzte antwort
            if (frage > 0)
            {
                if (lastAnswerCorrect)
                    PopupNotice.Create(quizWindow, "Richtig!", "x");
                else
                    PopupNotice.Create(quizWindow, "Falsch!", "x");
            }

            //Guess elemente
            switch (mode)
            {
                case Quiz_mode.FLAGGE_VOM_LAENDER:
                case Quiz_mode.FLAGGE_VOM_KAPITAL:
                    Image frageI = new(quizWindow, landFrage.FlaggePfad, frageX, frageY, frageW, frageH);
                    break;
                case Quiz_mode.LAENDER_VOM_FLAGGE:
                case Quiz_mode.LAENDER_VOM_KAPITAL:
                    Label frageLandL = new(quizWindow, frageX, frageY, frageW, frageH, landFrage.Land, fontSize: (landFrage.Land.Length < 12) ? frageFontSize : (byte)100, color: frageColor);
                    break;
                case Quiz_mode.KAPITAL_VOM_FLAGGE:
                case Quiz_mode.KAPITAL_VOM_LAENDER:
                    Label frageKapL = new(quizWindow, frageX, frageY, frageW, frageH, landFrage.Kapital, fontSize: (landFrage.Land.Length < 12) ? frageFontSize : (byte)100, color: frageColor);
                    break;
                default:
                    Console.WriteLine("FEHLER - Frage Mode nicht richtig");
                    break;
            }

            //Loesung knopfe
            Button button1B = new(quizWindow, loesung1X, loesung1Y, loesungW, loesungH, ".", fontSize: 5, color: ColorRGBA.White);
            Button button2B = new(quizWindow, loesung2X, loesung2Y, loesungW, loesungH, ".", fontSize: 5, color: ColorRGBA.White);
            Button button3B = new(quizWindow, loesung3X, loesung3Y, loesungW, loesungH, ".", fontSize: 5, color: ColorRGBA.White);
            Button button4B = new(quizWindow, loesung4X, loesung4Y, loesungW, loesungH, ".", fontSize: 5, color: ColorRGBA.White);

            //Loesung elemente
            switch (mode)
            {
                case Quiz_mode.FLAGGE_VOM_LAENDER:
                case Quiz_mode.KAPITAL_VOM_LAENDER:
                case Quiz_mode.LAENDER_VOM_KAPITAL:
                case Quiz_mode.FLAGGE_VOM_KAPITAL:
                    Label l1L = new(quizWindow, loesung1X, loesung1Y, loesungW, loesungH, (mode == Quiz_mode.FLAGGE_VOM_LAENDER || mode == Quiz_mode.KAPITAL_VOM_LAENDER) ? loesung1.Land : loesung1.Kapital,
                            fontSize: (landFrage.Land.Length < 12) ? loesungFontSize : (byte)35, color: loesungColor);
                    Label l2L = new(quizWindow, loesung2X, loesung2Y, loesungW, loesungH, (mode == Quiz_mode.FLAGGE_VOM_LAENDER || mode == Quiz_mode.KAPITAL_VOM_LAENDER) ? loesung2.Land : loesung2.Kapital,
                            fontSize: (landFrage.Land.Length < 12) ? loesungFontSize : (byte)35, color: loesungColor);
                    Label l3L = new(quizWindow, loesung3X, loesung3Y, loesungW, loesungH, (mode == Quiz_mode.FLAGGE_VOM_LAENDER || mode == Quiz_mode.KAPITAL_VOM_LAENDER) ? loesung3.Land : loesung3.Kapital,
                            fontSize: (landFrage.Land.Length < 12) ? loesungFontSize : (byte)35, color: loesungColor);
                    Label l4L = new(quizWindow, loesung4X, loesung4Y, loesungW, loesungH, (mode == Quiz_mode.FLAGGE_VOM_LAENDER || mode == Quiz_mode.KAPITAL_VOM_LAENDER) ? loesung4.Land : loesung4.Kapital,
                            fontSize: (landFrage.Land.Length < 12) ? loesungFontSize : (byte)35, color: loesungColor);
                    break;
                case Quiz_mode.KAPITAL_VOM_FLAGGE:
                case Quiz_mode.LAENDER_VOM_FLAGGE:
                    Image l1I = new(quizWindow, loesung1.FlaggePfad, loesung1X + (BUFFER / 2), loesung1Y + (BUFFER / 2), loesungW - BUFFER, loesungH - BUFFER);
                    Image l2I = new(quizWindow, loesung2.FlaggePfad, loesung2X + (BUFFER / 2), loesung2Y + (BUFFER / 2), loesungW - BUFFER, loesungH - BUFFER);
                    Image l3I = new(quizWindow, loesung3.FlaggePfad, loesung3X + (BUFFER / 2), loesung3Y + (BUFFER / 2), loesungW - BUFFER, loesungH - BUFFER);
                    Image l4I = new(quizWindow, loesung4.FlaggePfad, loesung4X + (BUFFER / 2), loesung4Y + (BUFFER / 2), loesungW - BUFFER, loesungH - BUFFER);
                    break;
                default:
                    Console.WriteLine("FEHLER - Loesung Mode nicht richtig");
                    break;
            }

            //quizWindow Logik
            bool running = true;

            button1B.Clicked += () =>
            {
                fragen[frage].BenutzerLoesung = loesung1.ISO_code;
                running = false;
            };
            button2B.Clicked += () =>
            {
                fragen[frage].BenutzerLoesung = loesung2.ISO_code;
                running = false;
            };
            button3B.Clicked += () =>
            {
                fragen[frage].BenutzerLoesung = loesung3.ISO_code;
                running = false;
            };
            button4B.Clicked += () =>
            {
                fragen[frage].BenutzerLoesung = loesung4.ISO_code;
                running = false;
            };

            while (running)
            {
                uiController.Event();
                uiController.Update(DELTATIME);
                uiController.Render(ColorRGBA.DarkGray);
            }

            db.Question_hinzufuegen(gameID, frage + 1, fragen[frage]);
            if (fragen[frage].BenutzerLoesung == landFrage.ISO_code)
            {
                db.Quiz_score_aktualisieren(gameID, 1);
                lastAnswerCorrect = true;
            }
            else
                lastAnswerCorrect = false;

            quizWindow.Dispose();
        }
        db.Quiz_fertig(gameID);

        nextScreen = NextScreen.STAT_SCREEN;
        outFragen = fragen;
        return nextScreen;
    }

    public static NextScreen StatWindow(CimpleUIController uiController, Database db, List<LaenderDaten> laender, List<Fragen> fragen)
    {
        //Program Kontrollvariablen
        NextScreen nextScreen = NextScreen.ERROR_SCREEN;
        int score = 0;
        bool running = true;


        //Haupt Fenster elemente
        WindowController statWindow = new(uiController, "Geo Quiz", SCREEN_WIDTH, SCREEN_HEIGHT);
        Label titleL = new(statWindow, 100, 50, 950, 50, "Jack's Geo Quiz", fontSize: 50, color: ColorRGBA.Olive);

        //PopupNotice for last question
        if (fragen[fragen.Count - 1].BenutzerLoesung == fragen[fragen.Count - 1].Frage)
            PopupNotice.Create(statWindow, "Richtig!", "x");
        else
            PopupNotice.Create(statWindow, "Falsch!", "x");


        const int x = 200, x2 = 600, y = 200, w = 300, h = 50;

        //Stat Screen Elemente
        Label frageL = new(statWindow, 200, 150, 300, 50, "Fragen:", fontIndex: 1, fontSize: 30, color: ColorRGBA.Navy);
        Label loesungL = new(statWindow, 600, 150, 300, 50, "Antworten:", fontIndex: 1, fontSize: 30, color: ColorRGBA.Navy);

        for (int i = 0; i < 10; ++i)
        {
            LaenderDaten f = laender.First(l => l.ISO_code == fragen[i].Frage);
            LaenderDaten a = laender.First(l => l.ISO_code == fragen[i].BenutzerLoesung);
            Image imgf = new(statWindow, f.FlaggePfad, x - 100, y + (h * i), 50, 30);
            Label frage = new(statWindow, x, y + (h * i), w, h, $"{f.Land}/{f.Kapital}", fontIndex: 1, color: ColorRGBA.Navy);
            Label antwort = new(statWindow, x2, y + (h * i), w, h, $"{a.Land}/{f.Kapital}", fontIndex: 1, color: ColorRGBA.Navy);
            Image imga = new(statWindow, a.FlaggePfad, x2 + w + 100, y + (h * i), 50, 30);
            if (a == f)
                ++score;
        }
        Label scoreL = new(statWindow, 850, 720, 120, 50, $"Score: {score}", fontSize: 25);

        Button mainMenuB = new(statWindow, 50, 720, 100, 50, "Hauptmenu");

        //Logik
        mainMenuB.Clicked += () =>
        {
            nextScreen = NextScreen.MAIN_MENU;
            running = false;
        };

        while (running)
        {
            uiController.Event();
            uiController.Update(DELTATIME);
            uiController.Render(ColorRGBA.DarkGray);
        }

        statWindow.Dispose();
        return nextScreen;
    }

    public static NextScreen HighScoreScreen(CimpleUIController uiController, Database db)
    {
        //Program Kontrollvariablen
        NextScreen nextScreen = NextScreen.ERROR_SCREEN;
        bool running = true;

        int HIGH_SCORE_AMOUNT = 10;

        //Holt die highscore aus dem databank raus
        List<Quiz> highScores = db.Quiz_highscore_query(HIGH_SCORE_AMOUNT);
        //Falls weniger rekords im datenbank sind
        if (highScores.Count < HIGH_SCORE_AMOUNT)
            HIGH_SCORE_AMOUNT = highScores.Count;

        //HighScore Fenster elemente
        WindowController highScoreWindow = new(uiController, "Geo Quiz", SCREEN_WIDTH, SCREEN_HEIGHT);

        Label titleL = new(highScoreWindow, 100, 50, 950, 50, "Jack's Geo Quiz", fontSize: 50, color: ColorRGBA.Olive);
        Label blurL = new(highScoreWindow, 550, 80, 300, 100, "Lern Geografie mit spass :)", color: ColorRGBA.Olive);

        const int x = 250, y = 200, w = 600, h = 30;

        if (HIGH_SCORE_AMOUNT == 0)
        {
            Label score = new(highScoreWindow, x, y, w, h, "Kein Scores gespeichert", fontIndex: 1, color: ColorRGBA.Navy);
        }
        else
        {
            for (int i = 0; i < HIGH_SCORE_AMOUNT; ++i)
            {
                Label score = new(highScoreWindow, x, y + (h * i), w, h, $"{highScores[i].ToString()}", fontIndex: 1, color: ColorRGBA.Navy);
            }
        }

        Button highScoreMenuB = new(highScoreWindow, 50, 720, 100, 50, "Hauptmenu");

        //Logik
        highScoreMenuB.Clicked += () =>
        {
            nextScreen = NextScreen.MAIN_MENU;
            running = false;
        };

        while (running)
        {
            uiController.Event();
            uiController.Update(DELTATIME);
            uiController.Render(ColorRGBA.DarkGray);
        }

        highScoreWindow.Dispose();
        return nextScreen;
    }
}
