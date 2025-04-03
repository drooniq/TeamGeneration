# Sports Team Generator

This project generates sport teams based on a combination of key factors: **Ranking Points**, **MMR**, and **Penalty**.

## Key Features

- **Ranking Points**: Players begin with an initial skill-based ranking score.
- **MMR (Matchmaking Rating)**: A dynamic rating that adjusts after each match in a `GameEvent` (e.g., a group of friends playing volleyball across multiple matches), reflecting player performance.
- **Penalty**: Ensures variety by discouraging the same players from being paired together repeatedly.

## How It Works

To create teams, the algorithm takes:
- A roster of players.
- The number of available courts.

Additionally, a built-in class generates random team names for a fun and engaging experience.

## Use Case

Perfect for organizing casual sports events, like a volleyball game night with friends, where balanced and varied teams enhance the fun!
