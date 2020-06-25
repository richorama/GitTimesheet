using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

class Program
{
    #region Commit Data

    class CommitDetails
    {
        public CommitDetails(DateTimeOffset date, string repo, string branch, string message)
        {
            this.Date = date.Date.Date;
            this.Time = date.DateTime;
            this.Repository = repo;
            this.Branch = branch;
            this.Message = message;
        }

        public DateTime Date { get; private set; }
        public DateTime Time { get; private set; }
        public string Repository { get; private set; }
        public string Branch { get; private set; }
        public string Message { get; private set; }
    }

    #endregion

    #region Json Serialization Data

    class CommitTimeSheets
    {
        public class MetaData
        {
            public int Days { get; set; }
            public int MaxNumberOfCommitsToDisplay { get; set; }
        }

        public List<CommitTimeSheet> Timesheets { get; set; }
        public MetaData Meta { get; set; }
    }

    class CommitTimeSheet
    {
        public class TimeSheetDurations
        {
            public string Hours { get; set; }
            public string Minutes { get; set; }
            public string Seconds { get; set; }
        }

        public DateTimeOffset StartDate { get; set; }

        public TimeSheetDurations Durations { get; set; }
        public List<CommitDetails> Commits { get; set; }
    }

    #endregion

    static void Main(string[] args)
    {
        var repos = args.GetDirectoryNames().ToArray();
        if (!args.Any())
        {
            repos = new string[]
            {
                "."
            };
        }

        var parsedArgs = args.ParseArgs().ToArray();
        var days = int.Parse(parsedArgs.GetOrDefault("-days", "7"));
        var max = int.Parse(parsedArgs.GetOrDefault("-max", "10"));
        var format = parsedArgs.GetOrDefault("-format", "summary");

        var startDate = new DateTimeOffset(DateTime.Now.AddDays(-days).Date);
        var commits = Query(startDate, repos).ToArray();

        switch (format)
        {
            case "summary":
                PrintSummary(commits, startDate, days, max);
                break;

            case "json":
                PrintJson(commits, startDate, days, max);
                break;

            default:
                throw new ArgumentException($"Unhandled format '{format}' was used. Supported formats are: 'summary' and 'json'.");
        }

        Console.ResetColor();
    }

    static void PrintSummary(IEnumerable<CommitDetails> commits, DateTimeOffset startDate, int days, int maximumNumberOfCommitsToDisplay)
    {
        foreach (var summary in ToSummary(commits, startDate, days, maximumNumberOfCommitsToDisplay))
        {
            Console.WriteLine(summary);
        }
    }
    
    static void PrintJson(IEnumerable<CommitDetails> commits, DateTimeOffset startDate, int days, int maximumNumberOfCommitsToDisplay)
    {
        // JSON Serialization Options
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        // Data structure that will be serialized and outputted in the end
        var timesheets = new CommitTimeSheets
        {
            Timesheets = new List<CommitTimeSheet>(),
            Meta = new CommitTimeSheets.MetaData
            {
                Days = days,
                MaxNumberOfCommitsToDisplay = maximumNumberOfCommitsToDisplay
            }
        };

        var commitDetails = commits as CommitDetails[] ?? commits.ToArray();

        for (var i = 0; i <= days; i++)
        {
            var day = startDate.AddDays(i).Date;

            var summary = new CommitTimeSheet
            {
                StartDate = day,
                Commits = new List<CommitDetails>(),
            };

            var dailyCommitsGroup = commitDetails.Where(x => x.Date == day).GroupBy(x => $@"{x.Repository}/{x.Branch}");
            foreach (var commitsOnThisDay in dailyCommitsGroup)
            {
                foreach (var commit in commitsOnThisDay.OrderBy(x => x.Time).Take(maximumNumberOfCommitsToDisplay))
                {
                    summary.Commits.Add(commit);
                }
            }

            // Skip days without commits
            if (summary.Commits.Count > 0)
            {
                // Compute duration between first and last commit
                var first = summary.Commits.First();
                var last = summary.Commits.Last();
                var diff = last.Time - first.Time;

                // Save duration in various helpful formats
                summary.Durations = new CommitTimeSheet.TimeSheetDurations
                {
                    Hours = diff.TotalHours.ToString("F"),
                    Minutes = diff.TotalMinutes.ToString("F"),
                    Seconds = diff.TotalSeconds.ToString("F"),
                };
                
                // Save to list for later serialization
                timesheets.Timesheets.Add(summary);
            }
        }

        // Make it so!
        var json = JsonSerializer.Serialize(timesheets, options);

        // Write to stdout
        Console.Write(json);
    }

    static IEnumerable<CommitDetails> Query(DateTimeOffset startDate, params string[] repos)
    {
        foreach (var repoDir in repos)
        {
            if (!Directory.Exists(repoDir))
            {
                Console.WriteLine($"Error: {repoDir} does not exist.");
                yield break;
            }

            var repoName = Directory.GetParent(Path.Combine(repoDir, ".git")).Name;

            using (var repo = new Repository(repoDir))
            {
                var user = "";
                var userConfigValue = repo.Config.FirstOrDefault(x => x.Key == "user.email");
                if (null != userConfigValue) user = userConfigValue.Value;

                foreach (var branch in repo.Branches.Where(x => x.IsTracking))
                {
                    // out of date branch
                    if (branch.Tip.Committer.When < startDate) continue;
                    foreach (var commit in branch.Commits.Where(x => x.Committer.When > startDate && x.Committer.Email == user))
                    {
                        yield return new CommitDetails(commit.Committer.When, repoName, branch.FriendlyName, commit.MessageShort);
                    }
                }
            }
        }
    }

    static IEnumerable<string> ToSummary(IEnumerable<CommitDetails> commits, DateTimeOffset startDate, int days, int maximumNumberOfCommitsToDisplay)
    {
        for (var i = 0; i <= days; i++)
        {
            var day = startDate.AddDays(i).Date;
            Console.ForegroundColor = System.ConsoleColor.Yellow;
            yield return $"{day:yyyy-MM-dd ddd}";

            var any = false;
            foreach (var commitsOnThisDay in commits.Where(x => x.Date == day).GroupBy(x => $@"{x.Repository}/{x.Branch}"))
            {
                Console.ForegroundColor = System.ConsoleColor.Cyan;
                any = true;
                yield return $"  {commitsOnThisDay.Key} : {commitsOnThisDay.Count()} {(commitsOnThisDay.Count() == 1 ? "commit" : "commits")}";

                Console.ForegroundColor = System.ConsoleColor.Green;
                foreach (var commit in commitsOnThisDay.OrderBy(x => x.Time).Take(maximumNumberOfCommitsToDisplay))
                {
                    yield return $"    {commit.Time:HH:mm} => {commit.Message}";
                }
            }

            if (!any)
            {
                Console.ForegroundColor = System.ConsoleColor.Gray;
                yield return "  --none--";
            }

            yield return "";
        }
    }
}

public static class Extensions
{
    public static string GetOrDefault(this KeyValuePair<string, string>[] values, string key, string defaultValue)
    {
        if (!values.Any(x => x.Key == key)) return defaultValue;
        return values.First(x => x.Key == key).Value;
    }

    public static IEnumerable<string> GetDirectoryNames(this string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].StartsWith("-")) continue;
            if (i > 0 && args[i - 1].StartsWith("-")) continue;
            yield return args[i];
        }
    }

    public static IEnumerable<KeyValuePair<string, string>> ParseArgs(this string[] args)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i].StartsWith("-")) yield return new KeyValuePair<string, string>(args[i], args[i + 1]);
        }
    }
}