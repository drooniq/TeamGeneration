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
        _attendingPlayers = GenerateCompositePlayers(attendingPlayers); // calculate composite values for each player
        _teams = GenerateTeams(_attendingPlayers, _numberOfCourts);     // create initial teams based upon nrOfCourts
    }
    
    public List<Team> GenerateTeams(List<CompositePlayer>? players, int? numberOfCourts )
    {
        List<Team> teams = [];
        
        if (numberOfCourts == null || players == null)
            return teams;
        
        // create balanced teams

        return teams;
    }
    
    private List<CompositePlayer> GenerateCompositePlayers(List<Player> _attendingPlayers)
    {
        List<CompositePlayer> compositePlayers = new();

        foreach (var player in _attendingPlayers)
        {
            var playerScore = CalculateCompositeScore(player);
            compositePlayers.Add(new CompositePlayer(player, playerScore));
        }
        
        return compositePlayers;
    }    

    private double CalculateCompositeScore(Player player)
    {
        // (w1 * RPnorm) + (w2 * MMRnorm)
        var rpNorm = player.RankingPoints / 2000.0;     // Normalize RP (0-2000)
        var mmrNorm = (player.MMR - 1000) / 1000.0;     // Normalize MMR (1000-2000)
        var compositeScore =  ((double)NormWeights.RP / 100.0) * rpNorm +
                                    ((double)NormWeights.MMR / 100.0) * mmrNorm;
        
        return compositeScore; 
    }
}

public class CompositePlayer(Player player, double compositeScore)
{
    public Player? Player = player;
    public double CompositeScore = compositeScore;
    public Dictionary<Player, int> TeamPlayerHistory = new();
}

public class Team(List<Player> teamPlayers, string teamName)
{
    public List<Player> Players = teamPlayers;
    public string TeamName = teamName;
}