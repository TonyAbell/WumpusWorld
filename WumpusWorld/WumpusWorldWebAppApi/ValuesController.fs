namespace FsWeb.Controllers
open WumpusWorld
open System.Web
open System.Web.Mvc
open System.Net.Http
open System.Web.Http
open System.Runtime.Serialization
open System.Data.Services.Common

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Auth
open Microsoft.WindowsAzure.Storage.Table

[<DataContract>]
[<CLIMutable>] 
type State = {  [<field: DataMember(Name="ActionSenses")>] ActionSenses: string;
                [<field: DataMember(Name="CellSenses")>] CellSenses: string list;
               [<field: DataMember(Name="ActorState")>] ActorState: string }




type ActorSavedState() =
    inherit TableEntity()
    member val XPos = 0 with get,set
    member val YPos = 0 with get,set
    member val Direction = "" with get,set

type GameBoard() =
    inherit TableEntity()      
    member val Data = "" with get,set

type MoveController() =
    inherit ApiController()
    let mutable state = N(0,0)
    // GET /api/values
    [<HttpGet>]
    member x.Forward(id:string) =
       async {
                
                let retrieveGameBoard = TableOperation.Retrieve<GameBoard>("game", id)
                let! retrieveGameBoardResult = Azure.executeOnGameboardTable retrieveGameBoard
                let maze = Helper.dser (retrieveGameBoardResult.Result :?> GameBoard).Data

                let retrieve = TableOperation.Retrieve<ActorSavedState>("state", id)
                
                            
               
                let! retrievedResult = Azure.executeOnActorStateTable retrieve

                
                let state = if (retrievedResult.Result <> null) then
                                let s = retrievedResult.Result :?> ActorSavedState
                                match s.Direction with 
                                    | "N" -> N(s.XPos,s.YPos)
                                    | "S" -> S(s.XPos,s.YPos)
                                    | "W" -> W(s.XPos,s.YPos)
                                    | "E" -> E(s.XPos,s.YPos)
                                    | _ -> N(0,0)                                    
                            else                               
                                N(0,0)

                let a,b,c = Helper.move maze state Forward
                let xPos,yPos = Helper.getPosition c
                let s = new ActorSavedState()
                
                s.PartitionKey <- "state"
                s.RowKey <- id
                s.XPos <- xPos
                s.YPos <- yPos
                s.Direction <- Helper.getDirectionAsString c
                               
                let insertOrReplaceOperation = TableOperation.InsertOrReplace(s)
                
                
                let! insertOrReplaceResult = Azure.executeOnActorStateTable insertOrReplaceOperation
                let b' = List.map(fun f-> f.ToString()) b
                return  { ActionSenses = a.ToString(); CellSenses = b' ; ActorState = c.ToString()}
       } |> Async.StartAsTask
    [<HttpGet>]
    member x.Left(id:string) =
        let r = new System.Random()
        r.Next()
    [<HttpGet>]
    member x.Right(id:string) =
        let r = new System.Random()
        r.Next()
    [<HttpGet>]
    member x.Shoot(id:string) =
        let r = new System.Random()
        r.Next()
    [<HttpGet>]
    member x.Grab(id:string) =
        let r = new System.Random()
        r.Next()


type GameController() =
    inherit ApiController()
    [<HttpGet>]
    member x.Start() =
     async {
            let r = new System.Random()
            let nextId = r.Next(1,System.Int32.MaxValue)
            let newMaze = Helper.createMaze 10 10
            let g = new GameBoard()
            g.PartitionKey <- "game"
            g.RowKey <- nextId.ToString()
            g.Data <- Helper.ser newMaze
            let insertOrReplaceOperation = TableOperation.InsertOrReplace(g)
                
                
            let! insertOrReplaceResult = Azure.executeOnGameboardTable insertOrReplaceOperation
            
            return nextId
        }|> Async.StartAsTask
    [<HttpGet>]
    member x.Status(id:string) =
        "abc"
    [<HttpGet>]
    member x.Board(id:string) =
        "abc"

