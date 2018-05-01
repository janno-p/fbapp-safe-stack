module FbApp.Client.Store.Store

open Fable.Core.JsInterop
open Fable.Import.Vue
open User

let Vue: Vue = importDefault "vue"
let Vuex: Vuex = importDefault "vuex"
let Store: Store<IRootState> = import "Store" "vuex"

Vue.Use(Vuex)

let createStore() =
    let options = createEmpty<StoreOptions<IRootState>>
    let store = Store.Create(options)
    user.registerModule(store)
    store
