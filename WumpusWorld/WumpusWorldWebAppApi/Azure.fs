module Azure

open System.Configuration

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Auth
open Microsoft.WindowsAzure.Storage.Table
open WumpusWorld
//let cnnString = ConfigurationManager.ConnectionStrings.["StorageConnectionString"].ConnectionString
let cnnString = "UseDevelopmentStorage=true"
let storageAccount =  CloudStorageAccount.Parse(cnnString);

let tableClient = storageAccount.CreateCloudTableClient()


let gameStateTable = 
        let t =  tableClient.GetTableReference("gamestate")
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

let insertOrUpdateBoard id mapData = 
    let g = new Board()
    g.PartitionKey <- "board"
    g.RowKey <- id
    g.MapData <- mapData 
    let op = TableOperation.InsertOrReplace(g)
    op

let insertOrUpdateGameStateOp boardId gameId xPos yPos dir  =
        let s = new GameState()
        s.PartitionKey <- boardId
        s.RowKey <- gameId
        s.XPos <- xPos
        s.YPos <- yPos
        s.Direction <- dir                                                     
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