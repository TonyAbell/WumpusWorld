namespace WumpusWorld

open Microsoft.FSharp.Reflection 
open System.Reflection 
open System.Runtime.Serialization 
open System.Runtime.Serialization.Json
open System.Runtime.Serialization.Formatters.Binary
open System.Xml
open System.IO
open Microsoft.WindowsAzure.Storage.Table
open Newtonsoft.Json

module Helper = 
    let rand = System.Random()
    let dser (s:string) = 
        let b = System.Convert.FromBase64String(s)
        let formatter = new BinaryFormatter()
        let ms = new MemoryStream(b)        
        let o = formatter.Deserialize(ms) :?> (CellObject * CellSenses list) [,]
        o

    let ser (s:(CellObject * CellSenses list) [,]) =                      
         let formatter = new BinaryFormatter()
         let ms = new MemoryStream()
         formatter.Serialize(ms, s);
         let o = System.Convert.ToBase64String(ms.ToArray())
         o
                               
    let inMaze (maze : (CellObject * CellSenses list) [,]) (x, y) = 
        let inline inMaze' xMax yMax x y = 0 <= x && x < xMax && 0 <= y && y < yMax
        let maxX = Array2D.length1 maze
        let maxY = Array2D.length2 maze
        if (inMaze' maxX maxY x y) then Some(x, y)
        else None
    
    let getPosition state = 
        match state with
        | E(x, y) -> x, y
        | W(x, y) -> x, y
        | S(x, y) -> x, y
        | N(x, y) -> x, y
    
    let getDirectionAsString state = 
        match state with
        | E(_, _) -> "E"
        | W(_, _) -> "W"
        | S(_, _) -> "S"
        | N(x, y) -> "N"

    let getCellSense (maze : (CellObject * CellSenses list) [,]) (x, y) = 
        let currentCellSence = snd maze.[x, y]
        currentCellSence
    
    let getCellObject (maze : (CellObject * CellSenses list) [,]) (x, y) = 
        let currentObject = fst maze.[x, y]
        currentObject
    
    let getNewActorState currentState newPosition = 
        match currentState with
        | E(_, _) -> E(newPosition)
        | W(_, _) -> W(newPosition)
        | S(_, _) -> S(newPosition)
        | N(_, _) -> N(newPosition)
    
    let getActorState (tableResult:TableResult) =
            if (tableResult.Result <> null) then
                let s = tableResult.Result :?> ActorSavedState
                match s.Direction with 
                    | "N" -> Some( N(s.XPos,s.YPos))
                    | "S" -> Some(S(s.XPos,s.YPos))
                    | "W" -> Some(W(s.XPos,s.YPos))
                    | "E" -> Some(E(s.XPos,s.YPos))
                    | _ -> None                                
            else                               
                None
    let getActorStateDefault (tableResult:TableResult) =
            if (tableResult.Result <> null) then
                let s = tableResult.Result :?> ActorSavedState
                match s.Direction with 
                    | "N" ->  N(s.XPos,s.YPos)
                    | "S" -> S(s.XPos,s.YPos)
                    | "W" -> W(s.XPos,s.YPos)
                    | "E" -> E(s.XPos,s.YPos)
                    | _ -> S(0,0)                                
            else                               
                S(0,0)  
    let getNewPos (maze : (CellObject * CellSenses list) [,]) actorState = 
        match actorState with
        | E(x, y) -> inMaze maze (x, y + 1)
        | W(x, y) -> inMaze maze (x, y - 1)
        | S(x, y) -> inMaze maze (x - 1, y)
        | N(x, y) -> inMaze maze (x + 1, y)
    
    let move 
        (maze : (CellObject * CellSenses list) [,]) 
        (actorState : ActorState) 
        (action : Action) = 
        let currentCellSence = getCellSense maze (getPosition actorState)
        match action with
        | Forward -> 
            let xy = getNewPos maze actorState
            match xy with
            | Some(pos) -> 
                let cellObj = getCellObject maze pos
                let newCellSense = getCellSense maze pos
                match cellObj with
                | Wumpus -> 
                    Eaten, currentCellSence, (getNewActorState actorState pos)
                | Pit -> 
                    Fell, currentCellSence, (getNewActorState actorState pos)
                | Gold -> Moved, newCellSense, (getNewActorState actorState pos)
                | Free -> Moved, newCellSense, (getNewActorState actorState pos)
                | Start -> 
                    Moved, newCellSense, (getNewActorState actorState pos)
            | None -> Bump, currentCellSence, actorState
        | Left -> 
            match actorState with
            | E(x, y) -> Turned, currentCellSence, N(x, y)
            | W(x, y) -> Turned, currentCellSence, S(x, y)
            | S(x, y) -> Turned, currentCellSence, E(x, y)
            | N(x, y) -> Turned, currentCellSence, W(x, y)
        | Right -> 
            match actorState with
            | E(x, y) -> Turned, currentCellSence, S(x, y)
            | W(x, y) -> Turned, currentCellSence, N(x, y)
            | S(x, y) -> Turned, currentCellSence, W(x, y)
            | N(x, y) -> Turned, currentCellSence, E(x, y)
        | Grab -> let pos = getPosition actorState
                  let cellObj = getCellObject maze pos
                  match cellObj with
                    | Gold -> Looted, currentCellSence, actorState
                    | _ -> Nothing , currentCellSence, actorState
        | Shoot -> 
            let xy = getNewPos maze actorState
            match xy with
            | Some(pos) -> 
                let cellObj = getCellObject maze pos
                match cellObj with
                | Wumpus -> Screem, currentCellSence, actorState
                | _ -> Silence, currentCellSence, actorState
            | None -> Silence, currentCellSence, actorState
    

    let rec addCellObject maze cellObj sense  =
        let xMax, yMax = (maze |> Array2D.length1),(maze |> Array2D.length2)
        
        let pos = rand.Next(2, xMax), rand.Next(2, yMax)
        let x, y = pos
        let inline inMaze x y = 0 <= x && x < xMax && 0 <= y && y < yMax
        match getCellObject maze pos with
            | Free ->   maze.[x, y] <- cellObj, []
                        if inMaze (x - 1) (y) then 
                            maze.[x - 1, y] <- (fst maze.[x - 1, y]), ((snd maze.[x - 1, y]) @ [sense])
                        if inMaze (x + 1) (y) then 
                            maze.[x + 1, y] <- (fst maze.[x + 1, y]), ((snd maze.[x + 1, y]) @ [sense])
                        if inMaze (x) (y - 1) then 
                            maze.[x, y - 1] <- (fst maze.[x, y - 1]), ((snd maze.[x, y - 1]) @ [sense])
                        if inMaze (x) (y + 1) then 
                            maze.[x, y + 1] <- (fst maze.[x, y + 1]), ((snd maze.[x, y + 1]) @ [sense])
            | _ -> addCellObject maze cellObj sense 
        ()

 
    let createMaze xMax yMax pits =        
        
        let maze = Array2D.create xMax yMax (Free, List.empty<CellSenses>)
        let inline inMaze x y = 0 <= x && x < xMax && 0 <= y && y < yMax
        maze.[0, 0] <- Start, List.empty<CellSenses>
        let goldx, goldy = rand.Next(2, xMax), rand.Next(2, yMax)
        maze.[goldx, goldy] <- Gold, [Glitter]
      
        for _ in 1 .. pits do
            addCellObject maze Pit Breeze

        addCellObject maze Wumpus Stench

        maze    

    let sampleMaze = createMaze 5 5 5
    
    let printMaze (maze : (CellObject * CellSenses list) [,]) = 
        let sb = new System.Text.StringBuilder()
        sb.AppendLine() |> ignore
        for x in 0..maze.GetLength(0) - 1 do
            for y in 0..maze.GetLength(1) - 1 do
                let (o, s) = maze.[x, y]
                //sb.Append("x" + x.ToString() + " y" + y.ToString() + " : ") |> ignore
                sb.Append(o.ToString()) |> ignore
                sb.Append(s.ToString()) |> ignore
                ()
            //sb.Append( (o.ToString()) + " " + (s.ToString())) |> ignore
            //printf "%s %s" (o.ToString()) (s.ToString())
            sb.AppendLine() |> ignore
        sb.ToString()
    
//    let testMove() = 
//        let m = create()
//        move m (N(0, 0)) Forward
