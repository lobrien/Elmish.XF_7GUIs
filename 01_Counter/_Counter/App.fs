namespace Counter

open Elmish
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

type Model = { Count : int }

type Msg = Increment 

type CounterApp () = 
    inherit Application ()

    let initModel = { Count = 0 }
    let init () = initModel, Cmd.none

    let update (msg : Msg) (model : Model) : Model * Cmd<'a> =
        // Not really necessary with a single case, but shows the fundamental pattern...
        match msg with
        | Increment -> { model with Count = model.Count + 1 }, Cmd.none

    let view (model: Model) (dispatch : Msg -> unit) : XamlElement =
      Xaml.ContentPage(
        content=Xaml.StackLayout(padding=20.0,
          children=[ 
            yield Xaml.StackLayout(padding=20.0, verticalOptions=LayoutOptions.Center,
              children=[
                 Xaml.Label(text= sprintf "%d" model.Count, horizontalOptions=LayoutOptions.Center, fontSize = "Large")
                 Xaml.Button(text="Increment", command= (fun () -> dispatch Increment))
                ])
          ]))

    do
        let page = 
            Program.mkProgram init update view
            |> Program.withConsoleTrace
            |> Program.withDynamicView
            |> Program.run

        do PlatformConfiguration.iOSSpecific.Page.SetUseSafeArea(page.On<PlatformConfiguration.iOS>(), true) |> ignore

        base.MainPage <- page
