using TeamGeneration;

List<Player> attendingPlayers = new()
{
//    new Player("Emil", 250, 0),
    new Player("Sara", 600, 0),
    new Player("Martin", 800, 0),
    new Player("Lukas", 800, 0),
    new Player("Jonas", 900, 0),
    //new Player("Viktor", 800, 0),
    new Player("Wilhelm", 850, 0),
    //   new Player("Johannes", 900, 0),
    new Player("Linus", 800, 0),
    new Player("Edward", 900, 0),
    new Player("Karl", 700, 0),
    new Player("Miles", 600, 0),
    new Player("Marcus", 700, 0),

    new Player("Tor", 800, 0)
//    new Player("Alexander", 900, 0)
};

var nrOfCourts = 2;

TeamsGenerator generator = new TeamsGenerator(attendingPlayers, nrOfCourts);
var compositePlayers = generator.GenerateAndSortCompositePlayers(attendingPlayers);

while (true)
{
    // Generate teams
    var teams = generator.GenerateTeams(compositePlayers, nrOfCourts);
    Console.WriteLine("\nTeams generated:");
    Console.WriteLine($"Team A: {teams[0].TeamName}");
    Console.WriteLine($"Team B: {teams[1].TeamName}");
    
    if (nrOfCourts > 1)
    {
        Console.WriteLine($"Team C: {teams[2].TeamName}");
        Console.WriteLine($"Team D: {teams[3].TeamName}");
    }

    // Wait for input
    Console.WriteLine("\nEnter the winning team (A or B): ");
    var input = Console.ReadLine()?.ToUpper();

    switch (input)
    {
        // Update match result based on input
        case "A":
            generator.UpdateMatchResult(winner: teams[0], loser: teams[1]);
            Console.WriteLine($"{teams[0].TeamName} wins!");
            break;
        case "B":
            generator.UpdateMatchResult(winner: teams[1], loser: teams[0]);
            Console.WriteLine($"{teams[1].TeamName} wins!");
            break;
        default:
            Console.WriteLine("Invalid input. Please enter 'A' or 'B'. Skipping update.");
            break;
    }

    // Handle multiple courts (optional)
    if (nrOfCourts > 1)
    {
        Console.WriteLine("\nEnter the winning team for second court (C or D): ");
        Console.ReadLine();
        input = Console.ReadLine()?.ToUpper();

        switch (input)
        {
            case "C":
                generator.UpdateMatchResult(winner: teams[2], loser: teams[3]);
                Console.WriteLine($"{teams[2].TeamName} wins!");
                break;
            case "D":
                generator.UpdateMatchResult(winner: teams[3], loser: teams[2]);
                Console.WriteLine($"{teams[3].TeamName} wins!");
                break;
            default:
                Console.WriteLine("Invalid input. Please enter 'C' or 'D'. Skipping update.");
                break;
        }
    }

    Console.WriteLine("\nPress any key to generate new teams or 'X' to stop all matches...");
    var key = Console.ReadKey(true).KeyChar; // Read single key press
    Console.WriteLine(); // New line after key press

    if (char.ToUpper(key) == 'X')
    {
        CloseMatches(compositePlayers);
        break; // Exit the loop
    }
}

// Method stub for you to implement
static void CloseMatches(List<CompositePlayer> players)
{
    players.ForEach(p => p.RankingPoints += p.MMR / 10);
    Console.WriteLine("\nMatches closed. Final player rankings:");
    foreach (var player in players)
    {
        Console.WriteLine($"Player: {player.Name}, RankingScore: {player.RankingPoints}  (MMR): {player.MMR})");
    }
}