[![Build status](https://ci.appveyor.com/api/projects/status/8mmlr37e0khup7wc/branch/master?svg=true)](https://ci.appveyor.com/project/richorama/gittimesheet/branch/master)

# Git Timesheet

Creates a timesheet report using your git history

## Example Outputs

### `-format summary`

```shell
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

---

### `-format json`

```json
{
  "timesheets": [
    {
      "startDate": "2020-06-20T00:00:00-04:00",
      "durations": {
        "hours": "0.03",
        "minutes": "1.88",
        "seconds": "113.00"
      },
      "commits": [
        {
          "date": "2020-06-20T00:00:00",
          "time": "2020-06-20T10:41:34",
          "repository": "{{ REPO_NAME }}",
          "branch": "{{ BRANCH_NAME }}",
          "message": "{{ COMMIT_MESSAGE }}"
        },
        {
          "date": "2020-06-20T00:00:00",
          "time": "2020-06-20T10:43:27",
          "repository": "{{ REPO_NAME }}",
          "branch": "{{ BRANCH_NAME }}",
          "message": "{{ COMMIT_MESSAGE }}"
        }
      ]
    }
  ],
  "meta": {
    "days": 5,
    "maxNumberOfCommitsToDisplay": 50
  }
}
```

#### Particularities
The `json` format is less-opinionated in the output and provides some helpful data precomputed for ease of use in a system the would render the data differently.

#### Run configuration Metadata
The `meta` key contains information about the some parameters that were passed to generate the `json` file output.

#### Pre-computed Durations
The `timesheets[i].durations` object contains some useful precomputed values in `hours`, `minutes` and `seconds` representing the duration of time between the ***first*** and ***last*** commit as returned by the passed arguments.

> **Note:**
>
> Please keep in mind that these pre-computed values **will vary** depending on the value used for the `max` command-line argument since they are computed relative to what is shown and not the actual complete state of the Git repository.
>
>For a more comprehensive view, one should choose a sensible value for the `max` argument to include a larger amount of commits in a given day than the default value.

## Usage

To run a report for your user for the last 7 days in the current repo:

```shell
$ git-timesheet
```

To run a report for multiple repos:

```shell
$ git-timesheet path\to\repo1 path\to\repo2
```

## Advanced Usage

To output a report as `json`, going back `10` days, with a maximum of `50` commits per day:

```shell
$ git-timesheet path\to\repo1 path\to\repo2 -format json -days 10 -max 50
```

On most operating systems, one can redirect the output into a file as follows:

```shell
$ git-timesheet path\to\repo1 path\to\repo2 -format json -days 10 -max 50 > path\for\output\timesheets_repo1_repo2_10days_50commits.json
```

## Building the Application

You can use the .net CLI to build the source code:

```shell
$ dotnet build
```

You can then publish a binary for your OS:

#### Windows

```shell
@dotnet publish -c Release -r win10-x64 
```

#### Linux

```shell
@dotnet publish -c Release -r linux-x64 
```

## License

MIT
