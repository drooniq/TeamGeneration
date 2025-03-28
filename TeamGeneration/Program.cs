﻿// See https://aka.ms/new-console-template for more information

using TeamGeneration;

List<Player> attendingPlayers = new List<Player>()
{
    new Player("Emil", 250, 1500),    // Beginner-level player
    new Player("Sophie", 800, 1500),  // Intermediate player
    new Player("Liam", 1500, 1500),   // Strong player
    new Player("Ava", 600, 1500),     // Lower intermediate
    new Player("Noah", 1800, 1500),   // Near-elite player
    new Player("Olivia", 400, 1500),  // Casual player
    new Player("Jackson", 1200, 1500) // Solid mid-tier player
};

TeamsGenerator generator = new TeamsGenerator(attendingPlayers, 2);

//var teams = generator.GenerateTeams();
