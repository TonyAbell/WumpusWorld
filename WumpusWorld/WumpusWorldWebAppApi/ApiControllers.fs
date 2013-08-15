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
type State = 
    { [<DataMember(Name = "ActionSenses")>] ActionSenses : string
      [<DataMember(Name = "CellSenses")>] CellSenses : string list
      [<DataMember(Name = "ActorState")>] ActorState : string }

type MoveController() = 
    inherit ApiController()
    
    let moveActorAndSaveNewState id action = 
        async { 
            let retrieveGameBoard = TableOperation.Retrieve<GameBoard>("game", id)
            let! retrieveGameBoardResult = Azure.executeOnGameboardTable retrieveGameBoard
            let maze = Helper.dser (retrieveGameBoardResult.Result :?> GameBoard).Data
            let retrieveSavedStateOp = TableOperation.Retrieve<ActorSavedState>("state", id)
            let! retrievedResult = Azure.executeOnActorStateTable retrieveSavedStateOp
            let state = Helper.getActorStateDefault retrievedResult
            let a, b, c = Helper.move maze state action
            let xPos, yPos = Helper.getPosition c
            let s = new ActorSavedState()
            s.PartitionKey <- "state"
            s.RowKey <- id
            s.XPos <- xPos
            s.YPos <- yPos
            s.Direction <- Helper.getDirectionAsString c
            let insertOrReplaceOperation = TableOperation.InsertOrReplace(s)
            let! insertOrReplaceResult = Azure.executeOnActorStateTable insertOrReplaceOperation
            
            return { ActionSenses = a.ToString()
                     CellSenses = List.map (fun f -> f.ToString()) b
                     ActorState = c.ToString() } 
        }
    
    [<HttpGet>]
    // GET /api/values
    member x.Forward(id : string) = 
        async { let! newState = moveActorAndSaveNewState id Forward
                return newState } |> Async.StartAsTask
    
    [<HttpGet>]
    member x.Left(id : string) = 
         async { let! newState = moveActorAndSaveNewState id Left
                return newState } |> Async.StartAsTask
    
    [<HttpGet>]
    member x.Right(id : string) = 
        async { let! newState = moveActorAndSaveNewState id Right
                return newState } |> Async.StartAsTask
    
    [<HttpGet>]
    member x.Shoot(id : string) = 
         async { let! newState = moveActorAndSaveNewState id Shoot
                return newState } |> Async.StartAsTask
    
    [<HttpGet>]
    member x.Grab(id : string) = 
         async { let! newState = moveActorAndSaveNewState id Grab
                return newState } |> Async.StartAsTask

type GameController() = 
    inherit ApiController()
    
    [<HttpGet>]
    member x.Start() = 
        async { 
            let r = new System.Random()
            let nextId = r.Next(1, System.Int32.MaxValue)
            let newMaze = Helper.createMaze 10 10
            let g = new GameBoard()
            g.PartitionKey <- "game"
            g.RowKey <- nextId.ToString()
            g.Data <- Helper.ser (newMaze 5)
            let insertOrReplaceOperation = TableOperation.InsertOrReplace(g)
            let! insertOrReplaceResult = Azure.executeOnGameboardTable 
                                             insertOrReplaceOperation
            return nextId }
        |> Async.StartAsTask
    
    [<HttpGet>]
    member x.Status(id : string) = "sample status"
    
    [<HttpGet>]
    member x.Board(id : string) = 
        async { 
            let retrieveGameBoard = 
                TableOperation.Retrieve<GameBoard>("game", id)
            let! retrieveGameBoardResult = Azure.executeOnGameboardTable 
                                               retrieveGameBoard
            let maze = 
                Helper.dser (retrieveGameBoardResult.Result :?> GameBoard).Data
            let lenght = maze |> Array2D.length1            

            return seq { 
                    for i in 0..lenght - 1 do
                        yield seq { 
                            for u in 0..lenght - 1 do
                                let c, s = maze.[i, u]
                                yield c.ToString() } }
        } |> Async.StartAsTask
