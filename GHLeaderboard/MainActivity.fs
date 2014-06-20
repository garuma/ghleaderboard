namespace GHLeaderboard

open System

open Android.App
open Android.Content
open Android.OS
open Android.Runtime
open Android.Views
open Android.Widget
open Android.Graphics
open Android.Support.V4.App
open Android.Support.V4.Widget

[<Activity (Label = "GitHub Leaderboard", MainLauncher = true, Theme = "@style/LeaderboardActionBarTheme")>]
type MainActivity () =
    inherit Android.Support.V4.App.FragmentActivity ()

    [<DefaultValue>]
    val mutable drawerToggle : ActionBarDrawerToggle

    override this.OnCreate (bundle) =

        base.OnCreate (bundle)

        this.SetContentView (Resource_Layout.Main)

        let avatarImage = this.FindViewById<ImageView>(Resource_Id.userAvatar)
        let avatarDrawable = new AvatarDrawable(BitmapFactory.DecodeResource(this.Resources, Resource_Drawable.avatar))
        avatarImage.SetImageDrawable(avatarDrawable)

        let drawer = this.FindViewById<DrawerLayout>(Resource_Id.drawer_layout)
        this.drawerToggle <- new ActionBarDrawerToggle(this, drawer, Resource_Drawable.ic_drawer, Resource_String.open_drawer, Resource_String.close_drawer)
        drawer.SetDrawerShadow(Resource_Drawable.drawer_shadow, int GravityFlags.Left)
        drawer.SetDrawerListener(this.drawerToggle)
        this.ActionBar.SetDisplayHomeAsUpEnabled(true)
        this.ActionBar.SetHomeButtonEnabled(true)

        let syncContext = System.Threading.SynchronizationContext.Current
        let menuList = this.FindViewById<ListView>(Resource_Id.left_drawer)
        let fillProjectMenu =
            async {
                let url = GitHubApi.makeProjectsUrl (PrefsManager.getUserToken this) (GitHubApi.Organization "mono")
                let! projects = GitHubApi.fetchProjectsNamesFor url
                do! Async.SwitchToContext syncContext
                let projectAdapter = new ArrayAdapter(this, Resource_Layout.MenuItemLayout, List.toArray projects)
                menuList.Adapter <- projectAdapter
            }
        Async.Start fillProjectMenu

        let statsFragment = new StatsFragment(this)
        this.SupportFragmentManager.BeginTransaction().Add(Resource_Id.content_frame, statsFragment).Commit () |> ignore

    override this.OnPostCreate (bundle) =
        base.OnPostCreate(bundle)
        this.drawerToggle.SyncState()

    override this.OnOptionsItemSelected (item) =
        this.drawerToggle.OnOptionsItemSelected(item)
