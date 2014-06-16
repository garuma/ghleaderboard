
namespace GHLeaderboard

open System
open System.Collections.Generic
open System.Linq
open System.Text
open System.Net

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Util
open Android.Views
open Android.Widget
open Android.Graphics

type StatsAdapter (context: Context) =
    inherit BaseAdapter ()

    let synchronizationContext = lazy (System.Threading.SynchronizationContext.Current)
    let mutable backingStore : GitHubApi.GHUserStats array = Array.empty

    member this.LoadItemsAsync (entity, repository) =
        async {
            let url = GitHubApi.makeStatsUrl entity repository
            let! stats = GitHubApi.fetchStats url
            backingStore <- List.toArray stats
        }

    override this.Count = Array.length backingStore

    override this.GetItem (position) =
        new Java.Lang.Object()
      
    override this.GetItemId (position) =
        int64 position
   
    override this.AreAllItemsEnabled () = false
    override this.IsEnabled (position) = false
    override this.HasStableIds = false
    
    override this.GetView (position, convertView, parent) =
        let view =
            match convertView with
            | null -> LayoutInflater.From(parent.Context).Inflate(Resource_Layout.LeaderBoardItem, parent, false)
            | _ -> convertView
        let stat = backingStore.[position]
        List.zip
            [ Resource_Id.commitCount; Resource_Id.additions; Resource_Id.removals ]
            [ stat.CommitCount; stat.Additions; stat.Removals ]
        |> List.iter (fun (id, count) -> view.FindViewById<TextView>(id).Text <- string count)

        let avatarView = view.FindViewById<ImageView>(Resource_Id.userAvatar)
        avatarView.Visibility <- ViewStates.Invisible
        let sync = synchronizationContext.Force()
        let loadAvatar = async {
            let request = WebRequest.CreateHttp stat.Avatar
            let! response = request.AsyncGetResponse()
            let bmp = BitmapFactory.DecodeStream(response.GetResponseStream())
            do! Async.SwitchToContext sync
            avatarView.SetImageDrawable(new AvatarDrawable(bmp))
            avatarView.Visibility <- ViewStates.Visible
        }
        Async.Start loadAvatar

        let placement = view.FindViewById<TextView>(Resource_Id.placement)
        placement.Text <- string (position+1)
        let baseSize = parent.Resources.GetDimension(Resource_Dimension.score_text_size)
        let multiplier =
            match position with
            | 0 -> 1.7f
            | 1 -> 1.5f
            | 2 -> 1.3f
            | _ -> 1.0f
        placement.SetTextSize(ComplexUnitType.Px, (baseSize * multiplier))

        view.FindViewById<TextView>(Resource_Id.username).Text <- stat.Name

        view

type StatsFragment(context) =
  inherit Android.Support.V4.App.ListFragment ()

  let adapter = new StatsAdapter(context)

  override x.OnCreate (savedInstanceState) =
    base.OnCreate (savedInstanceState)
    let syncContext = System.Threading.SynchronizationContext.Current
    let loadItems =
        async {
            do! adapter.LoadItemsAsync("mono", "mono")
            do! Async.SwitchToContext syncContext
            x.ListAdapter <- adapter
        }
    Async.Start loadItems

  override x.OnViewCreated (view, savedInstanceState) =
    base.OnViewCreated(view, savedInstanceState)
    let metrics = view.Context.Resources.DisplayMetrics
    let padding = int (TypedValue.ApplyDimension (ComplexUnitType.Dip, 8.0f, metrics))
    x.ListView.SetPadding(padding, padding, padding, padding)
    x.ListView.SetClipToPadding (false)
    x.ListView.DividerHeight <- int (TypedValue.ApplyDimension (ComplexUnitType.Dip, 8.0f, metrics))
    x.ListView.ScrollBarStyle <- ScrollbarStyles.OutsideOverlay
