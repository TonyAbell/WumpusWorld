namespace WumpusWorld

module Helper = 
    let createMaze xMax yMax = 
        let rand = System.Random()
        let maze = Array2D.create xMax yMax (Free, List.empty<CellSenses>)
        let inline inMaze x y = 0 <= x && x < xMax && 0 <= y && y < yMax
        maze.[0, 0] <- Start, List.empty<CellSenses>
        let goldx, goldy = rand.Next(1, xMax), rand.Next(1, yMax)
        maze.[goldx, goldy] <- Gold, [Glitter]
        let pitx, pity = rand.Next(1, xMax), rand.Next(1, yMax)
        maze.[pitx, pity] <- Pit, []
        if inMaze (pitx - 1) (pity) then 
            maze.[pitx - 1, pity] <- (fst maze.[pitx - 1, pity]), 
                                     ((snd maze.[pitx - 1, pity]) @ [Breeze])
        if inMaze (pitx + 1) (pity) then 
            maze.[pitx + 1, pity] <- (fst maze.[pitx + 1, pity]), 
                                     ((snd maze.[pitx + 1, pity]) @ [Breeze])
        if inMaze (pitx) (pity - 1) then 
            maze.[pitx, pity - 1] <- (fst maze.[pitx, pity - 1]), 
                                     ((snd maze.[pitx, pity - 1]) @ [Breeze])
        if inMaze (pitx) (pity + 1) then 
            maze.[pitx, pity + 1] <- (fst maze.[pitx, pity + 1]), 
                                     ((snd maze.[pitx, pity + 1]) @ [Breeze])
        maze
    
    let inMaze' xMax yMax x y = 0 <= x && x < xMax && 0 <= y && y < yMax
    
    let inMaze (maze : (CellObject * CellSenses list) [,]) (x, y) = 
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
    
    let getNewPos (maze : (CellObject * CellSenses list) [,]) actorState = 
        match actorState with
        | E(x, y) -> inMaze maze (x, y + 1)
        | W(x, y) -> inMaze maze (x, y - 1)
        | S(x, y) -> inMaze maze (x - 1, y)
        | N(x, y) -> inMaze maze (x + 1, y)
    
    let move (maze : (CellObject * CellSenses list) [,]) 
        (actorState : ActorState) (action : Action) = 
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
    
    let create = createMaze 5 5
    
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
