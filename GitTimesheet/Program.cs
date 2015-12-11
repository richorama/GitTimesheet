using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{

    static void Main(string[] args)
    {
        var parsedArgs = ParseArgs(args).ToArray();
        var year = int.Parse(parsedArgs.GetOrDefault("-year", DateTime.UtcNow.Year.ToString()));
        var month = int.Parse(parsedArgs.GetOrDefault("-month", DateTime.UtcNow.Month.ToString()));
        var user = parsedArgs.GetOrDefault("-user", "").ToLower();

        using (var repo = new Repository(Directory.GetCurrentDirectory()))
        {
            var query = GetAllPeriods(repo, year, month, user);
            foreach (var line in FormatTimesheets(query.ToArray()))
            {
                Console.WriteLine(line);
            }
        }

        Console.ReadKey();
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
                    yield return $"  {commiter}'s commits for {new DateTime(year, month, 1).ToString("MMMM")}";

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