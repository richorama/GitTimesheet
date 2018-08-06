[![Build status](https://ci.appveyor.com/api/projects/status/8mmlr37e0khup7wc/branch/master?svg=true)](https://ci.appveyor.com/project/richorama/gittimesheet/branch/master)

# Git Timesheet

Creates a timesheet report using your git history

## Example Output

```
2018-07-30 Mon
  --none--

2018-07-31 Tue
  git-timesheet/master : 2 commits
    13:32 => upgrade to latest package version
    13:34 => switch to netcore

2018-08-01 Wed
  git-timesheet/master : 1 commit
    13:08 => implement a max number of commits to show

2018-08-02 Thu
  --none--

2018-08-03 Fri
  --none--

2018-08-04 Sat
  --none--

2018-08-05 Sun
  --none--
```

## Usage

To run a report for your user for the last 7 days in the current repo:

```
$ git-timesheet
```

To run a report for multiple repos:

```
$ git-timesheet path\to\repo1 path\to\repo2
```

## Building the Application

You can use the .net CLI to build the source code:

```
$ dotnet build
```

You can then publish a binary for your OS:

```
@dotnet publish -c Release -r win10-x64 
```

## License

MIT
