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
    runChildBlock : Runner
}

module Action =
    exception MethodIsNotAnException of string
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

    let getActionsFromDLL = Assembly.LoadFile >> getActionsFromAssembly

    let applyContextToArgument actionContext (text: string) =
        actionContext.variables
        |> Map.fold (fun (text: string) k v -> text.Replace("$" + k, v)) text

    // 根据{}参数中每一行单独创建一个Task
    let blockArgumentTaskPerLine createTask ctx (script: Script) =
        Script.lines
        >> Seq.map Script.trimLineComment
        >> Script.trimLines
        >> Seq.map (applyContextToArgument ctx)
        >> Seq.collect (createTask ctx script)

    exception ActionUsageError of string

    // 一个只具有一个{}参数的指令，它每一行为一个Task，可使用此函数创建Action
    let singleBlockArgumentAction createTask ctx script =
        script.arguments
        |> Seq.tryExactlyOne
        |> function
        | None -> raise <| ActionUsageError "该指令只接受一个参数"
        | Some x -> blockArgumentTaskPerLine createTask ctx script x
        , ctx

    exception ActionNotFound of string
    let runAction ctx script =
        match ctx.actions |> Map.tryFind script.command with
        | None -> raise <| ActionNotFound script.command
        | Some action -> action.action ctx script
