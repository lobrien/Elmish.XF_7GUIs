namespace FlightBooker

open Elmish
open Elmish.XamarinForms
open Elmish.XamarinForms.DynamicViews
open Xamarin.Forms
open System

type TripType = 
    | OneWay of DateTime
    | RoundTrip of (DateTime * DateTime) 
   
type Model = { 
    Trip : TripType
    DepartureError : bool; ReturnError : bool;  
    Msg : string
    }

type Msg = 
    | SetDeparture of DateTime 
    | SetReturnDate of DateTime
    | CheckTripValidity
    | FlightTypeChanged of int
    | BookFlight

type App () = 
    inherit Application ()

    let initModel = { 
        Trip = OneWay DateTime.Now;
        DepartureError = false;
        ReturnError = false;
        Msg = "Valid one-way" }
    let init () = initModel, Cmd.none

    let ValidDepartureDate (d : DateTime) = d > DateTime.Now

    let ValidReturnDate (departure : DateTime) (returnDate : DateTime) = returnDate > departure
 
    let pickerItems = [ "One-Way"; "Round-trip" ]

    let update msg model =
        match msg with
        | SetDeparture departure -> 
            let newTrip =  match model.Trip with 
                                | OneWay _ -> { model with Trip = OneWay departure }
                                | RoundTrip rt -> { model with Trip = RoundTrip (departure, (snd rt)) }
            newTrip, CheckTripValidity |> Cmd.ofMsg
        | SetReturnDate returnDate -> 
            let newTrip = match model.Trip with 
                            | OneWay _ -> raise <| new Exception("Unexpected state change: assigning return on 1W")
                            | RoundTrip rt -> { model with Trip = RoundTrip ((fst rt), returnDate) }
            newTrip, CheckTripValidity |> Cmd.ofMsg
        | CheckTripValidity -> 
            let (departureValid, returnValid) = match model.Trip with 
                                                | OneWay departure -> ValidDepartureDate departure, true 
                                                | RoundTrip (departure, returnDate) -> ValidDepartureDate departure, ValidReturnDate departure returnDate
            let msg = match (departureValid, returnValid) with 
                      | false, false -> "Both dates bad"
                      | false, true -> "Bad departure date"
                      | true, false -> "Bad return date"
                      | true, true -> "Valid trip"
            { model with DepartureError = not departureValid; ReturnError = not returnValid; Msg = msg }, Cmd.none
        | FlightTypeChanged ix -> 
            let departureDate = match model.Trip with 
                                | OneWay t -> t 
                                | RoundTrip (t, _) -> t
            let newTrip = match pickerItems.[ix] with 
                          | "One-Way" -> OneWay departureDate
                          | "Round-trip" -> RoundTrip (departureDate, departureDate.AddDays(1.)) 
                          | unexpected -> raise <| new ArgumentOutOfRangeException(unexpected)
            { model with Trip = newTrip }, CheckTripValidity |> Cmd.ofMsg

        | BookFlight -> { model with Msg = "Booking Confirmed" }, Cmd.none


    let view (model: Model) (dispatch : Msg -> unit) =
        let selectedIndex = match model.Trip with 
                            | OneWay _ -> 0
                            | RoundTrip _ -> 1

        let departureDate = match model.Trip with 
                            | OneWay t -> t 
                            | RoundTrip (t, _) -> t

        let returnDate, returnEnabled = match model.Trip with 
                                        | OneWay t -> t.AddDays(1.), false 
                                        | RoundTrip (_, r) -> r, true

        let bookingEnabled = not (model.DepartureError || model.ReturnError)
        Xaml.ContentPage(
            content=Xaml.StackLayout(padding=20.0,
                children=[ 
                    yield Xaml.StackLayout(padding=20.0, verticalOptions=LayoutOptions.Center,
                      children=[
                        Xaml.Label(text="Flight type:")
                        Xaml.Picker(title="Flight kind:", selectedIndex= selectedIndex , itemsSource= pickerItems, horizontalOptions=LayoutOptions.CenterAndExpand,selectedIndexChanged=(fun (i, item) -> dispatch (FlightTypeChanged i)))
                        Xaml.Label(text= "Departure Date:")
                        Xaml.DatePicker (date = departureDate, dateSelected = (fun (d) -> d.NewDate |> SetDeparture |> dispatch))
                        Xaml.Label(text = "Return Date:")
                        Xaml.DatePicker (date = returnDate, isEnabled = returnEnabled, dateSelected = (fun (d) -> d.NewDate |> SetReturnDate |> dispatch))
                        Xaml.Button(text="Submit", isEnabled = bookingEnabled, command= (fun () -> dispatch BookFlight))
                        Xaml.Label(text = model.Msg)
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
