# Cyber Defense Scoring Engine

This provides the baseline of a scoring engine which allows for management of instances and teams as well as their individual scores.

## Development setup

The following programs are needed for development:

- Node.js 14
- .NET 5 SDK
- Either Visual Studio or Visual Studio Code

From the project root, run the following command:

```
dotnet restore EngineController/EngineController.csproj
```

After making changes, run `dotnet run --project EngineController/EngineController.csproj` to build and run the code
