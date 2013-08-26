namespace WumpusWorld


open System.Runtime.Serialization.Formatters.Binary
open System.IO
open Microsoft.WindowsAzure.Storage.Table


module Helper = 
    let rand = System.Random()
    let dser (s:string) = 
        let b = System.Convert.FromBase64String(s)
        let formatter = new BinaryFormatter()
        let ms = new MemoryStream(b)        
        let o = formatter.Deserialize(ms) :?> (CellObject * CellSense list) [,]
        o

    let ser (s:(CellObject * CellSense list) [,]) =                      
         let formatter = new BinaryFormatter()
         let ms = new MemoryStream()
         formatter.Serialize(ms, s);
         let o = System.Convert.ToBase64String(ms.ToArray())
         o
                               
    let inMaze (maze : (CellObject * CellSense list) [,]) (x, y) = 
        let inline inMaze' xMax yMax x y = 0 <= x && x < xMax && 0 <= y && y < yMax
        let maxX = Array2D.length1 maze
        let maxY = Array2D.length2 maze
        if (inMaze' maxX maxY x y) then Some(x, y)
        else None
    let getMazeFromTable (table:TableResult) =
        match table with
            | null -> None
            | t -> match t.Result with 
                    | null -> None
                    | r -> Some(dser (r :?> Board).MapData)
                
    let getPosition state = 
        match state with
        | E(x, y) -> x, y
        | W(x, y) -> x, y
        | S(x, y) -> x, y
        | N(x, y) -> x, y
    let getPositionWithDirection state = 
        match state with
        | E(x, y) -> x, y, "E"
        | W(x, y) -> x, y, "W"
        | S(x, y) -> x, y, "S"
        | N(x, y) -> x, y, "N"
    let getDirectionAsString state = 
        match state with
        | E(_, _) -> "E"
        | W(_, _) -> "W"
        | S(_, _) -> "S"
        | N(_, _) -> "N"

    let getCellSense (maze : (CellObject * CellSense list) [,]) (x, y) = 
        let currentCellSence = snd maze.[x, y]
        currentCellSence
    
    let filterStenchCellSense s:CellSense list =
        s |> List.filter (fun f -> match f with 
                                       | Stench -> false
                                       | _ -> true)
    let filterGlitterCellSense s:CellSense list =
        s |> List.filter (fun f -> match f with 
                                       | Glitter -> false
                                       | _ -> true)

    let getCellObject (maze : (CellObject * CellSense list) [,]) (x, y) = 
        let currentObject = fst maze.[x, y]
        currentObject
    
    let getNewGameState currentState newPosition = 
        match currentState with
        | E(_, _) -> E(newPosition)
        | W(_, _) -> W(newPosition)
        | S(_, _) -> S(newPosition)
        | N(_, _) -> N(newPosition)
    

    let getGameScore (tableResult:TableResult) =
         match tableResult with
            | null -> 0
            | t -> match t.Result with
                     | null -> 0
                     | r -> let s = r :?> GameState
                            s.Score
    let getGameState (tableResult:TableResult) =
           match tableResult with
            | null -> None
            | t -> match t.Result with
                     | null -> None
                     | r -> let s = r :?> GameState
                            let map = dser s.MapData
                            match s.Direction with 
                                | "N" -> Some(N(s.XPos,s.YPos),map)
                                | "S" -> Some(S(s.XPos,s.YPos),map)
                                | "W" -> Some(W(s.XPos,s.YPos),map)
                                | "E" -> Some(E(s.XPos,s.YPos),map)
                                | _ -> None     
          
    let getGameStateDefault (tableResult:TableResult) =
            if (tableResult.Result <> null) then
                let s = tableResult.Result :?> GameState
                match s.Direction with 
                    | "N" ->  N(s.XPos,s.YPos)
                    | "S" -> S(s.XPos,s.YPos)
                    | "W" -> W(s.XPos,s.YPos)
                    | "E" -> E(s.XPos,s.YPos)
                    | _ -> S(0,0)                                
            else                               
                S(0,0)  
    let getNewPos (maze : (CellObject * CellSense list) [,]) actorState = 
        match actorState with
        | E(x, y) -> inMaze maze (x, y + 1)
        | W(x, y) -> inMaze maze (x, y - 1)
        | S(x, y) -> inMaze maze (x + 1, y)
        | N(x, y) -> inMaze maze (x - 1, y)
    
    
    let printMaze (maze : (CellObject * CellSense list) [,]) = 
        let sb = new System.Text.StringBuilder()
        sb.AppendLine() |> ignore
        for x in 0..maze.GetLength(0) - 1 do
            for y in 0..maze.GetLength(1) - 1 do
                let (o, s) = maze.[x, y]  
                sb.Append(o.ToString()) |> ignore
                sb.Append(s.ToString()) |> ignore
                ()
            sb.AppendLine() |> ignore
        sb.ToString()
  