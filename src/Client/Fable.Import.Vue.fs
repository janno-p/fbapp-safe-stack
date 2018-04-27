namespace Fable.Import

open Fable.Core
open Fable.Import.Browser
open Fable.Import.JS
open System
open System.Collections.Generic
open System.Text.RegularExpressions

[<StringEnum>]
type RouterMode =
    | Hash
    | History
    | Abstract

type VNodeData =
    | Key of U2<string, int>
    | Slot of string
    | Class of obj
    | Attrs of obj

type VNode = obj
type VNodeChildren = obj
type CreateElement = Func<string, VNodeData, VNode[], VNode>
type AsyncComponent = obj

type VueConfiguration =
    abstract silent: bool with get, set
    abstract optionMergeStrategies: obj with get, set
    abstract devtools: bool with get, set
    abstract productionTip: bool with get, set
    abstract performance: bool with get, set
    abstract errorHandler: err: Error -> vm: Vue -> info: string -> unit
    abstract warnHandler: msg: string -> vm: Vue -> trace: string -> unit
    abstract ignoredElements: U2<string, Regex>[] with get, set
    abstract keyCodes: Dictionary<string, U2<int, int[]>> with get, set

and ComponentOptions =
    abstract data: (unit -> obj) with get, set
    abstract el: U2<Element, string> with get, set
    abstract name: string with get, set
    abstract render: (CreateElement -> VNode) with get, set
    abstract router: VueRouter with get, set

and VueRouter =
    [<Emit("new $0($1...)")>]
    abstract Create: RouterOptions option -> VueRouter

and RouterOptions =
    abstract mode: RouterMode with get, set
    abstract routes: RouteConfig[] with get, set

and RouteConfig =
    abstract path: string with get, set
    abstract name: string with get, set
    [<Emit("$0.component{{=$1}}")>]
    abstract component': Component with get, set
    abstract components: Dictionary<string, Component> with get, set
    //abstract redirect: RedirectOption with get, set
    abstract alias: U2<string, string[]> with get, set
    abstract children: RouteConfig[] with get, set
    abstract meta: obj with get, set
    //abstract beforeEnter: NavigationGuard with get, set
    //abstract props: U3<bool, obj, RoutePropsFunction> with get, set
    abstract caseSensitive: bool with get, set
    //abstract pathToRegexpOptions: PathToRegexpOptions with get, set

and Component = obj

and Vue =
    [<Emit("new $0($1...)")>]
    abstract Create: ComponentOptions -> Vue
    [<Emit("$0.extend($1...)")>]
    abstract Extend: ComponentOptions -> Vue
    [<Emit("$0.use($1...)")>]
    abstract Use: obj -> unit
    abstract config: VueConfiguration with get
