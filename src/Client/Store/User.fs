module FbApp.Client.Store.User

open Fable.Core.JsInterop
open Fable.Import.Vue
open Helper

type UserState () =
    member val isAuthenticated = false with get, set

type IRootState =
    interface
    end

let private b = DynamicModuleBuilder<UserState, IRootState>("user", UserState())

let private setAuthenticated =
    System.Action<_,_>(
        fun (state: UserState) (isAuthenticated: bool) ->
            state.isAuthenticated <- isAuthenticated
    )

let private stateGetter = b.state()

type UserModule () =
    member __.state with get() = stateGetter()
    member val commitSetAuthenticated = b.commit(setAuthenticated, "setAuthenticated")
    member __.registerModule(store) = b.register(store)

let user = UserModule()
