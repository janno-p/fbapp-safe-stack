module Fable.Helpers.Vue

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import

let [<Emit("$0")>] str (s: string): VNode = unbox s

[<Emit("h($0...)")>]
let createElement (tag: U3<string, Component, AsyncComponent>) (data: obj) (children: VNode[]): VNode = jsNative

let inline com (comp: ComponentOptions) =
    createElement (U3.Case3 (box comp)) createEmpty<obj> [||]

let inline domEl (tag: string) (props: VNodeData list) (children: VNode list): VNode =
    createElement !^ tag (keyValueList CaseRules.LowerFirst props) (children |> Array.ofList)

let inline button b c = domEl "button" b c
let inline div b c = domEl "div" b c
let inline footer b c = domEl "footer" b c
let inline h1 b c = domEl "h1" b c
let inline i b c = domEl "i" b c
let inline li b c = domEl "li" b c
let inline nav b c = domEl "nav" b c
let inline p b c = domEl "p" b c
let inline span b c = domEl "span" b c
let inline ul b c = domEl "ul" b c

let inline routerLink b c = domEl "router-link" b c
let inline routerView () = domEl "router-view" [] []

let (!!) = createObj
