// See https://aka.ms/new-console-template for more information

using DataSeries;

namespace FilRouge323
{
    class Program
    {
        static void Main(string[] args)
        {

            
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
            //Console.WriteLine($"Raphaël matchs : {raphaelGenerated.Count}"); // 20
            
            Func<Cs2Match, bool> isValid = m =>
                m.Kills + m.Assists <= 50 &&
                m.Deaths >= 1;
            
            if (args.Contains("--generate"))
            {
                var target = args[Array.IndexOf(args, "--generate") + 1];

                var players = target == "all"
                    ? new[] { "Raphaël", "Kiara", "Dylan", "Noé" }
                    : new[] { target };

                foreach (var name in players)
                {
                    var series = MatchGenerator.GenerateCs2(name, 20);
                    ExportCs2(series.Filter(isValid), $"{name.ToLower()}_generated.csv");
                    Console.WriteLine($"{name} : données générées et exportées");
                }
                return;
            }
            
            string? player = args.Contains("--player")
                ? args[Array.IndexOf(args, "--player") + 1]
                : null;

            string filterMode = args.Contains("--filter")
                ? args[Array.IndexOf(args, "--filter") + 1]
                : "all";

            string stat = args.Contains("--stat")
                ? args[Array.IndexOf(args, "--stat") + 1]
                : "kda";

            // Table de prédicats — le mode CLI sélectionne une fonction
            var filters = new Dictionary<string, Func<ValorantMatch, bool>>
            {
                ["wins"]   = m => m.Won,
                ["losses"] = m => !m.Won,
                ["all"]    = m => true,
            };

            var result = valorant.Filter(filters[filterMode]);

            // Table de sélecteurs — le flag CLI choisit la fonction de transformation
            var selectors = new Dictionary<string, Func<ValorantMatch, double>>
            {
                ["kda"]     = m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths),
                ["kills"]   = m => m.Kills,
                ["assists"] = m => m.Assists,
            };

            if (!selectors.ContainsKey(stat))
                throw new ArgumentException($"Stat inconnue : {stat}");

            DataSeries<double> values = valorant.Transform(selectors[stat]);
            
            var raphaelValid = raphaelGenerated.Filter(isValid);
            //Console.WriteLine($"Avant : {raphaelGenerated.Count}, après : {raphaelValid.Count}");
            
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
            
            Func<ValorantMatch, bool> isWin       = m => m.Won;
            Func<ValorantMatch, bool> isHighScore = m => m.Kills > 20;

            // Combinaison : un nouveau prédicat (victoire éclatante) construit à partir des deux autres
            Func<ValorantMatch, bool> isCrushingWin = m => isWin(m) && isHighScore(m);

            var top = valorant.Filter(isCrushingWin);
            
            // Valorant : kills plausibles pour un match compétitif
            var valorantValid = valorant.RemoveOutliers(m =>
                m.Kills   >= 0 && m.Kills   <= 50 &&
                m.Deaths  >= 1 && m.Deaths  <= 30 &&
                m.Assists >= 0
            );

            // CS2 : contraintes similaires
            var cs2Valid = cs2.RemoveOutliers(m =>
                m.Kills + m.Assists <= 50 &&
                m.Deaths >= 1
            );

            // LoL : le support a structurellement peu de kills
            var lolValid = lol.RemoveOutliers(m =>
                m.Kills   <= 10 &&
                m.Deaths  >= 1  &&
                m.Assists >= 0  &&
                m.Cs      >= 0
            );
            
            var kdaLea = valorant
                .Filter(m => m.Player == "Léa")
                .Transform(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths));

            var kdaRaphael = cs2
                .Filter(m => m.Player == "Raphaël")
                .Transform(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths));
            
            var kdaKiara = cs2
                .Filter(m => m.Player == "Kiara")
                .Transform(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths));
            
            var kdaNoe = lol
                .Filter(m => m.Player == "Noé")
                .Transform(m => (m.Kills + m.Assists) / (double)(m.Deaths == 0 ? 1 : m.Deaths));

            int window = 3;

            var kdaLeaNorm = MathHelpers.Normalize(kdaLea);
            var kdaRaphaelNorm = MathHelpers.Normalize(kdaRaphael);
            var kdaKiaraNorm = MathHelpers.Normalize(kdaKiara);
            var kdaNoeNorm = MathHelpers.Normalize(kdaNoe);
            
            var kdaLeaSmooth = MathHelpers.Smooth(kdaLea, window);
            var kdaRaphaelSmooth = MathHelpers.Smooth(kdaRaphael, window);
            var kdaKiaraSmooth = MathHelpers.Smooth(kdaKiara, window);
            var kdaNoeSmooth = MathHelpers.Smooth(kdaNoe, window);
            
            Console.WriteLine($"Léa brut : {string.Join(", ", kdaLea.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Léa normalize : {string.Join(", ", kdaLeaNorm.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Léa smooth : {string.Join(", ", kdaLeaSmooth.Values.Select(l => l.ToString("F2")))}");
            
            Console.WriteLine();
            
            Console.WriteLine($"Raphaël brut : {string.Join(", ", kdaRaphael.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Raphaël normalize : {string.Join(", ", kdaRaphaelNorm.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Raphaël smooth : {string.Join(", ", kdaRaphaelSmooth.Values.Select(l => l.ToString("F2")))}");

            Console.WriteLine();
            
            Console.WriteLine($"Kiara brut : {string.Join(", ", kdaKiara.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Kiara normalize : {string.Join(", ", kdaKiaraNorm.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Kiara smooth : {string.Join(", ", kdaKiaraSmooth.Values.Select(l => l.ToString("F2")))}");

            Console.WriteLine();
            
            Console.WriteLine($"Noé brut : {string.Join(", ", kdaNoe.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Noé normalize : {string.Join(", ", kdaNoeNorm.Values.Select(l => l.ToString("F2")))}");
            Console.WriteLine($"Noé smooth : {string.Join(", ", kdaNoeSmooth.Values.Select(l => l.ToString("F2")))}");
            
            Console.WriteLine();
            
            Console.WriteLine(kdaLea.Count);
            Console.WriteLine(valorant.Count);
        }
    }
    
    public static class MathHelpers
    {
        public static DataSeries<double> Normalize(DataSeries<double> series)
        {
            var points = series.DataPoints.ToList();
            var values = points.Select(dp => dp.Value).ToList();
            var min    = values.Min();
            var max    = values.Max();
            var range  = max - min;
            return DataSeries<double>.From(
                points.Select(dp => new DataPoint<double>(dp.Timestamp, range == 0 ? 0.0 : (dp.Value - min) / range))
            );
        }
        
        public static DataSeries<double> Smooth(DataSeries<double> series, int windowSize)
        {
            var points = series.DataPoints.ToList();
            var values = points.Select(dp => dp.Value).ToList();
            return DataSeries<double>.From(
                Enumerable.Range(0, values.Count)
                    .Select(i =>
                    {
                        var window = values.Skip(Math.Max(0, i - windowSize + 1)).Take(windowSize);
                        return new DataPoint<double>(points[i].Timestamp, window.Average());
                    })
            );
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