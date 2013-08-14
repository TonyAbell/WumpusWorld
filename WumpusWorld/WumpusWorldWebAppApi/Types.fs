namespace WumpusWorld

    type CellObject = 
        | Wumpus
        | Pit
        | Gold
        | Free
        | Start
        override this.ToString() = 
            match this with
            | Wumpus -> "Wumpus"
            | Pit -> "Pit"
            | Gold -> "Gold"
            | Free -> "Free"
            | Start -> "Start"

    type CellSenses = 
        | Stench
        | Breeze
        | Glitter              
        override this.ToString() = 
            match this with
            | Stench -> "Stench"
            | Breeze -> "Breeze"
            | Glitter -> "Glitter"
        

    type Action = 
        | Forward
        | Left
        | Right
        | Shoot
        | Grab 

    type ActorState = 
        | E of int * int
        | W of int * int
        | S of int * int
        | N of int * int

    type ActionSenses = 
        | Bump
        | Screem
        | Silence
        | Moved
        | Turned
        | Fell
        | Eaten
        | Looted
        | Nothing
        override this.ToString() = 
            match this with
            | Bump -> "Bump"
            | Screem -> "Screem"
            | Silence -> "Silence"
            | Moved -> "Moved"
            | Turned -> "Turned"
            | Fell -> "Fell"
            | Eaten -> "Eaten"
            | Looted -> "Looted"
            | Nothing -> "Nothing"
