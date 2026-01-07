using Npgsql;

public class Database
{
    public const int DATABANK_FEHLER = -1;
    private readonly string connString;

    public Database(string username, string password)
    {
        connString = $"Host=localhost;Username={username};Password={password};Database=geo_quiz";
    }

    public bool TestConnection()
    {
        try
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
            return false;
        }
    }

    // erneuert die LaenderDaten list von der Datenbank
    public void Laender_list_populate(List<LaenderDaten> laender)
    {
        if (laender.Count > 0)
            laender.Clear();

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string query = "SELECT * FROM country_data";
        using var cmd = new NpgsqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var land = new LaenderDaten(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3)
                );
            laender.Add(land);
        }

    }

    //Holt die benutzern aus die datenbank rause
    public void Benutzer_list_populate(List<Benutzer> benutzer)
    {
        if (benutzer.Count > 0)
            benutzer.Clear();

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string query = "SELECT * FROM users";
        using var cmd = new NpgsqlCommand(query, conn);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var user = new Benutzer(reader.GetInt32(0), reader.GetString(1));
            benutzer.Add(user);
        }
    }

    //gibts die user_id zurueck
    public int Benutzer_hinfuegen(string Alias)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string insertBenutzer = "INSERT INTO users(alias) VALUES (@alias) RETURNING id";
        using var cmd = new NpgsqlCommand(insertBenutzer, conn);
        cmd.Parameters.AddWithValue("alias", Alias);

        try
        {
            int id = (int)cmd.ExecuteScalar();
            return id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
            return DATABANK_FEHLER;
        }
    }

    //speichert ein neu quiz rund und gibt die quiz_id zurueck
    public int Quiz_neu_hinzufuegen(Benutzer benutzer, Quiz_mode mode)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string insertBenutzer = "INSERT INTO quiz(user_id, mode_id, score) VALUES (@user_id, @mode_id, @score) RETURNING id";
        using var cmd = new NpgsqlCommand(insertBenutzer, conn);
        cmd.Parameters.AddWithValue("user_id", benutzer.BenutzerId);
        cmd.Parameters.AddWithValue("mode_id", (int)mode);
        cmd.Parameters.AddWithValue("score", 0);

        try
        {
            int id = (int)cmd.ExecuteScalar();
            return id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
            return DATABANK_FEHLER;
        }

    }

    public void Question_hinzufuegen(int quizID, int frageNummer, Fragen fragen)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string insertQuestion = "INSERT INTO questions(quiz_id, question_number, question, answer) VALUES (@quiz_id, @question_number, @question, @answer)";
        using var cmd = new NpgsqlCommand(insertQuestion, conn);
        cmd.Parameters.AddWithValue("quiz_id", quizID);
        cmd.Parameters.AddWithValue("question_number", frageNummer);
        cmd.Parameters.AddWithValue("question", fragen.Frage);
        cmd.Parameters.AddWithValue("answer", fragen.BenutzerLoesung ?? (object)DBNull.Value);
        if (fragen.BenutzerLoesung == null)
            Console.WriteLine("WARNUNG - BenutzerLoesung ist null");

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
        }

    }

    public void Quiz_score_aktualisieren(int quizID, int score)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string updateScore = "UPDATE quiz SET score = score + @score WHERE id = @quiz_id";
        using var cmd = new NpgsqlCommand(updateScore, conn);
        cmd.Parameters.AddWithValue("quiz_id", quizID);
        cmd.Parameters.AddWithValue("score", score);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
        }
    }

    public void Quiz_fertig(int quizID)
    {
        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string updateScore = "UPDATE quiz SET finished_at = CURRENT_TIMESTAMP WHERE id = @quiz_id";
        using var cmd = new NpgsqlCommand(updateScore, conn);
        cmd.Parameters.AddWithValue("quiz_id", quizID);

        try
        {
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FEHLER: {ex.Message}");
        }
    }

    public List<Quiz> Quiz_highscore_query(int amount)
    {
        List<Quiz> scores = new();

        using var conn = new NpgsqlConnection(connString);
        conn.Open();

        string query = "SELECT q.score, u.id, u.alias, q.mode_id FROM quiz q JOIN users u ON q.user_id = u.id ORDER BY score DESC LIMIT @amount";
        using var cmd = new NpgsqlCommand(query, conn);
        cmd.Parameters.AddWithValue("amount", amount);

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var quiz = new Quiz(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                (Quiz_mode)reader.GetInt32(3)
                );
            scores.Add(quiz);
        }

        return scores;
    }
}
