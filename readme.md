# Git Timesheet

Creates a timesheet report using your git history

## Example Output

```
Commits by dave@hotmail.com
  dave@hotmail.com's commits for October
    2015-10-01 Thu 1 commits
    2015-10-02 Fri 1 commits
    2015-10-03 Sat
    2015-10-04 Sun
    2015-10-05 Mon 1 commits
    2015-10-06 Tue
    2015-10-07 Wed
    2015-10-08 Thu
    2015-10-09 Fri 1 commits
    2015-10-10 Sat
    2015-10-11 Sun
    2015-10-12 Mon 1 commits
    2015-10-13 Tue 12 commits
    2015-10-14 Wed
    2015-10-15 Thu 7 commits
    2015-10-16 Fri 2 commits
    2015-10-17 Sat 1 commits
    2015-10-18 Sun
    2015-10-19 Mon 3 commits
    2015-10-20 Tue 2 commits
    2015-10-21 Wed 1 commits
    2015-10-22 Thu 7 commits
    2015-10-23 Fri 2 commits
    2015-10-24 Sat 3 commits
    2015-10-25 Sun
    2015-10-26 Mon 5 commits
    2015-10-27 Tue 3 commits
    2015-10-28 Wed 4 commits
    2015-10-29 Thu 11 commits
    2015-10-30 Fri 3 commits
    2015-10-31 Sat
  MONTHLY TOTAL = 20 days

GRAND TOTAL = 20 days
```

## Usage

To run a report for all users for the current month:

```
$ GitTimesheet
```

To run a report for a different month:

```
$ GitTimesheet -year 2015 -month 10
```

Just report on a single user:

```
$ GitTimesheet -user dave@hotmail.com
```

## License

MIT
