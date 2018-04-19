﻿using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{

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


    static void Main(string[] args)
    {
        var repos = args;
        if (!args.Any())
        {
            repos = new string[] { "." };
        }

        var days = 7;

        var startDate = new DateTimeOffset(DateTime.Now.AddDays(-days).Date);
        var commits = Query(startDate, repos).ToArray();
        foreach (var summary in ToSummary(commits, startDate, days))
        {
            Console.WriteLine(summary);
        }
        Console.ResetColor();
    }



    static IEnumerable<CommitDetails> Query(DateTimeOffset startDate, params string[] repos)
    {
        foreach (var repoDir in repos)
        {
            if (!Directory.Exists(repoDir))
            {
                Console.WriteLine($"Error: {repoDir} does not exist.");
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

    static IEnumerable<string> ToSummary(IEnumerable<CommitDetails> commits, DateTimeOffset startDate, int days)
    {
        for (var i = 0; i <= days; i++)
        {
            var day = startDate.AddDays(i).Date;
            Console.ForegroundColor = System.ConsoleColor.Yellow;
            yield return $"{day:yyyy-MM-dd ddd}";
            

            var any = false;
            foreach (var commitsOnThisDay in commits.Where(x => x.Date == day).GroupBy(x => $@"{x.Repository}\{x.Branch}"))
            {

                Console.ForegroundColor = System.ConsoleColor.Cyan;
                any = true;
                yield return $"  {commitsOnThisDay.Key} : {commitsOnThisDay.Count()}";


                Console.ForegroundColor = System.ConsoleColor.Green;
                foreach (var commit in commitsOnThisDay.OrderBy(x => x.Time))
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

