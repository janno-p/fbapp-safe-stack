module FbApp.Store

open Fable.Core.JsInterop
open Fable.Import.Vue
open FbApp.Store.User

let Vue: Vue = importDefault "vue"
let Vuex: Vuex = importDefault "vuex"
let Store: Store<IRootState> = import "Store" "vuex"

Vue.Use(Vuex)

let storeOptions = createEmpty<StoreOptions<IRootState>>

let store = Store.Create(storeOptions)
user.registerModule(store)
