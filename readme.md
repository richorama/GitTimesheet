[![Build status](https://ci.appveyor.com/api/projects/status/8mmlr37e0khup7wc/branch/master?svg=true)](https://ci.appveyor.com/project/richorama/gittimesheet/branch/master)

# Git Timesheet

Creates a timesheet report using your git history

## Example Output

```
2018-02-28 Wed
  project1\dev : 2

2018-03-01 Thu
  project1\master : 5

2018-03-02 Fri
  project1\test : 2

2018-03-03 Sat
  --none--

2018-03-04 Sun
  --none--

2018-03-05 Mon
  project1\master : 4

2018-03-06 Tue
  project1\master : 3
  project1\test : 1

2018-03-07 Wed
  --none--
```

## Usage

To run a report for your user for the last 7 days in the current repo:

```
$ GitTimesheet
```

To run a report for multiple repos:

```
$ GitTimesheet path\to\repo1 path\to\repo2
```


## License

MIT
