namespace TempConverter

open Elmish
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms

type Model = { 
    TempF : float
    TempC : float }

type Msg = 
    | ValidateF of string
    | ValidateC of string
    | SetViaF of float
    | SetViaC of float

type TempConverterApp () = 
    inherit Application ()

    let initModel = { TempF = 32.; TempC = 0. }
    let init () = initModel, Cmd.none

    let update msg model =
        match msg with
        | SetViaF newF -> { model with TempF = newF;  TempC = (newF - 32.) * 5./9.}, Cmd.none
        | SetViaC newC -> { model with TempC = newC;  TempF = newC * 9./5. + 32.}, Cmd.none
        | ValidateF textF -> 
            match System.Double.TryParse(textF) with
               | (true, newF) -> model, SetViaF newF |> Cmd.ofMsg 
               | _ -> model, Cmd.none
        | ValidateC textC -> 
            match System.Double.TryParse(textC) with
               | (true, newC) -> model, SetViaC newC |> Cmd.ofMsg
               | _ -> model, Cmd.none

    let view model dispatch =
      Xaml.ContentPage(
        content=Xaml.StackLayout(padding=20.0,
          children=[ 
            yield Xaml.StackLayout(padding=20.0, verticalOptions=LayoutOptions.Center,
              children=[
                 Xaml.Entry(text = sprintf "%.1f" model.TempF, completed = (fun (txt) -> ValidateF txt |> dispatch))
                 Xaml.Entry(text = sprintf "%.1f" model.TempC, completed = (fun (txt) -> ValidateC txt |> dispatch))
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
