module Azure

open System.Configuration

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Auth
open Microsoft.WindowsAzure.Storage.Table
open WumpusWorld
//let cnnString = ConfigurationManager.ConnectionStrings.["StorageConnectionString"].ConnectionString
//let cnnString = "UseDevelopmentStorage=true"
let cnnString = ConfigurationManager.ConnectionStrings.["StorageConnectionString"].ConnectionString
let storageAccount =
       
        
        CloudStorageAccount.Parse(cnnString);

let tableClient = storageAccount.CreateCloudTableClient()


let gameStateTable = 
        let t =  tableClient.GetTableReference("gamestate")
        t.CreateIfNotExists() |> ignore
        t
let gameLogTable = 
        let t =  tableClient.GetTableReference("gamelog")
        t.CreateIfNotExists() |> ignore
        t
let boardTable = 
        let t =  tableClient.GetTableReference("board")
        t.CreateIfNotExists() |> ignore
        t

let findBoardOp boardId =
    let op = TableOperation.Retrieve<Board>("board", boardId)
    op
let findGameStateOp boardId gameId =
    let op = TableOperation.Retrieve<GameState>(boardId, gameId)
    op

let insertOrUpdateBoard id mapData size pits = 
    let g = new Board()
    g.PartitionKey <- "board"
    g.RowKey <- id
    g.MapData <- mapData 
    g.Size <- size
    g.Pits <- pits
    let op = TableOperation.InsertOrReplace(g)
    op

let insertGameLogOp boardId gameId action state  =
    let l = new GameLog()
    l.PartitionKey <- gameId
    l.RowKey <- System.DateTime.UtcNow.Ticks.ToString() 
    l.BoardId <- boardId
    l.Action <- action
    l.NewState <- state
    let op = TableOperation.Insert(l)
    op
    
    
let insertOrUpdateGameStateOp boardId gameId xPos yPos dir mapData =
        let s = new GameState()
        s.PartitionKey <- boardId
        s.RowKey <- gameId
        s.XPos <- xPos
        s.YPos <- yPos
        s.Direction <- dir
        s.MapData <- mapData                                                      
        let insertOrReplaceOperation = TableOperation.InsertOrReplace(s)
        insertOrReplaceOperation
let executeOn_boardTable op =
    let beginExecute op =
        fun (cp,_) -> boardTable.BeginExecute(op,cp,null) :> System.IAsyncResult
    Async.FromBeginEnd(beginExecute op,boardTable.EndExecute)

let executeOn_gameStateTable op =
    let beginExecute op =
        fun (cp,_) -> gameStateTable.BeginExecute(op,cp,null) :> System.IAsyncResult
    Async.FromBeginEnd(beginExecute op,gameStateTable.EndExecute)

let executeOn_gameLogTable op =
    let beginExecute op =
        fun (cp,_) -> gameLogTable.BeginExecute(op,cp,null) :> System.IAsyncResult
    Async.FromBeginEnd(beginExecute op,gameLogTable.EndExecute)