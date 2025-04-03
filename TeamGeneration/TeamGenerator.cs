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
    }
    
public List<Team> GenerateTeams(List<CompositePlayer>? players = null, int? numberOfCourts = null)
    {
        if (numberOfCourts == null || players == null)
            return new List<Team>();

        // Step 1: Create initial empty teams
        var teams = CreateEmptyTeams(players.Count, numberOfCourts.Value);
        
        // Step 2: Identify and separate common pairs
        var remainingPlayers = SeparateCommonPairs(players, teams);
        
        // Step 3: Assign remaining players optimally
        AssignRemainingPlayers(remainingPlayers, teams);
        
        // Step 4: Update player history data
        UpdateTeamPlayerHistory(teams);
        
        // Step 5: Log results
        LogTeamResults(teams);

        return teams;
    }

    private List<Team> CreateEmptyTeams(int playerCount, int courtCount)
    {
        List<Team> teams = new List<Team>();
        var teamSizesList = CalculateTeamSize(playerCount, courtCount);
        
        foreach (var teamSize in teamSizesList)
        {
            teams.Add(new Team(_nameGenerator!.GenerateTeamName()));
        }
        
        return teams;
    }

    private Dictionary<(string, string), int> IdentifyCommonPairs(List<CompositePlayer> players)
    {
        Dictionary<(string, string), int> pairFrequency = new Dictionary<(string, string), int>();
        
        foreach (var player in players)
        {
            foreach (var (teammate, count) in player.TeamPlayerHistory.Where(h => h.Value > 0))
            {
                var pair = String.Compare(player.Name, teammate, StringComparison.Ordinal) < 0 
                    ? (player.Name, teammate) 
                    : (teammate, player.Name);
                    
                if (!pairFrequency.ContainsKey(pair))
                    pairFrequency[pair] = 0;
                    
                pairFrequency[pair] += count;
            }
        }
        
        return pairFrequency;
    }

    private List<CompositePlayer> SeparateCommonPairs(List<CompositePlayer> players, List<Team> teams)
    {
        // Copy players to modify
        var remainingPlayers = new List<CompositePlayer>(players);
        
        // Get most common pairs
        var pairFrequency = IdentifyCommonPairs(players);
        var orderedPairs = pairFrequency.OrderByDescending(p => p.Value).ToList();
        
        // Calculate team sizes
        var teamSizesList = CalculateTeamSize(players.Count, teams.Count / 2);
        
        // Separate most common pairs into different teams
        foreach (var ((player1, player2), count) in orderedPairs)
        {
            var p1 = remainingPlayers.FirstOrDefault(p => p.Name == player1);
            var p2 = remainingPlayers.FirstOrDefault(p => p.Name == player2);
            
            if (p1 == null || p2 == null) continue; // Player might have been assigned already
            
            // Find two different teams with open slots
            var availableTeams = teams
                .Where(t => t.Players.Count < teamSizesList[teams.IndexOf(t)])
                .ToList();
                
            if (availableTeams.Count < 2) break; // Not enough teams available
            
            // Assign to different teams
            availableTeams[0].Players.Add(p1);
            availableTeams[1].Players.Add(p2);
            
            // Remove from remaining players
            remainingPlayers.Remove(p1);
            remainingPlayers.Remove(p2);
            
            //Console.WriteLine($"Separated common pair: {player1} and {player2} (paired {count} times)");
        }
        
        return remainingPlayers;
    }

    private void AssignRemainingPlayers(List<CompositePlayer> remainingPlayers, List<Team> teams)
    {
        // Add some randomness to player order to prevent deterministic patterns
        Random rng = new Random();
        var sortedPlayers = remainingPlayers
            .OrderByDescending(p => p.CompositeScore + (rng.NextDouble() * 0.05))
            .ToList();
        
        var teamSizesList = CalculateTeamSize(teams.Sum(t => t.Players.Count) + sortedPlayers.Count, teams.Count / 2);
        var penaltyWeight = (double)PenaltyWeight.Penalty / 100;
        
        while (sortedPlayers.Count > 0)
        {
            var currentPlayer = sortedPlayers[0];
            
            // Find the team with the lowest adjusted score
            var teamToAddTo = FindOptimalTeam(currentPlayer, teams, teamSizesList, penaltyWeight);

            // Add the player to the team
            teamToAddTo.Players.Add(currentPlayer);
            sortedPlayers.RemoveAt(0);
        }
    }
    
    private Team FindOptimalTeam(CompositePlayer player, List<Team> teams, List<int> teamSizesList, double penaltyWeight)
    {
        return teams
            .Where(t => t.Players.Count < teamSizesList[teams.IndexOf(t)]) // Not full
            .OrderBy(t => CalculateAdjustedScore(player, t, penaltyWeight))
            .First();
    }
    
    private double CalculateAdjustedScore(CompositePlayer player, Team team, double penaltyWeight)
    {
        // Base score: sum of CompositeScores
        double baseScore = team.Players.Sum(p => p is CompositePlayer cp ? cp.CompositeScore : 0);

        // Penalty: sum of history counts with current team members
        double penalty = team.Players
            .Sum(p => player.TeamPlayerHistory.GetValueOrDefault(p.Name, 0));
        
        // Apply a quadratic penalty to more strongly discourage repeat pairings
        double quadraticPenalty = penalty * penalty * (penaltyWeight * 2);
        
        double adjusted = baseScore + quadraticPenalty;
        //Console.WriteLine($"{player.Name} to {team.TeamName}: base={baseScore:F3}, penalty={penalty}, adjusted={adjusted:F3}");
        
        return adjusted;
    }

    private void UpdateTeamPlayerHistory(List<Team> teams)
    {
        foreach (var team in teams)
        {
            foreach (var player in team.Players.Cast<CompositePlayer>())
            {
                foreach (var teammate in team.Players.Cast<CompositePlayer>())
                {
                    if (player != teammate)
                    {
                        player.TeamPlayerHistory.TryGetValue(teammate.Name, out int currentCount);
                        player.TeamPlayerHistory[teammate.Name] = currentCount + 1;
                    }
                }
            }
        }
    }
    
    private void LogTeamResults(List<Team> teams)
    {
        Console.WriteLine("------------------------------------------------------------------------------------------------");
        foreach (var team in teams)
        {
            Console.WriteLine($"Team {team.TeamName} ({team.TotalTeamCompositeScore()} Total Ranking Score) ===================");
            foreach (var player in team.Players)
            {
                Console.WriteLine($"{player.RankingPoints} : {player.Name} {((CompositePlayer)player).CompositeScore:F3}");
            }
            Console.WriteLine();
        }
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
            .OrderByDescending(p => p.CompositeScore)  // Sort by inherited RankingPoints
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
        var rpNorm  = player.RankingPoints / 2000.0;    // Normalize RP (0-2000)
        var mmrNorm = player.MMR / 1000.0;              // Normalize MMR (0-2000)
        var compositeScore =  ((double)NormWeights.RP / 100.0) * rpNorm +
                                    ((double)NormWeights.MMR / 100.0) * mmrNorm;
        return compositeScore;
    }

    public void UpdateMatchResult(Team winner, Team loser)
    {
        foreach(var player in winner.Players)
        {
            player.MMR += 50; // Increase winner's MMR
            ((CompositePlayer)player).CompositeScore = CalculateCompositeScore(player);
        }
        foreach(var player in loser.Players)
        {
            player.MMR -= 50; // Decrease winner's MMR
            ((CompositePlayer)player).CompositeScore = CalculateCompositeScore(player);
        }
    }
}

public class CompositePlayer(string name, int rankingPoints, int mmr, double compositeScore) 
    : Player(name, rankingPoints, mmr)
{
    public double CompositeScore { get; set; } = compositeScore;
    public Dictionary<string, int> TeamPlayerHistory { get; set; } = new();
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