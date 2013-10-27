module Azure

open System.Configuration
open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Auth
open Microsoft.WindowsAzure.Storage.Table
open WumpusWorld
//let cnnString = ConfigurationManager.ConnectionStrings.["StorageConnectionString"].ConnectionString
//let cnnString = "UseDevelopmentStorage=true"
let cnnString = ConfigurationManager.ConnectionStrings.["StorageConnectionString"].ConnectionString
let storageAccount =               
        CloudStorageAccount.Parse(cnnString)

let tableClient = storageAccount.CreateCloudTableClient()

let userstoreTable = 
        tableClient.GetTableReference("userstore")       
let apitokensTable = 
        tableClient.GetTableReference("apitokens")       
let userloginstoreTable = 
        tableClient.GetTableReference("userloginstore")        
let gameStateTable = 
        tableClient.GetTableReference("gamestate")        
let gameLogTable = 
        tableClient.GetTableReference("gamelog")                
let boardTable = 
        tableClient.GetTableReference("board")
        
let initTables = 
       [Async.AwaitTask (userstoreTable.CreateIfNotExistsAsync());
        Async.AwaitTask (apitokensTable.CreateIfNotExistsAsync());
        Async.AwaitTask (userloginstoreTable.CreateIfNotExistsAsync());
        Async.AwaitTask (gameStateTable.CreateIfNotExistsAsync());
        Async.AwaitTask (gameLogTable.CreateIfNotExistsAsync());
        Async.AwaitTask (boardTable.CreateIfNotExistsAsync())] 
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore

    
let findBoardOp boardId =
    TableOperation.Retrieve<Board>("board", boardId)
    

let findApiTokenOp apiToken =
    TableOperation.Retrieve<ApiToken>(ApiToken.PartitionKeyName, apiToken)
    

let findGameStateOp boardId gameId id =
    let userId,apiToken = id
    TableOperation.Retrieve<GameState>(userId, gameId)
    

let insertOrUpdateBoard id mapData size pits = 
    let g = new Board()
    g.PartitionKey <- "board"
    g.RowKey <- id
    g.MapData <- mapData 
    g.Size <- size
    g.Pits <- pits
    let op = TableOperation.InsertOrReplace(g)
    op

let insertGameLogOp boardId gameId id action state  =
    let userId,apiToken = id
    let l = new GameLog()
    l.PartitionKey <- userId
    l.RowKey <- System.DateTime.UtcNow.Ticks.ToString() 
    l.GameId <-gameId
    l.UserId <- userId
    l.ApiToken <- apiToken
    l.BoardId <- boardId
    l.Action <- action
    l.NewState <- state
    
    let op = TableOperation.Insert(l)
    op
    
    
let insertOrUpdateGameStateOp boardId gameId id xPos yPos dir score mapData =
        let s = new GameState()
        let userId,apiToken = id
        s.PartitionKey <- userId
        s.RowKey <- gameId
        s.BoardId <- boardId
        s.UserId <- userId
        s.ApiToken <- apiToken
        s.XPos <- xPos
        s.YPos <- yPos
        s.Score <- score
        s.Direction <- dir
        s.MapData <- mapData                                                      
        let insertOrReplaceOperation = TableOperation.InsertOrReplace(s)
        insertOrReplaceOperation

let awaitOp (table:CloudTable) (op:TableOperation) =
    Async.AwaitTask (table.ExecuteAsync op)

let awaitOp_BoardTable op =
    awaitOp boardTable op  

let awaitOp_GameStateTable op =
    awaitOp gameStateTable op

let awaitOp_GameLogTable op =
    awaitOp gameLogTable op
