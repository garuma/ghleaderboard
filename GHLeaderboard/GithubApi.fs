namespace GHLeaderboard

open System
open FSharp.Data

module GitHubApi =
    let accessToken = "..."

    type GitHubEntity =
        | User of string
        | Organization of string
    
    type GHUserStats = {
        Name: string;
        Avatar: Uri;
        CommitCount : int;
        Additions: int;
        Removals: int;
    }

    type GitHubStats = JsonProvider<"https://api.github.com/repos/mono/monodevelop/stats/contributors">
    type GitHubProjects = JsonProvider<"https://api.github.com/users/mono/repos">
    type GHStatsUrl = string
    type GHProjectsUrl = string
    type GHUserToken = string option

    let grabTokenUrlFragment (accessToken: GHUserToken) =
        match accessToken with
        | None -> String.Empty
        | Some token -> "?access_token=" + token

    let makeStatsUrl accessToken organization repository : GHStatsUrl =
        sprintf "https://api.github.com/repos/%s/%s/stats/contributors%s" organization repository (grabTokenUrlFragment accessToken)
    
    let fetchStats (url: GHStatsUrl) =
        async {
            let! rawStats = GitHubStats.AsyncLoad(url)
            let stats =
                rawStats
                |> Seq.where (fun s -> s.Total > 0 && (Seq.last s.Weeks).C > 0)
                |> Seq.sortBy (fun s -> -(Seq.last s.Weeks).C)
                |> Seq.truncate 20
            return [
                for stat in stats ->
                    {
                        Name = stat.Author.Login;
                        Avatar = Uri stat.Author.AvatarUrl;
                        CommitCount = (Seq.last stat.Weeks).C;
                        Additions = (Seq.last stat.Weeks).A;
                        Removals = (Seq.last stat.Weeks).D;
                    }
            ]
        }

    let makeProjectsUrl accessToken entity : GHProjectsUrl =
        match entity with
        | User user -> sprintf "https://api.github.com/users/%s/repos%s" user (grabTokenUrlFragment accessToken)
        | Organization org -> sprintf "https://api.github.com/orgs/%s/repos%s" org (grabTokenUrlFragment accessToken)
    
    let fetchProjectsNamesFor (entity : GHProjectsUrl) =
        async {
            let! projects = GitHubProjects.AsyncLoad(entity)
            return [for project in projects -> project.Name]
        }