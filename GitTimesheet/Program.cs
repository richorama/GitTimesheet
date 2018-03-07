﻿using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{

    static void Main(string[] vargs)
    {
        var args = vargs.Select(x => x.ToLower()).ToArray();

        if (args.Contains("-help") || args.Contains("--help") || args.Contains("-h") || args.Contains("-?") || args.Contains("/?"))
        {
            Console.WriteLine(@"
GitTimesheet

Optional arguments:
    
    -month 12              (shows activity in December)
    -year 2017             (shows activity in 2017)
    -user dave@hotmail.com (shows activity of a single user)

By default, all activity for the current month is shown
");
            return;
        }
        

        var parsedArgs = ParseArgs(args).ToArray();
        var year = int.Parse(parsedArgs.GetOrDefault("-year", DateTime.UtcNow.Year.ToString()));
        var month = int.Parse(parsedArgs.GetOrDefault("-month", DateTime.UtcNow.Month.ToString()));
        var days = int.Parse(parsedArgs.GetOrDefault("-days", "0"));
        var user = parsedArgs.GetOrDefault("-user", "").ToLower();
        var repos = parsedArgs.GetOrDefault("-dir", ".").Split(',').Select(x => x.Trim()).ToArray();
        var format = parsedArgs.GetOrDefault("-format", "report");


        foreach (var repoDir in repos)
        {
            if (repos.Length > 1) Console.WriteLine(repoDir);

            using (var repo = new Repository(repoDir))
            {
                if (user == "me")
                {
                    var userConfigValue = repo.Config.FirstOrDefault(x => x.Key == "user.email");
                    if (null != userConfigValue) user = userConfigValue.Value;
                }

                var query = days > 0 ? GetLastNDays(repo, days, user) : GetAllPeriods(repo, year, month, user);

                if (format == "report")
                {

                    foreach (var line in FormatTimesheets(query.ToArray()))
                    {
                        Console.WriteLine(line);
                    }
                }
                else
                {
                    foreach (var line in FormatSummary(query.ToArray()))
                    {
                        Console.WriteLine(line);
                    }
                }
            }
        }
    }

    static IEnumerable<KeyValuePair<string, string>> ParseArgs(string[] args)
    {
        for (var i = 0; i < args.Length-1; i+=2)
        {
            yield return new KeyValuePair<string, string>(args[i], args[i + 1]);
        }
    }


    static IEnumerable<Commit> GetAllPeriods(Repository repo, int year = -1, int month = -1, string userEmail = null)
    {
        IEnumerable<Commit> query = repo.Commits;

        if (!string.IsNullOrWhiteSpace(userEmail)) query = query.Where(x => x.Committer.Email == userEmail);
        if (year != -1) query = query.Where(x => x.Committer.When.ToLocalTime().Year == year);
        if (month != -1) query = query.Where(x => x.Committer.When.ToLocalTime().Month == month);

        return query;
    }


    static IEnumerable<Commit> GetLastNDays(Repository repo, int days, string userEmail = null)
    {
        IEnumerable<Commit> query = repo.Commits;
        var startDate = DateTimeOffset.Now.AddDays(-days); ;
        if (!string.IsNullOrWhiteSpace(userEmail)) query = query.Where(x => x.Committer.Email == userEmail);
        query = query.Where(x => x.Committer.When.ToLocalTime() > startDate);

        return query;
    }


    static IEnumerable<string> FormatSummary(Commit[] commits)
    {
        var commiters = commits.Select(x => x.Committer.Email.ToLower()).Distinct().ToArray();
        foreach (var commiter in commiters)
        {
            yield return $"Commits by {commiter}";
            var subsetByCommitter = commits.Where(x => x.Committer.Email.ToLower() == commiter).ToArray();
            foreach (var commitsByDay in subsetByCommitter.GroupBy(x => x.Committer.When.ToString("yyyy-MM-dd ddd")))
            {
                yield return $"Commits on {commitsByDay.Key}";
                foreach (var commit in commitsByDay)
                {
                    yield return $"  {commit.MessageShort}";
                }

            }
        }
    }

    static IEnumerable<string> FormatTimesheets(Commit[] commits)
    {
        var grandTotal = 0;
        var commiters = commits.Select(x => x.Committer.Email.ToLower()).Distinct().ToArray();
        foreach (var commiter in commiters)
        {
            yield return $"Commits by {commiter}";
            var subsetByCommitter = commits.Where(x => x.Committer.Email.ToLower() == commiter).ToArray();
            foreach (var year in subsetByCommitter.Select(x => x.Committer.When.Year).Distinct().OrderBy(x => x))
            {
                var subsetByYear = subsetByCommitter.Where(x => x.Committer.When.Year == year).ToArray();
                foreach (var month in subsetByYear.Where(x => x.Committer.When.Year == year).Select(x => x.Committer.When.Month).Distinct().OrderBy(x => x))
                {
                    var subsetByMonth = subsetByYear.Where(x => x.Committer.When.Month == month).ToArray();
                    yield return $"  {commiter}'s commits for {new DateTime(year, month, 1).ToString("MMMM")} {year}";

                    var count = 0;
                    // we're at a month for a given commiter
                    foreach (var day in Enumerable.Range(1, DateTime.DaysInMonth(year, month)))
                    {
                        var date = new DateTime(year, month, day);
                        var dailyCommitCount = subsetByMonth.Count(x => x.Committer.When.DayOfYear == date.DayOfYear);
                        var message = dailyCommitCount > 0 ? $"{dailyCommitCount} commits" : "";
                        if (dailyCommitCount > 0)
                        {
                            count++;
                            grandTotal++;
                        }
                        yield return $"    {date.ToString("yyyy-MM-dd ddd")} {message}";
                    }
                    yield return $"  MONTHLY TOTAL = {count} days";
                    yield return "";

                }
            }
        }
        yield return $"GRAND TOTAL = {grandTotal} days";
    }


}

public static class Extensions
{
    public static string GetOrDefault(this KeyValuePair<string, string>[] values, string key, string defaultValue)
    {
        if (!values.Any(x => x.Key == key)) return defaultValue;
        return values.First(x => x.Key == key).Value;
    }
}
