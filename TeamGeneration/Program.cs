// See https://aka.ms/new-console-template for more information

using TeamGeneration;

List<Player> attendingPlayers = new List<Player>()
{
    new Player("Emil", 250, 0),
    new Player("Sara", 500, 0),
    new Player("Martin", 1000, 0),
    new Player("Lukas", 800, 0),
    new Player("Jonas", 1200, 0),
    new Player("Viktor", 800, 0),
    new Player("Johannes", 900, 0),
    new Player("Linus", 900, 0),
    new Player("Alexander", 900, 0)
};

var nrOfCourts = 1;

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
    string input = Console.ReadLine()?.ToUpper();

    // Update match result based on input
    if (input == "A")
    {
        generator.UpdateMatchResult(winner: teams[0], loser: teams[1]);
        Console.WriteLine($"{teams[0].TeamName} wins!");
    }
    else if (input == "B")
    {
        generator.UpdateMatchResult(winner: teams[1], loser: teams[0]);
        Console.WriteLine($"{teams[1].TeamName} wins!");
    }
    else
    {
        Console.WriteLine("Invalid input. Please enter 'A' or 'B'. Skipping update.");
    }

    // Handle multiple courts (optional)
    if (nrOfCourts > 1)
    {
        Console.WriteLine("\nEnter the winning team for second court (C or D): ");
        input = Console.ReadLine()?.ToUpper();

        if (input == "C")
        {
            generator.UpdateMatchResult(winner: teams[2], loser: teams[3]);
            Console.WriteLine($"{teams[2].TeamName} wins!");
        }
        else if (input == "D")
        {
            generator.UpdateMatchResult(winner: teams[3], loser: teams[2]);
            Console.WriteLine($"{teams[3].TeamName} wins!");
        }
        else
        {
            Console.WriteLine("Invalid input. Please enter 'C' or 'D'. Skipping update.");
        }
    }

    Console.WriteLine("\nPress any key to generate new teams...");
    Console.ReadKey(); // Wait for user to press a key before looping
}