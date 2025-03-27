namespace TeamGeneration;

public class Player(string name, int rankingPoints, int mmr)
{
    public string Name { get; set; } = name;
    public int RankingPoints { get; set; } = rankingPoints;
    public int MMR { get; set; } = mmr;
}