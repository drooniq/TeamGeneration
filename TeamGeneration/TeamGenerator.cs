using System.Text.Json;

namespace TeamGeneration;

public class TeamsGenerator()
{
    private List<CompositePlayer>? _attendingPlayers;
    private int? _numberOfCourts;
    private readonly Random _random = new();
    private List<Team>? _teams;
    private NameGenerator? _nameGenerator;

    public TeamsGenerator(List<Player> attendingPlayers, int numberOfCourts) : this()
    {
        _numberOfCourts = numberOfCourts;
        _nameGenerator = NameGenerator.CreateFromJson("../../../team_names.json");
        _attendingPlayers = GenerateAndSortCompositePlayers(attendingPlayers);  // calculate composite values for each player
        _teams = GenerateTeams(_attendingPlayers, _numberOfCourts);             // create initial teams based upon nrOfCourts
    }
    
    public List<Team> GenerateTeams(List<CompositePlayer>? players = null, int? numberOfCourts = null )
    {
        List<Team> teams = [];
        
        if (numberOfCourts == null || players == null)
            return teams;

        var teamSizesList = CalculateTeamSize(players.Count(), _numberOfCourts!.Value);

        //teams.AddRange(teamSizesList.Select(teamSize => new Team(_nameGenerator!.GenerateTeamName())));
        
        // create number of teams
        foreach (var teamSize in teamSizesList)
        {
            teams.Add( new Team(_nameGenerator!.GenerateTeamName()) );
        }
        
        var remainingPlayers = new List<CompositePlayer>(players); // Copy to modify
        while (remainingPlayers.Count > 0)
        {
            // Find the team with the lowest current total that isn't full
            var teamToAddTo = teams
                .Where(t => t.Players.Count < teamSizesList[teams.IndexOf(t)])
                .OrderBy(t => t.Players.Sum(p => ((CompositePlayer)p).CompositeScore))
                .First();

            // Add the next player (from sorted list, highest first)
            teamToAddTo.Players ??= [];
            teamToAddTo.Players.Add(remainingPlayers[0]);
            remainingPlayers.RemoveAt(0); // Remove assigned player
        }

        foreach (var team in teams)
        {
            Console.WriteLine($"Team {team.TeamName} ({team.TotalTeamCompositeScore()})===================");
            foreach (var player in team.Players)
            {
                Console.WriteLine($"{player.RankingPoints} => {player.Name}");
            }

            Console.WriteLine();
        }

        //var value = (int)PenaltyWeight.Penalty / 100.0;

        // foreach (var player in players)
        // {
        //     Console.WriteLine($"{player.CompositeScore:F3} => {player.Player.RankingPoints}:{player.Player!.Name}");
        // }

        return teams;
    }

    private List<int> CalculateTeamSize(int numberOfPlayers, int numberOfCourts)
    {
        var numberOfTeams = 2 * numberOfCourts;
        numberOfTeams = Math.Min(numberOfTeams, numberOfPlayers);                   // Cap teams at player count if insufficient
        var baseTeamSize = numberOfPlayers / numberOfTeams;
        var remainingPlayers = numberOfPlayers % numberOfTeams;

        var teamSizes = new List<int>(numberOfTeams);
        for (int i = 0; i < numberOfTeams; i++)
        {
            teamSizes.Add(baseTeamSize + (i < remainingPlayers ? 1 : 0));
        }
        
        return teamSizes;
    }
    
    public List<CompositePlayer> GenerateAndSortCompositePlayers(List<Player> attendingPlayers)
    {
//        var compositePlayers = attendingPlayers
//                .Select(p => new CompositePlayer(p, CalculateCompositeScore(p))).ToList();
        
        // foreach (var player in attendingPlayers)
        // {
        //     var playerScore = CalculateCompositeScore(player);
        //     compositePlayers.Add(new CompositePlayer(player, playerScore));
        // }

//        var sortedPlayers = compositePlayers
//                .OrderByDescending(p => p.Player!.RankingPoints).ToList();

        // sort descending b vs a instead of a vs b ascending
        //compositePlayers.Sort((a, b) => b.CompareTo(a));
        
        var compositeSortedPlayers = attendingPlayers
            .Select(p => new CompositePlayer(p.Name, p.RankingPoints, p.MMR, CalculateCompositeScore(p)))
            .OrderByDescending(p => p.RankingPoints)  // Sort by inherited RankingPoints
            .ToList();      
        
        return compositeSortedPlayers;
    }
    
    /*private double GetTeamTotalCompositeScore(Team team, List<CompositePlayer> compositePlayers)
    {
        return team.Players
            .Sum(p => compositePlayers
                .First(cp => cp.Player == p).CompositeScore);
    }  */  
    
    private double CalculateCompositeScore(Player player)
    {
        // (w1 * RPnorm) + (w2 * MMRnorm)
        var rpNorm = player.RankingPoints / 2000.0;     // Normalize RP (0-2000)
        var mmrNorm = player.MMR / 1000.0;     // Normalize MMR (0-2000)
        var compositeScore =  ((double)NormWeights.RP / 100.0) * rpNorm +
                                    ((double)NormWeights.MMR / 100.0) * mmrNorm;
        
        return compositeScore; 
    }
}

public class CompositePlayer(string name, int rankingPoints, int mmr, double compositeScore) 
    : Player(name, rankingPoints, mmr)
{
    public double CompositeScore { get; set; } = compositeScore;
    public Dictionary<Player, int> TeamPlayerHistory { get; set; } = new();
}

public class Team()
{
    public List<Player> Players = [];
    public string TeamName;

    public Team(string name) : this()
    {
        TeamName = name;
    }

    public Team(List<Player> teamPlayers, string teamName) : this(teamName)
    {
        Players = teamPlayers;
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public int TotalTeamCompositeScore()
    {
        return Players.Sum(p => p.RankingPoints);
    }
}