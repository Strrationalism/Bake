namespace Bake

open System
open System.Reflection

[<AttributeUsage(AttributeTargets.Property)>]
type BakeActionAttribute () =
    inherit System.Attribute ()

type BakeAction = {
    action : BakeActionContext -> Script -> Task seq * BakeActionContext
    help : string
    usage : string list
    example : string list
}

and Runner = BakeActionContext -> Script seq -> Task seq * BakeActionContext

and BakeActionContext = {
    variables : Map<string, string>
    actions : Map<string, BakeAction>
    loadedModules : string Set
    runChildBlock : Runner
}

module Action =
    exception ActionUsageError of string
    
    let getActionsFromAssembly (assembly: Assembly) =
        let toBakeAction (action: PropertyInfo) = 

            let packAction (action: PropertyInfo) =
                action.GetMethod.Invoke(null, [||]) :?> BakeAction

            action.GetCustomAttributes(typeof<BakeActionAttribute>)
            |> Seq.tryExactlyOne
            |> Option.map (fun attr ->
                let attr = attr :?> BakeActionAttribute
                action.Name, packAction action)
            
        
        assembly.GetExportedTypes ()
        |> Seq.collect (fun t ->
            t.GetProperties ()
            |> Seq.choose toBakeAction)

    let getActionsFromDLL = Assembly.LoadFrom >> getActionsFromAssembly

    exception ActionNotFound of string
    exception ActionException of Script * BakeActionContext * exn
    let run ctx script =
        match ctx.actions |> Map.tryFind script.command with
        | None -> raise <| ActionNotFound script.command
        | Some action -> 
            try action.action ctx script
            with e -> raise <| ActionException (script, ctx, e)
