module Azure

open System.Configuration

open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Auth
open Microsoft.WindowsAzure.Storage.Table

//let cnnString = ConfigurationManager.ConnectionStrings.["StorageConnectionString"].ConnectionString
let cnnString = "UseDevelopmentStorage=true"
let storageAccount =  CloudStorageAccount.Parse(cnnString);

let tableClient = storageAccount.CreateCloudTableClient()


let actorstateTable = 
        let t =  tableClient.GetTableReference("actorstate")
        t.CreateIfNotExists() |> ignore
        t

let gameboardTable = 
        let t =  tableClient.GetTableReference("gameboard")
        t.CreateIfNotExists() |> ignore
        t
let executeOnGameboardTable op =
    let beginExecute op =
        fun (cp,_) -> gameboardTable.BeginExecute(op,cp,null) :> System.IAsyncResult
    Async.FromBeginEnd(beginExecute op,gameboardTable.EndExecute)

let executeOnActorStateTable op =
    let beginExecute op =
        fun (cp,_) -> actorstateTable.BeginExecute(op,cp,null) :> System.IAsyncResult
    Async.FromBeginEnd(beginExecute op,actorstateTable.EndExecute)