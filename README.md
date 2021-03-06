# Chess.com Tournament Aggregator

[![Azure DevOps][azuredevopslogo]][azuredevopslink]
[![Nuget][nugetlogo]][nugetlink]

[azuredevopslogo]: https://dev.azure.com/eduherminio/ChessComTournamentAggregator/_apis/build/status/eduherminio.ChessComTournamentAggregator?branchName=master
[azuredevopslink]: https://dev.azure.com/eduherminio/ChessComTournamentAggregator/_build/latest?definitionId=1&branchName=master
[nugetlogo]: https://img.shields.io/nuget/v/ChessComTournamentAggregator.svg?style=flat-square&label=nuget
[nugetlink]: https://www.nuget.org/packages/ChessComTournamentAggregator

**Chess.com Tournament Aggregator** is a simple command line application that uses [Chess.com API](https://www.chess.com/news/view/published-data-api) to aggregate the results of multiple tournaments and write them to a `.csv` file.

It's currently available for **Windows** (10, 8.1 and 7, for both 32 and 64 bits), **Linux** (Ubuntu, CentOS, Debian, Fedora and derivatives) and **macOS** (macOS 10.12 Sierra and above).
If you happen to use another operating system, another architecture (arm) or an older version of macOS, please open an [issue](https://github.com/eduherminio/ChessComTournamentAggregator/issues) and I'll be happy to try to help.

It's also available as a NuGet package.

## Usage

- Download the appropiated version [from here](https://github.com/eduherminio/ChessComTournamentAggregator/releases)
- Run the executable.
- Enter the tournament urls whose results you want to aggregate (one per line).
- Press double `enter` to start the process.
- If the process has concluded successfully, you should see a `.csv` file placed in the same folder as the executable.
- You can import that `.csv` file from Excel, Google Docs, OpenOffice or your favourite spreadsheet application. Please bear in mind that `;` is the separator you must select if you're given that option.

Alternatively, tournament urls or ids can be passed to the executable as parameters, i.e.:

```bash
ChessComTournamentAggregator-win-x64.exe https://www.chess.com/tournament/live/hampstead-congress-1173973 https://www.chess.com/tournament/live/chess-england-blitz-1180073
```

## FAQs

> I'm a Windows user. How do I know which version to use?

You can check that in `System Information` -> `System type`. Although if you use a reasonably new computer, it's probably going to be x64.

> I've introduced the tournament urls and nothing's happenning...

Try pressing enter again. If it still doesn't work and you get no further any other messages in the console, please fill an [issue](https://github.com/eduherminio/ChessComTournamentAggregator/issues) and I'll do my best to help you.

> I'm seeing a red message

Please read it and follow the instructions there.

> I've imported the `*.csv` file and the format seems wrong :(

Please make sure to select `;` as separator when importing the file.

---

Feel free to contact or or fill an [issue](https://github.com/eduherminio/ChessComTournamentAggregator/issues) if you're struggling to use the application or the NuGet package in any way, I'm always happy to help 😉
