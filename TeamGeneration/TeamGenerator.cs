using System.Text.Json;

namespace TeamGeneration;

public class TeamsGenerator()
{
    private List<CompositePlayer>? _attendingPlayers;
    private int? _numberOfCourts;
    private readonly Random _random = new();
    private List<Team>? _teams;

    public TeamsGenerator(List<Player> attendingPlayers, int numberOfCourts) : this()
    {
        _numberOfCourts = numberOfCourts;
        _attendingPlayers = GenerateAndSortCompositePlayers(attendingPlayers);  // calculate composite values for each player
        _teams = GenerateTeams(_attendingPlayers, _numberOfCourts);             // create initial teams based upon nrOfCourts
    }
    
    
    
    public List<Team> GenerateTeams(List<CompositePlayer>? players = null, int? numberOfCourts = null )
    {
        List<Team> teams = [];
        
        if (numberOfCourts == null || players == null)
            return teams;

        var teamSizesList = CalculateTeamSize(players.Count(), _numberOfCourts!.Value);

        //var value = (int)PenaltyWeight.Penalty / 100.0;

        foreach (var player in players)
        {
            Console.WriteLine($"{player.CompositeScore:F3} => {player.Player!.Name}");
        }

        return teams;
    }

    private List<int> CalculateTeamSize(int numberOfPlayers, int numberOfCourts)
    {
        var numberOfTeams = 2 * numberOfCourts;
        // Cap teams at player count if insufficient
        numberOfTeams = Math.Min(numberOfTeams, numberOfPlayers);
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
        List<CompositePlayer> compositePlayers = new();

        foreach (var player in attendingPlayers)
        {
            var playerScore = CalculateCompositeScore(player);
            compositePlayers.Add(new CompositePlayer(player, playerScore));
        }

        // sort descending b vs a instead of a vs b ascending
        compositePlayers.Sort((a, b) => b.CompareTo(a));
        
        return compositePlayers;
    }    
    
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

public class CompositePlayer(Player player, double compositeScore) : IComparable<CompositePlayer>
{
    public Player? Player = player;
    public double CompositeScore = compositeScore;
    public Dictionary<Player, int> TeamPlayerHistory = new();
    
    public int CompareTo(CompositePlayer? other)
    {
        if (other == null) return 1;
        return this.CompositeScore.CompareTo(other.CompositeScore);
    }
}

public class Team(List<Player> teamPlayers, string teamName)
{
    public List<Player> Players = teamPlayers;
    public string TeamName = teamName;
}