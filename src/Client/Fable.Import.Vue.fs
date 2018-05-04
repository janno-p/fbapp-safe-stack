module rec Fable.Import.Vue

open Fable.Core
open Fable.Import.Browser
open Fable.Import.JS
open System
open System.Collections.Generic
open System.Text.RegularExpressions

open Fable.Import.JS

type DispatchOptions =
    abstract root: bool with get, set

type CommitOptions =
    abstract silent: bool with get, set
    abstract root: bool with get, set

type ActionContext<'S, 'R> =
    abstract dispatch: payload: 'P * options: DispatchOptions -> Promise<obj>
    abstract commit: payload: 'P * options: CommitOptions -> unit
    abstract state: 'S with get, set
    abstract getters: obj with get, set
    abstract rootState: 'R with get, set
    abstract rootGetters: obj with get, set

type ActionHandler<'S, 'R> = System.Func<ActionContext<'S, 'R>, obj, obj>

type ActionObject<'S, 'R> =
    abstract root: bool with get, set
    abstract handler: ActionHandler<'S, 'R> with get, set

type Action<'S, 'R> = U2<ActionHandler<'S, 'R>, ActionObject<'S, 'R>>
type ActionTree<'S, 'R> = Dictionary<string, Action<'S, 'R>>

type Getter<'S, 'R> = System.Func<'S, obj, 'R, obj, obj>
type GetterTree<'S, 'R> = Dictionary<string, Getter<'S, 'R>>

type Mutation<'S> = System.Func<'S, obj, obj>
type MutationTree<'S> = Dictionary<string, Mutation<'S>>

type WatchOptions =
    abstract deep: bool with get, set
    abstract immediate: bool with get, set

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
    | On of obj
    | NativeOn of obj
    | Props of obj
    | DomProps of obj
    | Style of obj

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
    abstract computed: obj with get, set
    abstract data: (unit -> obj) with get, set
    abstract el: U2<Element, string> with get, set
    abstract name: string with get, set
    abstract render: (CreateElement -> VNode) with get, set
    abstract router: VueRouter with get, set
    abstract store: Store<obj> with get, set

and VueRouter =
    [<Emit("new $0($1...)")>]
    abstract Create: RouterOptions option -> VueRouter

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
    abstract ``$router``: VueRouter with get
    [<Emit("$0.$route")>]
    abstract ``$route``: Route with get
    abstract ``$store``: Store<obj> with get

and ModuleTree<'R> = Dictionary<string, Module<obj, 'R>>

and Module<'S, 'R> =
    abstract namespaced: bool with get, set
    abstract state: U2<'S, unit -> 'S> with get, set
    abstract getters: GetterTree<'S, 'R> with get, set
    abstract actions: ActionTree<'S, 'R> with get, set
    abstract mutations: MutationTree<'S> with get, set
    abstract modules: ModuleTree<'R> with get, set

and Plugin<'S> = System.Func<Store<'S>, obj>

and StoreOptions<'S> =
    abstract state: 'S with get, set
    abstract getters: GetterTree<'S, 'S> with get, set
    abstract actions: ActionTree<'S, 'S> with get, set
    abstract mutations: MutationTree<'S> with get, set
    abstract modules: ModuleTree<'S> with get, set
    abstract plugins: Plugin<'S>[] with get, set
    abstract strict: bool with get, set

and [<Import("Store", "vuex")>] Store<'S> =
    [<Emit("new $0($1...)")>]
    abstract Create: StoreOptions<'S> -> Store<'S>
    abstract state: 'S with get
    abstract getters: obj with get
    abstract replaceState: state: 'S -> unit
    abstract dispatch: key: string * payload: 'P * options: DispatchOptions -> Promise<'T>
    abstract commit: key: string * payload: 'P * options: CommitOptions -> unit
    abstract subscribe: fn: ('P * 'S -> obj) -> (unit -> unit)
    abstract subscribeAction: fn: ('P * 'S -> obj) -> (unit -> unit)
    abstract watch: getter: ('S * obj -> 'T) * cb: ('T * 'T -> unit) * options: WatchOptions option -> (unit -> unit)
    abstract registerModule: path: string[] * m: Module<'T, 'S> * options: ModuleOptions option -> unit
    abstract unregisterModule: path: string[] -> unit
    abstract hotUpdate: options: HotUpdateOptions<'S> -> unit  

and HotUpdateOptions<'S> =
    abstract actions: ActionTree<'S, 'S> with get, set
    abstract mutations: MutationTree<'S> with get, set
    abstract getters: GetterTree<'S, 'S> with get, set
    abstract modules: ModuleTree<'S> with get, set

and ModuleOptions =
    abstract preserveState: bool with get, set

type BareActionContext<'S, 'R> =
    abstract state: 'S with get, set
    abstract rootState: 'R with get, set

type ActionHandler<'S, 'R, 'P, 'T> = System.Func<BareActionContext<'S, 'R>, 'P, U2<'T, Promise<'T>>>
type GetterHandler<'S, 'R, 'T> = System.Func<'S, 'R, 'T>
type MutationHandler<'S, 'P> = System.Action<'S, 'P>

type Vuex =
    interface
    end

type PositionResultType =
    abstract selector: string with get, set
    abstract offset: Position option with get, set

type PositionResult =
    U3<Position, PositionResultType, unit>

type RawLocation =
    U2<string, Location>

type RedirectOption =
    U2<RawLocation, System.Func<Route, RawLocation>>

type Route =
    abstract path: string with get
    abstract name: string option with get
    abstract hash: string with get
    abstract query: Dictionary<string, U2<string, string[]>> with get
    abstract ``params``: Dictionary<string, string> with get
    abstract fullPath: string with get
    abstract matched: RouteRecord[] with get
    abstract redirectedFrom: string option with get
    abstract meta: obj option

type RouteRecord =
    abstract path: string with get
    abstract regex: Regex with get
    abstract components: Dictionary<string, Component> with get
    abstract instances: Dictionary<string, Vue> with get
    abstract name: string option with get
    abstract parent: RouteRecord option with get
    abstract redirect: RedirectOption option with get
    abstract matchAs: string option with get
    abstract meta: obj with get
    abstract beforeEnter: route: Route * redirect: (RawLocation -> unit) * next: (unit -> unit) -> obj
    abstract props: U4<bool, obj, RoutePropsFunction, Dictionary<string, U3<bool, obj, RoutePropsFunction>>>

type RoutePropsFunction =
    System.Func<Route, obj>

type RouterOptions =
    abstract routes: RouteConfig[] with get, set
    abstract mode: RouterMode with get, set
    abstract fallback: bool with get, set
    [<Emit("$0.base{{=$1}}")>]
    abstract ``base``: string with get, set
    abstract linkActiveClass: string with get, set
    abstract linkExactActiveClass: string with get, set
    abstract parseQuery: System.Func<string, obj> with get, set
    abstract stringifyQuery: System.Func<obj, string> with get, set
    abstract scrollBehavior: System.Func<Route, Route, U2<Position, unit>, U2<PositionResult, Promise<PositionResult>>> with get, set
