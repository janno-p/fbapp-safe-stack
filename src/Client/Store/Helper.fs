module FbApp.Client.Store.Helper

open Fable.Core.JsInterop
open Fable.Import.JS
open Fable.Import.Vue

let useRootNamespace = createObj ["root" ==> true]

let qualifyKey (namespaceName: string) (key: string) =
     createObj [
        "key" ==> key
        "namespacedKey" ==> (sprintf "%s/%s" namespaceName key)
     ]

type DynamicModuleBuilder<'S, 'R> (moduleNamespace: string, initialState: 'S) =
    let getters = GetterTree<'S, 'R>()
    let mutations = MutationTree<'S>()
    let actions = ActionTree<'S, 'R>()
    let mutable vuexStore = Option<Store<'R>>.None
    let mutable vuexModule = Option<Module<'S, 'R>>.None

    member __.state(): (unit -> 'S) =
        if moduleNamespace.IndexOf("/") < 0 then
            (fun () -> vuexStore.Value.state?(moduleNamespace) |> unbox<'S>)
        else
            let namespaces = moduleNamespace.Split('/')
            (fun () ->
                let mutable accessor = vuexStore.Value.state |> box
                for ns in namespaces do
                    accessor <- accessor?(ns)
                accessor |> unbox<'S>
            )

    member __.commit(handler: MutationHandler<'S, 'P>, name: string): ('P -> unit) =
        let (key: string), (namespacedKey: string) =
            let x = qualifyKey moduleNamespace name
            x?key |> unbox, x?namespacedKey |> unbox
        if mutations.ContainsKey(key) then
            failwithf "There is already a mutation named %s." key
        mutations.Add(key, handler |> unbox<Mutation<'S>>)
        (fun payload -> vuexStore.Value.commit(namespacedKey, payload, useRootNamespace |> unbox<CommitOptions>))

    member __.dispatch(handler: ActionHandler<'S, 'R, 'P, 'T>, name: string): ('P -> Promise<'T>) =
        let (key: string), (namespacedKey: string) =
            let x = qualifyKey moduleNamespace name
            x?key |> unbox, x?namespacedKey |> unbox
        if actions.ContainsKey(key) then
            failwithf "There is already an action named %s." key
        actions.Add(key, handler |> unbox<Action<'S, 'R>>)
        (fun payload -> vuexStore.Value.dispatch(namespacedKey, payload, useRootNamespace |> unbox<DispatchOptions>))

    member __.read(handler: GetterHandler<'S, 'R, 'T>, name: string): (unit -> 'T) =
        let (key: string), (namespacedKey: string) =
            let x = qualifyKey moduleNamespace name
            x?key |> unbox, x?namespacedKey |> unbox
        if getters.ContainsKey(key) then
            failwithf "There is already an getter named %s." key
        getters.Add(key, handler |> unbox<Getter<'S, 'R>>)
        (fun () -> vuexStore.Value.getters?(namespacedKey) |> unbox<'T>)

    member __.register(store: Store<'R>) =
        if vuexStore.IsSome then
            failwith "Module is already registered with the store."
        vuexStore <- Some(store)
        if vuexModule.IsNone then
            vuexModule <- Some(
                createObj [
                    "actions" ==> (fun _ -> actions)
                    "getters" ==> (fun _ -> getters)
                    "mutations" ==> (fun _ -> mutations)
                    "namespaced" ==> true
                    "state" ==> initialState
                ] |> unbox
            )
        vuexStore.Value.registerModule([| moduleNamespace |], vuexModule.Value, Some createEmpty<ModuleOptions>)

    member __.unregister() =
        vuexStore.Value.unregisterModule([| moduleNamespace |])
        vuexStore <- None
