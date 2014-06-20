
namespace GHLeaderboard

open System
open System.Collections.Generic
open System.Linq
open System.Text

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Util
open Android.Views
open Android.Widget

type LoginDialogFragment() =
  inherit DialogFragment ()

  override x.OnCreate (savedInstanceState) =
    base.OnCreate (savedInstanceState)
    // Create your fragment here


