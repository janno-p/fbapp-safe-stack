module FbApp.Components.Home

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Helpers.Vue

let private comp = createEmpty<ComponentOptions>

comp.render <-
    (fun h ->
        div [Class "jumbotron" (* v-once *)] [
            h1 [] [str "Ennustusmäng"]
            p [] [
                str "Ajavahemikus 14. juunist 15. juulini toimuvad Venemaal 2018. aasta jalgpalli
                    maailmameistrivõistlused. Lisaks rahvusmeeskondade mõõduvõtmistele pakub antud veebileht
                    omavahelist võistlusmomenti ka tugitoolisportlastele tulemuste ennustamise näol."
            ]
            p [] [
                str "Oma eelistusi saad valida ja muuta kuni avamänguni 14. juunil kell 18:00. Pärast seda on
                    võimalik sama veebilehe vahendusel jälgida, kuidas tegelikud tulemused kujunevad ning kui täpselt
                    need Sinu või teiste ennustustega kokku langevad."
            ]
            p [] [str "Auhinnaks lühiajaline au ja kuulsus."]
            p [] [
                routerLink [Class "btn btn-lg btn-success"; Attrs !!["to" ==> "/predictions/predict"; "role" ==> "button"]] [
                    str "Tee oma ennustused »"
                ]
            ]
        ])

[<ExportDefault>]
let exports = comp
