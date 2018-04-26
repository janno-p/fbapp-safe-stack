module Fable.Helpers.Vue

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import

[<Emit("h($0...)")>]
let createElement (tag: U3<string, Component, AsyncComponent>) (data: obj) (children: VNode[]): VNode = jsNative

let inline com (comp: ComponentOptions) =
    createElement (U3.Case3 (box comp)) createEmpty<obj> [||]

let inline domEl (tag: string) (props: obj list) (children: VNode list): VNode =
    createElement !^ tag (keyValueList CaseRules.LowerFirst props) (children |> Array.ofList)

let inline h1 b c = domEl "h1" b c
let inline p b c = domEl "p" b c

let [<Emit("$0")>] str (s: string): VNode = unbox s
