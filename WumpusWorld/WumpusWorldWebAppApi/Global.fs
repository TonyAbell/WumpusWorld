namespace FsWeb

open System
open System.Web
open System.Web.Mvc
open System.Web.Routing
open System.Web.Http
open System.Data.Entity
open System.Web.Optimization

type Route = { controller : string; action : string; id : UrlParameter }
type ApiRoute = { id : RouteParameter }
type ApiMoveRoute = { session : RouteParameter; action: RouteParameter }

type FilterConfig() =
    static member RegisterGlobalFilters(filters:GlobalFilterCollection) =
        filters.Add(new HandleErrorAttribute())
        filters.Add(new System.Web.Mvc.AuthorizeAttribute());

type WebApiConfig() =
    static member Register(config: HttpConfiguration) =
        config.MapHttpAttributeRoutes();       
        //config.Routes.MapHttpRoute( "DefaultApi", "api/{controller}/{id}", { id = RouteParameter.Optional } ) |> ignore         

type RouteConfig() =
    static member RegisterRoutes(routes: RouteCollection) =
        routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" )
        
        routes.MapRoute("Default", "{controller}/{action}/{id}", { controller = "Home"; action = "Index"; id = UrlParameter.Optional } ) |> ignore

type BundleConfig() =
    static member RegisterBundles (bundles:BundleCollection) =
        bundles.Add(ScriptBundle("~/bundles/jquery").Include([|"~/Scripts/jquery-2*"|]))
     
            
        bundles.Add(ScriptBundle("~/bundles/modernizr").Include([|"~/Scripts/modernizr-*"|]))

        bundles.Add(ScriptBundle("~/bundles/bootstrap").Include([|"~/Scripts/bootstrap*"|]))
       
        bundles.Add(StyleBundle("~/Content/css").Include("~/Content/bootstrap.css",
                                                         //"~/Content/site.css",
                                                         "~/Content/bootstrap-theme.css"))
        BundleTable.EnableOptimizations <- true;



type Global() =
    inherit System.Web.HttpApplication() 

    member this.Start() =
        Azure.initTables
        AreaRegistration.RegisterAllAreas()
        GlobalConfiguration.Configure(fun h -> WebApiConfig.Register(h))        
        FilterConfig.RegisterGlobalFilters GlobalFilters.Filters
        RouteConfig.RegisterRoutes RouteTable.Routes        
        BundleConfig.RegisterBundles BundleTable.Bundles
