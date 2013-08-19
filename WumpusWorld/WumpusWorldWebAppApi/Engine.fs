namespace WumpusWorld

module Engine =
    let rand = System.Random()

    let killWumpus maze pos =
        let xMax, yMax = (maze |> Array2D.length1),(maze |> Array2D.length2) 
        let inline inMaze x y = 0 <= x && x < xMax && 0 <= y && y < yMax  
        let x, y = pos
        let cellSenses = Helper.getCellSense maze pos
        maze.[x,y] <- Free,cellSenses
        if inMaze (x - 1) (y) then 
            let currentSenes = (snd maze.[x - 1, y])
            let newSenses = currentSenes |> Helper.filterStenchCellSense
            maze.[x - 1, y] <- (fst maze.[x - 1, y]), newSenses
        if inMaze (x + 1) (y) then 
            let currentSenes = (snd maze.[x + 1, y])
            let newSenses = currentSenes |> Helper.filterStenchCellSense
            maze.[x + 1, y] <- (fst maze.[x + 1, y]), newSenses
        if inMaze (x) (y - 1) then 
            let currentSenes = (snd maze.[x, y - 1])
            let newSenses = currentSenes |> Helper.filterStenchCellSense
            maze.[x, y - 1] <- (fst maze.[x, y - 1]), newSenses
        if inMaze (x) (y + 1) then 
            let currentSenes = (snd maze.[x, y + 1])
            let newSenses = currentSenes |> Helper.filterStenchCellSense
            maze.[x, y + 1] <- (fst maze.[x, y + 1]), newSenses

        maze
    let move 
        (maze : (CellObject * CellSense list) [,]) 
        (actorState : ActorState) 
        (action : Action) = 
        let currentCellSence = Helper.getCellSense maze (Helper.getPosition actorState)
        
        match action with
        | Forward -> 
            let xy = Helper.getNewPos maze actorState
            match xy with
            | Some(pos) -> 
                let cellObj = Helper.getCellObject maze pos
                //let newCellSense = Helper.getCellSense maze pos
                let newActorState = Helper.getNewGameState actorState pos
                match cellObj with
                    | Wumpus -> Eaten,  newActorState, maze
                    | Pit -> Fell,  newActorState, maze
                    | Gold -> Moved,  newActorState, maze
                    | Free -> Moved, newActorState, maze
                    | Start -> Moved,  newActorState,maze
            | None -> Bump,  actorState, maze
        | Left -> 
            match actorState with
            | E(x, y) -> Turned,  N(x, y),maze
            | W(x, y) -> Turned,  S(x, y),maze
            | S(x, y) -> Turned,  E(x, y),maze
            | N(x, y) -> Turned,  W(x, y),maze
        | Right -> 
            match actorState with
            | E(x, y) -> Turned,  S(x, y),maze
            | W(x, y) -> Turned,  N(x, y),maze
            | S(x, y) -> Turned,  W(x, y),maze
            | N(x, y) -> Turned,  E(x, y),maze
        | Grab -> 
                let pos = Helper.getPosition actorState
                let cellObj = Helper.getCellObject maze pos
                match cellObj with
                        | Gold ->   let x,y = pos
                                    let cellSenses = Helper.getCellSense maze pos |> Helper.filterGlitterCellSense
                                    maze.[x,y] <- Free,cellSenses
                                    Looted,  actorState, maze
                        | _ -> Nothing ,  actorState,maze
        | Shoot -> 
            let xy = Helper.getNewPos maze actorState
            match xy with
            | Some(pos) -> 
                let cellObj = Helper.getCellObject maze pos
                match cellObj with
                | Wumpus -> Screem,  actorState,(killWumpus maze pos)
                | _ -> Silence,  actorState,maze
            | None -> Silence,  actorState,maze
    

   
       
    let rec addCellObject maze cellObj sense  =
        let xMax, yMax = (maze |> Array2D.length1),(maze |> Array2D.length2)
        
        let pos = rand.Next(2, xMax), rand.Next(2, yMax)
        let x, y = pos
        let inline inMaze x y = 0 <= x && x < xMax && 0 <= y && y < yMax
        match Helper.getCellObject maze pos with
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
        
        let maze = Array2D.create xMax yMax (Free, List.empty<CellSense>)
        let inline inMaze x y = 0 <= x && x < xMax && 0 <= y && y < yMax
        maze.[0, 0] <- Start, List.empty<CellSense>
        let goldx, goldy = rand.Next(2, xMax), rand.Next(2, yMax)
        maze.[goldx, goldy] <- Gold, [Glitter]
      
        for _ in 1 .. pits do
            addCellObject maze Pit Breeze

        addCellObject maze Wumpus Stench

        maze    


