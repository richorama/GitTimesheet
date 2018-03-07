using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{

    class CommitDetails
    {
        public CommitDetails(DateTimeOffset date, string repo, string branch)
        {
            this.Date = date.Date.Date;
            this.Repository = repo;
            this.Branch = branch;
        }
        public DateTime Date { get; set; }
        public string Repository { get; set; }
        public string Branch { get; set; }
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
    }



    static IEnumerable<CommitDetails> Query(DateTimeOffset startDate, params string[] repos)
    {
        foreach (var repoDir in repos)
        {
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
                        yield return new CommitDetails(commit.Committer.When, repoName, branch.FriendlyName);
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
            yield return $"{day:yyyy-MM-dd ddd}";

            var any = false;
            foreach (var commitsOnThisDay in commits.Where(x => x.Date == day).GroupBy(x => $@"{x.Repository}\{x.Branch}"))
            {
                any = true;
                yield return $"  {commitsOnThisDay.Key} : {commitsOnThisDay.Count()}";
            }

            if (!any)
            {
                yield return "  --none--";
            }
            yield return "";

        }


    }



}

