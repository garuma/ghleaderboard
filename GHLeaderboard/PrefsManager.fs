
namespace GHLeaderboard

open System
open System.Collections.Generic
open System.Linq
open System.Text

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget

module PrefsManager =

    let getPrefs (context: Context) = context.GetSharedPreferences("GHLeaderboard", FileCreationMode.Private)

    let getUserToken context =
        let prefs = getPrefs context
        let token = prefs.GetString("token", null)
        match token with
        | null -> None
        | _ -> Some token

    let setUserToken context token =
        let prefs = getPrefs context
        let editor = prefs.Edit()
        editor.PutString("token", token) |> ignore
        editor.Commit() |> ignore
