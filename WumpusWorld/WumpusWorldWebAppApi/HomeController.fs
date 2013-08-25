namespace FsWeb.Controllers

open System.Web
open System.Web.Mvc
open System.Collections.Generic
open System.Security.Claims



open Microsoft.Owin.Security

[<HandleError>]
[<AllowAnonymous>]
type HomeController() =
    inherit Controller()
    member this.Index () =
        this.View() :> ActionResult
