// See https://aka.ms/new-console-template for more information

using DataSeries;

namespace FilRouge323
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0 || args.Contains("--dav"))
            {
                Console.WriteLine("Usage: EsportApp [--game valorant|cs2|lol]");
                return;
            }

            string? game = null;
            if (args.Contains("--game"))
                game = args[Array.IndexOf(args, "--game") + 1];
            
            var cs2 = DataSeries<Cs2Match>.From(new[]
            {
                new Cs2Match("Raphaël", "Mirage",  "CT", 21, 14, 5, 2, true),
                new Cs2Match("Kiara",   "Dust2",   "T",  26, 11, 1, 4, true),
                new Cs2Match("Raphaël", "Inferno", "T",  14, 16, 6, 1, false),
            });

            var lol = DataSeries<LolMatch>.From(new[]
            {
                new LolMatch("Noé", "Thresh", 2, 4, 18, 42, 71, true),
                new LolMatch("Noé", "Thresh", 1, 6, 12, 35, 64, false),
            });

            var valorant = DataSeries<ValorantMatch>.From(new[]
            {
                new ValorantMatch("Léa", "Jett",  18, 6, 4, 8,  13, true),
                new ValorantMatch("Léa", "Reyna", 22, 8, 2, 11,  9, false),
                new ValorantMatch("Léa", "Neon",  20, 7, 5,  9, 13, true),
            });
            
            // séries construites avec les matchs en dur des étapes 2 et 3
            if (game == null || game == "valorant")
                Console.WriteLine($"Valorant : {valorant.Count} matchs");
            if (game == null || game == "cs2")
                Console.WriteLine($"CS2      : {cs2.Count} matchs");
            if (game == null || game == "lol")
                Console.WriteLine($"LoL      : {lol.Count} matchs");
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