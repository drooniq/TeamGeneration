// See https://aka.ms/new-console-template for more information

using TeamGeneration;

List<Player> attendingPlayers = new List<Player>()
{
    new Player("Emil", 250, 1000),    // Beginner-level player
    new Player("Sophie", 800, 1000),  // Intermediate player
    new Player("Liam", 1500, 1000),   // Strong player
    new Player("Ava", 600, 1000),     // Lower intermediate
    new Player("Noah", 1800, 1000),   // Near-elite player
    new Player("Olivia", 400, 1000),  // Casual player
    new Player("Jackson", 1200, 1000) // Solid mid-tier player
};

TeamsGenerator generator = new TeamsGenerator(attendingPlayers, 1);
var compositePlayers = generator.GenerateAndSortCompositePlayers(attendingPlayers);
//var teams = generator.GenerateTeams(compositePlayers, 1);

