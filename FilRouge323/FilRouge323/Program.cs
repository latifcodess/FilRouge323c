// See https://aka.ms/new-console-template for more information

using DataSeries;

namespace FilRouge323
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Contains("--generate"))
            {
                var target = args[Array.IndexOf(args, "--generate") + 1];

                var players = target == "all"
                    ? new[] { "Raphaël", "Kiara", "Dylan", "Noé" }
                    : new[] { target };

                foreach (var player in players)
                {
                    var series = MatchGenerator.GenerateCs2(player, 20);
                    ExportCs2(series.Filter(isValid), $"{player.ToLower()}_generated.csv");
                    Console.WriteLine($"{player} : données générées et exportées");
                }
                return;
            }
            
            ValorantMatch ParseValorant(string[] cols) => new ValorantMatch(
                cols[1],              // player
                cols[2],              // agent
                int.Parse(cols[3]),   // kills
                int.Parse(cols[4]),   // deaths
                int.Parse(cols[5]),   // assists
                int.Parse(cols[6]),   // headshots
                int.Parse(cols[7]),   // roundsWon
                bool.Parse(cols[8])   // won
            );

            Cs2Match ParseCs2(string[] cols) => new Cs2Match(
                cols[1],              // player
                cols[2],              // map
                cols[3],              // startSide (côté joué en 1re mi-temps — CT ou T)
                int.Parse(cols[4]),   // kills
                int.Parse(cols[5]),   // deaths
                int.Parse(cols[6]),   // assists
                int.Parse(cols[7]),   // mvps
                bool.Parse(cols[8])   // won
            );

            LolMatch ParseLol(string[] cols) => new LolMatch(
                cols[1],              // player
                cols[2],              // champion
                int.Parse(cols[4]),   // kills
                int.Parse(cols[5]),   // deaths
                int.Parse(cols[6]),   // assists
                int.Parse(cols[7]),   // cs
                int.Parse(cols[8]),   // visionScore
                bool.Parse(cols[9])   // won
            );
            
            var valorant = DataSeries<ValorantMatch>.FromCsv("data/valorant.csv", ParseValorant);
            var cs2      = DataSeries<Cs2Match>.FromCsv("data/cs2.csv", ParseCs2);
            var lol      = DataSeries<LolMatch>.FromCsv("data/lol.csv", ParseLol);

            Console.WriteLine($"Valorant : {valorant.Count} matchs");
            Console.WriteLine($"CS2      : {cs2.Count} matchs");
            Console.WriteLine($"LoL      : {lol.Count} matchs \n");
            // Total : 75 matchs
            
            var raphaelGenerated = MatchGenerator.GenerateCs2("Raphaël", 20);
            Console.WriteLine($"Raphaël matchs : {raphaelGenerated.Count}"); // 20
            
            Func<Cs2Match, bool> isValid = m =>
                m.Kills + m.Assists <= 50 &&
                m.Deaths >= 1;

            var raphaelValid = raphaelGenerated.Filter(isValid);
            Console.WriteLine($"Avant : {raphaelGenerated.Count}, après : {raphaelValid.Count}");
            
            void ExportCs2(DataSeries<Cs2Match> matches, string path)
            {
                var header = "date,player,map,start_side,kills,deaths,assists,mvps,won";
                var lines = matches.DataPoints.Select(dp =>
                    $"{dp.Timestamp:yyyy-MM-dd},{dp.Value.Player},{dp.Value.Map},{dp.Value.StartSide}," +
                    $"{dp.Value.Kills},{dp.Value.Deaths},{dp.Value.Assists},{dp.Value.Mvps},{dp.Value.Won.ToString().ToLower()}"
                );
                File.WriteAllLines(path, lines.Prepend(header));
            }

            // Générer, filtrer et exporter
            ExportCs2(raphaelValid, "raphael_generated.csv");
        }
    }
    
    public class ValorantMatch
    {
        public string Player { get; }
        public string Agent { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Headshots { get; }
        public int RoundsWon { get; }
        public bool Won { get; }

        public ValorantMatch(string player, string agent, int kills, int deaths,
            int assists, int headshots, int roundsWon, bool won)
        {
            Player = player;
            Agent = agent;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Headshots = headshots;
            RoundsWon = roundsWon;
            Won = won;
        }
    }
    
    public class Cs2Match
    {
        public string Player { get; }
        public string Map { get; }
        public string StartSide { get; }  // côté joué en 1re mi-temps (CT ou T)
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Mvps { get; }
        public bool Won { get; }

        public Cs2Match(string player, string map, string startSide, int kills,
            int deaths, int assists, int mvps, bool won)
        {
            Player = player;
            Map = map;
            StartSide = startSide;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Mvps = mvps;
            Won = won;
        }
    }

    public class LolMatch
    {
        public string Player { get; }
        public string Champion { get; }
        public int Kills { get; }
        public int Deaths { get; }
        public int Assists { get; }
        public int Cs { get; }
        public int VisionScore { get; }
        public bool Won { get; }

        public LolMatch(string player, string champion, int kills, int deaths,
            int assists, int cs, int visionScore, bool won)
        {
            Player = player;
            Champion = champion;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Cs = cs;
            VisionScore = visionScore;
            Won = won;
        }
    }
}