
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
open Android.Graphics
open Android.Graphics.Drawables

type AvatarDrawable (bitmap: Bitmap) as this =
    inherit BitmapDrawable (bitmap)

    let avatarPaint = lazy (
        let shader = new BitmapShader(this.Bitmap,Shader.TileMode.Clamp, Shader.TileMode.Clamp)
        let paint = new Paint(AntiAlias=true)
        let shader = new BitmapShader(this.Bitmap,Shader.TileMode.Clamp, Shader.TileMode.Clamp)
        let matrix = new Matrix()
        matrix.SetScale(float32 (this.Bounds.Width()) / float32 this.Bitmap.Width, float32 (this.Bounds.Height()) / float32 this.Bitmap.Height, 0.0f, 0.0f)
        shader.SetLocalMatrix(matrix)
        paint.SetShader(shader) |> ignore
        paint
    )
    let avatarStroke = lazy (new Paint(Color=Color.Rgb(220, 220, 220), AntiAlias=true))
    let avatarShadow = lazy (new Paint(Color=Color.Argb(60, 0, 0, 0), AntiAlias=true))

    override this.Draw (canvas) =
        let hwidth = float32 (this.Bounds.Width() / 2)
        let hheight = float32 (this.Bounds.Height() / 2)
        let radius = hwidth - 3.0f
        let delta = 3.0f
        canvas.DrawCircle(hwidth, hheight + 2.0f*delta/3.0f, radius, avatarShadow.Force())
        canvas.DrawCircle(hwidth, hheight, radius, avatarStroke.Force())
        canvas.DrawCircle(hwidth, hheight, radius-delta, avatarPaint.Force())
