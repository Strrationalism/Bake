module Bake.Actions.Action

open Bake

[<BakeAction>]
let Action = {
    help = "创建自定义动作"

    usage = [
        """Action 动作名称 参数列表 { 代码块 }"""
    ]

    example = [
        """Action PrintStr str { Print str } """
    ]

    action = fun ctx script ->
        let actionName = script.arguments |> List.head
        let body = script.arguments |> List.last
        let parameters = script.arguments.[1..List.length script.arguments - 2]

        let action = {
            help = actionName
            usage = [ actionName + " " + List.reduce (fun a b -> a + " " + b) parameters ]
            example = []
            action = fun ctx script ->
                script.arguments
                |> List.fold2 
                    (fun (body: string) parameterName argument -> body.Replace (parameterName, argument))
                    body
                    parameters
                |> Parser.parseScripts script.scriptFile
                |> function
                | Error e -> raise e
                | Ok x -> ctx.runChildBlock ctx <| Seq.cast x
        }

        Seq.empty,
        Import.importAction ctx actionName action
}