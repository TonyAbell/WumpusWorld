namespace FsWeb

open System
open System.Web
open System.Web.Mvc
open System.Web.Routing
open System.Web.Http
open System.Data.Entity
open System.Web.Optimization

type BundleConfig() =
    static member RegisterBundles (bundles:BundleCollection) =
        bundles.Add(ScriptBundle("~/bundles/jquery").Include([|"~/Scripts/jquery-2*"|]))

        //bundles.Add(ScriptBundle("~/bundles/jqueryui").Include([|"~/Scripts/jquery-ui*"|]))



//        bundles.Add(ScriptBundle("~/bundles/jqueryval").Include(
//                                     [|"~/Scripts/jquery.unobtrusive*";
//                                     "~/Scripts/jquery.validate*"|]))
            
        bundles.Add(ScriptBundle("~/bundles/modernizr").Include([|"~/Scripts/modernizr-*"|]))

        bundles.Add(ScriptBundle("~/bundles/bootstrap").Include([|"~/Scripts/bootstrap*"|]))

        //bundles.Add(StyleBundle("~/Content/css").Include([|"~/Content/*.css"|]))

//        bundles.Add(StyleBundle("~/Content/themes/base/css").Include(
//                                    "~/Content/themes/base/jquery.ui.core.css",
//                                    "~/Content/themes/base/jquery.ui.resizable.css",
//                                    "~/Content/themes/base/jquery.ui.selectable.css",
//                                    "~/Content/themes/base/jquery.ui.accordion.css",
//                                    "~/Content/themes/base/jquery.ui.autocomplete.css",
//                                    "~/Content/themes/base/jquery.ui.button.css",
//                                    "~/Content/themes/base/jquery.ui.dialog.css",
//                                    "~/Content/themes/base/jquery.ui.slider.css",
//                                    "~/Content/themes/base/jquery.ui.tabs.css",
//                                    "~/Content/themes/base/jquery.ui.datepicker.css",
//                                    "~/Content/themes/base/jquery.ui.progressbar.css",
//                                    "~/Content/themes/base/jquery.ui.theme.css"))

        bundles.Add(StyleBundle("~/Content/css").Include("~/Content/bootstrap.css",
                                                         //"~/Content/site.css",
                                                               "~/Content/bootstrap-responsive.css"))

type Route = { controller : string; action : string; id : UrlParameter }
type ApiRoute = { id : RouteParameter }
type ApiMoveRoute = { session : RouteParameter; action: RouteParameter }

type Global() =
    inherit System.Web.HttpApplication() 

    static member RegisterGlobalFilters (filters:GlobalFilterCollection) =
        filters.Add(new HandleErrorAttribute())

    static member RegisterRoutes(routes:RouteCollection) =
        routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" )
        routes.MapHttpRoute( "DefaultApi", "api/{controller}/{id}", 
            { id = RouteParameter.Optional } ) |> ignore        
//        routes.MapHttpRoute( "ActionApi", "api/{controller}/{action}/{id}", 
//            { id = RouteParameter.Optional } ) |> ignore       
        routes.MapRoute("Default", "{controller}/{action}/{id}", 
            { controller = "Home"; action = "Index"; id = UrlParameter.Optional } ) |> ignore

    member this.Start() =
        GlobalConfiguration.Configuration.MapHttpAttributeRoutes()
        AreaRegistration.RegisterAllAreas()
        Global.RegisterRoutes RouteTable.Routes
        Global.RegisterGlobalFilters GlobalFilters.Filters
        BundleConfig.RegisterBundles BundleTable.Bundles
