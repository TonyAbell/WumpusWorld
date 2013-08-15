namespace WumpusWorld
open Microsoft.FSharp.Reflection 
open System.Reflection 
open System.Runtime.Serialization 
open System.Xml

    [<KnownType("KnownTypes")>] 
    type CellObject = 
        | Wumpus
        | Pit
        | Gold
        | Free
        | Start
        override this.ToString() = 
            let dcs = new DataContractSerializer(typeof<CellObject>)
            let sb = new System.Text.StringBuilder()
            let xw = XmlWriter.Create(sb)
            dcs.WriteObject(xw, this)
            xw.Close()
            sb.ToString()           

        static member KnownTypes() = // KnownTypes uses the F# reflection API to select only the union types
            typeof<CellObject>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic) |> Array.filter FSharpType.IsUnion 



    [<KnownType("KnownTypes")>] 
    type CellSenses = 
        | Stench
        | Breeze
        | Glitter              
        override this.ToString() = 
            let dcs = new DataContractSerializer(typeof<CellSenses>)
            let sb = new System.Text.StringBuilder()
            let xw = XmlWriter.Create(sb)
            dcs.WriteObject(xw, this)
            xw.Close()
            sb.ToString()     
        static member KnownTypes() = // KnownTypes uses the F# reflection API to select only the union types
            typeof<CellSenses>.GetNestedTypes(BindingFlags.Public ||| BindingFlags.NonPublic) |> Array.filter FSharpType.IsUnion 

        

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
