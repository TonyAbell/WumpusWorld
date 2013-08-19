namespace FsWeb.Controllers

open WumpusWorld
open System.Web
open System.Web.Mvc
open System.Net.Http
open System.Web.Http
open System.Net
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
    
    let moveActorAndSaveNewGameState boardId gameId action = 
        async { 
                                  
            let! retrievedResult = Azure.executeOn_gameStateTable (Azure.findGameStateOp boardId gameId)           
            let gameState = Helper.getGameState retrievedResult           

            return! match gameState with 
                            | Some(gameState) ->  async{     
                                                            let state,maze = gameState
                                                            let currentPos = Helper.getPosition state
                                                            let currentCellObj = Helper.getCellObject maze currentPos
                                                            return! match currentCellObj with 
                                                                            | Pit | Wumpus -> async{ 
                                                                                            return Some({ActionSenses = "Dead"; CellSenses = []; ActorState = state.ToString()})
                                                                                        }
                                                                            | _ -> async{ 
                                                                                            let actionSense, newGameState, newMaze = Engine.move maze state action
                                                                                            let xPos, yPos,dir = Helper.getPositionWithDirection newGameState  
                                                                                            let cellSenses = Helper.getCellSense newMaze (xPos, yPos)                                                                                                             
                                                                                            let mapData = Helper.ser newMaze                                                       
                                                                                            let gameStateOp = Azure.insertOrUpdateGameStateOp boardId gameId xPos yPos dir mapData  
                                                                                            
                                                                                            let! insertResutl = Azure.executeOn_gameStateTable gameStateOp 
                                                                                            let gameLogOp = Azure.insertGameLogOp boardId gameId (action.ToString()) (newGameState.ToString())
                                                                                            let! interLogResult = Azure.executeOn_gameLogTable gameLogOp
                                                                                            return  Some({ActionSenses = actionSense.ToString(); CellSenses = List.map (fun f -> f.ToString()) cellSenses; ActorState = newGameState.ToString()})                                                                                                                                                               
                                                                                         }                                                            
                                                            }
                            | _ -> async{return None}                                                                   
        }
    
    [<HttpGet("api/board/{boardid}/game/{gameid}/forward")>]
    member x.Forward(boardid : string, gameid: string) =
        
        async { let! newState = moveActorAndSaveNewGameState boardid gameid Forward
                return match newState with 
                                | Some state -> x.Request.CreateResponse<State>(HttpStatusCode.OK, state)
                                | None -> x.Request.CreateResponse(HttpStatusCode.BadRequest)

                } |> Async.StartAsTask
    
    [<HttpGet("api/board/{boardid}/game/{gameid}/left")>]
    member x.Left(boardid : string, gameid: string) = 
         async { let!newState = moveActorAndSaveNewGameState boardid gameid Left
                 return match newState with 
                                | Some state -> x.Request.CreateResponse<State>(HttpStatusCode.OK, state)
                                | None -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
                } |> Async.StartAsTask
    
    [<HttpGet("api/board/{boardid}/game/{gameid}/right")>]
    member x.Right(boardid : string, gameid: string) = 
        async { let!newState = moveActorAndSaveNewGameState boardid gameid Right
                return match newState with 
                                | Some state -> x.Request.CreateResponse<State>(HttpStatusCode.OK, state)
                                | None -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
                } |> Async.StartAsTask
    
    [<HttpGet("api/board/{boardid}/game/{gameid}/shoot")>]
    member x.Shoot(boardid : string, gameid: string) = 
         async { let!newState = moveActorAndSaveNewGameState boardid gameid Shoot
                 return match newState with 
                                | Some state -> x.Request.CreateResponse<State>(HttpStatusCode.OK, state)
                                | None -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
                } |> Async.StartAsTask
    
    [<HttpGet("api/board/{boardid}/game/{gameid}/grab")>]
    member x.Grab(boardid : string, gameid: string) = 
         async { let!newState = moveActorAndSaveNewGameState boardid gameid  Grab
                 return match newState with 
                                | Some state -> x.Request.CreateResponse<State>(HttpStatusCode.OK, state)
                                | None -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
                } |> Async.StartAsTask



type GameController() = 
    inherit ApiController()
    let r = new System.Random()
    [<HttpGet("api/board/new/{size=10}/{pits=5}")>]
    member x.NewBoard(size:int, pits:int) =
        async {                  
                    let nextId = r.Next(1, System.Int32.MaxValue).ToString()
                    let newMaze = Engine.createMaze size size
                    let newBoardId = nextId
                    let boardData = Helper.ser (newMaze pits)
                    let g = new Board()           
                    let! insertOrReplaceResult = Azure.executeOn_boardTable (Azure.insertOrUpdateBoard newBoardId boardData size pits)
                    return match insertOrReplaceResult with
                                    | null -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
                                    | _ ->  x.Request.CreateResponse<string>(HttpStatusCode.OK, nextId)
                    
                   
        } |> Async.StartAsTask

    [<HttpGet("api/board/{boardid}/game/new")>]
    member x.NewGame(boardId:string) = 
        async { 
                let nextId = r.Next(1, System.Int32.MaxValue).ToString()
                let! retrieveGameBoardResult = Azure.executeOn_boardTable (Azure.findBoardOp boardId)            
                let board = Helper.getMazeFromTable retrieveGameBoardResult  
                return! match board with 
                                | Some b -> async {
                                                let mapData = Helper.ser b
                                                let! insertResutl = Azure.executeOn_gameStateTable (Azure.insertOrUpdateGameStateOp boardId nextId 0 0 "S" mapData)   
                                                let gameLogOp = Azure.insertGameLogOp boardId nextId "Init" ""
                                                let! interLogResult = Azure.executeOn_gameLogTable gameLogOp
                                                return x.Request.CreateResponse<string>(HttpStatusCode.OK, nextId)
                                            }
                                | None ->  async {return x.Request.CreateResponse(HttpStatusCode.BadRequest)}
                 
           }
        |> Async.StartAsTask
    
    [<HttpGet("api/board/{boardid}/game/{gameid}/status")>]
    member x.Status(boardid : string, gameid: string) = 
        async { 

                let! retrievedResult = Azure.executeOn_gameStateTable (Azure.findGameStateOp boardid gameid)           
                let stateAndMaze = Helper.getGameState retrievedResult 
                return match stateAndMaze with 
                                | Some(gameState)-> let state,maze = gameState
                                                    let xPos, yPos,dir = Helper.getPositionWithDirection state   
                                                             
                                                    let currentCellObj = Helper.getCellObject maze (xPos,yPos)
                                                    let returnState = match currentCellObj with 
                                                                    | Pit | Wumpus -> {ActionSenses = "Dead"; CellSenses = []; ActorState = state.ToString()}
                                                                    | _ -> let cellSenses = Helper.getCellSense (maze) (xPos, yPos)  
                                                                           {ActionSenses = ""; CellSenses = List.map (fun f -> f.ToString()) cellSenses; ActorState = state.ToString()}
                                                    x.Request.CreateResponse<State>(HttpStatusCode.OK, returnState)
                                |  _ -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
        }
         |> Async.StartAsTask

     [<HttpGet("api/board/{boardid}/game/{gameid}/view")>]
     member x.GameBoard(boardid : string, gameid: string) = 
        async { 
            
            let! retrievedResult = Azure.executeOn_gameStateTable (Azure.findGameStateOp boardid gameid)           
            let stateAndMaze = Helper.getGameState retrievedResult 
            return match stateAndMaze with
                            | Some gameState -> 
                                           let state,maze = gameState
                                           let lenght = maze |> Array2D.length1  
                                           let view  = seq {  for i in 0..lenght - 1 do
                                                                yield seq { 
                                                                    for u in 0..lenght - 1 do
                                                                        let c, s = maze.[i, u]
                                                                        yield c.ToString() } }
                                                      

                                           x.Request.CreateResponse<seq<seq<string>>>(HttpStatusCode.OK, view)
                            | None -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
                      

            
        } |> Async.StartAsTask
    [<HttpGet("api/board/{boardid}/view")>]
    member x.Board(boardid : string) = 
        async { 
            
            let! retrieveGameBoardResult = Azure.executeOn_boardTable (Azure.findBoardOp boardid)            
            let board = Helper.getMazeFromTable retrieveGameBoardResult      
            return match board with
                            | Some maze -> let lenght = maze |> Array2D.length1  
                                           let view  = seq {  for i in 0..lenght - 1 do
                                                                yield seq { 
                                                                    for u in 0..lenght - 1 do
                                                                        let c, s = maze.[i, u]
                                                                        yield c.ToString() } }
                                                      

                                           x.Request.CreateResponse<seq<seq<string>>>(HttpStatusCode.OK, view)
                            | None -> x.Request.CreateResponse(HttpStatusCode.BadRequest)
                      

            
        } |> Async.StartAsTask
