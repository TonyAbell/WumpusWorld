namespace WumpusWorld
open Microsoft.FSharp.Reflection 
open System.Reflection 
open System.Runtime.Serialization 
open System.Xml
open System
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Auth
open Microsoft.WindowsAzure.Storage.Table

    type GameState() =
        inherit TableEntity()
        member val XPos = 0 with get,set
        member val YPos = 0 with get,set
        member val Direction = "" with get,set

    type GameLog() =
        inherit TableEntity()    
        member val Action = "" with get,set
        
    type Board() =
        inherit TableEntity()      
        member val MapData = "" with get,set
        member val Size = 0 with get,set
        member val Pits = 0 with get,set




    [<Serializable>]
    type CellObject = 
        | Wumpus
        | Pit
        | Gold
        | Free
        | Start
        override this.ToString() = 
            match this with 
                |  Wumpus -> "Wumpus"
                |  Pit -> "Pit "
                |  Gold -> "Gold"
                |  Free -> "Free"
                |  Start -> "Strt"       

    [<Serializable>]
    type CellSense = 
        | Stench
        | Breeze
        | Glitter              
        override this.ToString() = 
            match this with 
                 | Stench  -> "Stench"
                 | Breeze  -> "Breeze"
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
        override this.ToString() = 
            match this with
            | E(x,y) -> sprintf "%s(%i,%i)" "E" x y
            | W(x,y) -> sprintf "%s(%i,%i)" "W" x y
            | S(x,y) -> sprintf "%s(%i,%i)" "S" x y
            | N(x,y) -> sprintf "%s(%i,%i)" "N" x y

    type ActionSense = 
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
