namespace TeamGeneration;

/// <summary>
/// Generates teams from a list of players, optimizing for balanced teams and minimizing repeat pairings.
/// </summary>
public class TeamsGenerator()
{
    public const double WIN_LOSE_K_FACTOR = 0.04;

    private int? _numberOfCourts;
    private readonly NameGenerator? _nameGenerator;

    /// <summary>
    /// Initializes a new instance of the TeamsGenerator class with specified players and court count.
    /// </summary>
    /// <param name="attendingPlayers">The list of players to generate teams from.</param>
    /// <param name="numberOfCourts">The number of available courts.</param>
    public TeamsGenerator(List<Player> attendingPlayers, int numberOfCourts) : this()
    {
        _numberOfCourts = numberOfCourts;
        _nameGenerator = NameGenerator.CreateFromJson("../../../team_names.json");
    }

    /// <summary>
    /// Generates teams from the provided players, balancing skill levels and avoiding frequent pairings.
    /// </summary>
    /// <param name="players">The list of players to distribute into teams. Uses class field if null.</param>
    /// <param name="numberOfCourts">The number of courts available. Uses class field if null.</param>
    /// <returns>A list of generated teams.</returns>
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

        // Step 5: Log Teams
        LogTeamResults(teams);

        return teams;
    }

    /// <summary>
    /// Creates empty teams based on player count and available courts.
    /// </summary>
    /// <param name="playerCount">Total number of players to distribute.</param>
    /// <param name="courtCount">Number of available courts.</param>
    /// <returns>A list of empty teams with generated names.</returns>
    private List<Team> CreateEmptyTeams(int playerCount, int courtCount)
    {
        List<Team> teams = new List<Team>();
        var teamSizesList = CalculateTeamSize(playerCount, courtCount);

        foreach (var unused in teamSizesList)
        {
            teams.Add(new Team(_nameGenerator!.GenerateTeamName()));
        }

        return teams;
    }

    /// <summary>
    /// Identifies how often players have been teamed together historically.
    /// </summary>
    /// <param name="players">The list of players to analyze.</param>
    /// <returns>A dictionary mapping player pairs to their team-up frequency.</returns>
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

    /// <summary>
    /// Separates frequently paired players into different teams.
    /// </summary>
    /// <param name="players">The list of players to process.</param>
    /// <param name="teams">The list of teams to assign players to.</param>
    /// <returns>A list of players remaining after common pairs are assigned.</returns>
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

    /// <summary>
    /// Assigns remaining players to teams while maintaining balance.
    /// </summary>
    /// <param name="remainingPlayers">The list of players yet to be assigned.</param>
    /// <param name="teams">The list of teams to assign players to.</param>
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

    /// <summary>
    /// Finds the most suitable team for a player based on balance and history.
    /// </summary>
    /// <param name="player">The player to assign.</param>
    /// <param name="teams">The list of available teams.</param>
    /// <param name="teamSizesList">The target sizes for each team.</param>
    /// <param name="penaltyWeight">The weight applied to repeat pairing penalties.</param>
    /// <returns>The optimal team for the player.</returns>
    private Team FindOptimalTeam(CompositePlayer player, List<Team> teams, List<int> teamSizesList,
        double penaltyWeight)
    {
        return teams
            .Where(t => t.Players.Count < teamSizesList[teams.IndexOf(t)]) // Not full
            .OrderBy(t => CalculateAdjustedScore(player, t, penaltyWeight))
            .First();
    }

    /// <summary>
    /// Calculates a team's score adjusted for player history penalties.
    /// </summary>
    /// <param name="player">The player being considered.</param>
    /// <param name="team">The team to evaluate.</param>
    /// <param name="penaltyWeight">The weight applied to repeat pairing penalties.</param>
    /// <returns>The adjusted score for adding the player to the team.</returns>
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

    /// <summary>
    /// Updates the team-up history for all players in the generated teams.
    /// </summary>
    /// <param name="teams">The list of teams to update history for.</param>
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

    /// <summary>
    /// Logs the generated team compositions to the console.
    /// </summary>
    /// <param name="teams">The list of teams to log.</param>
    private void LogTeamResults(List<Team> teams)
    {
        Console.WriteLine(
            "------------------------------------------------------------------------------------------------");
        foreach (var team in teams)
        {
            Console.WriteLine(
                $"Team {team.TeamName} ({team.TotalTeamRankingScore()} Total Ranking Score) ===================");
            foreach (var player in team.Players)
            {
                Console.WriteLine(
                    $"{player.RankingPoints} : {player.Name} {((CompositePlayer)player).CompositeScore:F3}");
            }

            Console.WriteLine();
        }
    }

    /// <summary>
    /// Calculates the size of each team based on player and court counts.
    /// </summary>
    /// <param name="numberOfPlayers">Total number of players.</param>
    /// <param name="numberOfCourts">Number of available courts.</param>
    /// <returns>A list of team sizes.</returns>
    private List<int> CalculateTeamSize(int numberOfPlayers, int numberOfCourts)
    {
        var numberOfTeams = 2 * numberOfCourts;
        numberOfTeams = Math.Min(numberOfTeams, numberOfPlayers); // Cap teams at player count if insufficient
        var baseTeamSize = numberOfPlayers / numberOfTeams;
        var remainingPlayers = numberOfPlayers % numberOfTeams;

        var teamSizes = new List<int>(numberOfTeams);
        for (int i = 0; i < numberOfTeams; i++)
        {
            teamSizes.Add(baseTeamSize + (i < remainingPlayers ? 1 : 0));
        }

        return teamSizes;
    }

    /// <summary>
    /// Creates and sorts composite players from the input player list.
    /// </summary>
    /// <param name="attendingPlayers">The list of basic players to convert.</param>
    /// <returns>A sorted list of composite players.</returns>
    public List<CompositePlayer> GenerateAndSortCompositePlayers(List<Player> attendingPlayers)
    {
        var compositeSortedPlayers = attendingPlayers
            .Select(p => new CompositePlayer(p.Name, p.RankingPoints, p.MMR, CalculateCompositeScore(p)))
            .OrderByDescending(p => p.CompositeScore) // Sort by inherited RankingPoints
            .ToList();

        return compositeSortedPlayers;
    }

    /// <summary>
    /// Calculates a composite score based on player's ranking points and MMR.
    /// </summary>
    /// <param name="player">The player to calculate the score for.</param>
    /// <returns>The calculated composite score.</returns>
    private double CalculateCompositeScore(Player player)
    {
        // (w1 * RPnorm) + (w2 * MMRnorm)
        var rpNorm = player.RankingPoints / 2000.0; // Normalize RP (0-2000)
        var mmrNorm = player.MMR / 1000.0; // Normalize MMR (0-2000)
        var compositeScore = ((double)NormWeights.RP / 100.0) * rpNorm +
                             ((double)NormWeights.MMR / 100.0) * mmrNorm;
        return compositeScore;
    }

    /// <summary>
    /// Updates player statistics based on match results using dynamic MMR adjustments.
    /// Rewards weaker teams more for wins and penalizes stronger teams more for losses, with no bounds.
    /// </summary>
    /// <param name="winner">The winning team.</param>
    /// <param name="loser">The losing team.</param>
    public void UpdateMatchResult(Team winner, Team loser)
    {
        // Calculate TotalRankingPoints for each team using Team method
        int totalRankingPointsWinner = winner.TotalTeamRankingScore();
        int totalRankingPointsLoser = loser.TotalTeamRankingScore();

        // Calculate Strength Difference from each team's perspective
        int winnerDifference = totalRankingPointsLoser - totalRankingPointsWinner;
        int loserDifference = totalRankingPointsWinner - totalRankingPointsLoser;

        // Calculate MMR changes with no clamping
        int winnerMmrChange = 10 + (winnerDifference > 0 ? (int)(winnerDifference * WIN_LOSE_K_FACTOR) : 0);
        int loserMmrChange = -10 - (loserDifference < 0 ? (int)(-loserDifference * WIN_LOSE_K_FACTOR) : 0);

        foreach (var player in winner.Players)
        {
            player.MMR += winnerMmrChange;
            ((CompositePlayer)player).CompositeScore = CalculateCompositeScore(player);
        }

        foreach (var player in loser.Players)
        {
            player.MMR += loserMmrChange;
            ((CompositePlayer)player).CompositeScore = CalculateCompositeScore(player);
        }
    }
}

/// <summary>
/// Represents a player with additional composite scoring and team history data.
/// </summary>
public class CompositePlayer(string name, int rankingPoints, int mmr, double compositeScore) 
    : Player(name, rankingPoints, mmr)
{
    /// <summary>
    /// Gets or sets the player's composite score combining various metrics.
    /// </summary>
    public double CompositeScore { get; set; } = compositeScore;
    /// <summary>
    /// Gets or sets the dictionary tracking how often this player has teamed with others.
    /// </summary>
    public Dictionary<string, int> TeamPlayerHistory { get; set; } = new();
}

/// <summary>
/// Represents a team of players with a name and scoring functionality.
/// </summary>
public class Team
{
    /// <summary>
    /// Gets or sets the list of players in the team.
    /// </summary>
    public List<Player> Players = [];

    /// <summary>
    /// Gets or sets the team's name.
    /// </summary>
    public string TeamName;

    /// <summary>
    /// Initializes a new instance of the Team class with default values.
    /// </summary>
    public Team() { }

    /// <summary>
    /// Initializes a new instance of the Team class with a specified name.
    /// </summary>
    /// <param name="name">The name of the team.</param>
    public Team(string name) : this()
    {
        TeamName = name;
    }

    /// <summary>
    /// Initializes a new instance of the Team class with players and a name.
    /// </summary>
    /// <param name="teamPlayers">The list of players in the team.</param>
    /// <param name="teamName">The name of the team.</param>
    public Team(List<Player> teamPlayers, string teamName) : this(teamName)
    {
        Players = teamPlayers;
    }

    /// <summary>
    /// Adds a player to the team.
    /// </summary>
    /// <param name="player">The player to add.</param>
    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    /// <summary>
    /// Calculates the total composite score of the team based on player ranking points.
    /// </summary>
    /// <returns>The sum of all players' ranking points.</returns>
    public int TotalTeamRankingScore()
    {
        return Players.Sum(p => p.RankingPoints);
    }
}