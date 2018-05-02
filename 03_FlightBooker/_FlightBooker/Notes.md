# Implementation notes


TODO: Red-highlight the fields if `model.DepartureError` or `model.ReturnError && returnEnabled` 

Now I think we're getting in to it. I took one pass at `update` and when it started to get ugly wrote a state-transition
diagram to help me out. Kinda' ironic to be thinking in terms of state machines when a major point is that you're
with immutable data. 

I think any more complex than this and I'm going to start factoring out the response functions in `update`. 

I'm not sure about how `Msg` grows. `CheckValidity`, `FlightTypeChanged of int` (totally coupled to view implementation),
and `BookFlight` seem like they should live in different abstraction realms. `BookFlight` is nicely high-level and 
precondition is that it's a valid flight, so that's lovely. `CheckValidity` is somewhat coupled with the UX although you
could make the argument that it's something reflexively done by the agent in the domain ("You can't book a flight that leaves
yesterday!"). But `FlightTypeChanged of int` is definitely an implementation artifact. 

The question of validation is a big one, I think. So far, my bias has been towards writing response functions that have,
as a precondition, a model valid for the response function. That certainly makes the "Happy Path" cleaner. But it puts
a burden on the developer sending the message and there is no type-system reinforcement. 

Thoughts: Can you / should you leverage the type system so that response functions are passed only the precise data they 
need? A criticism of the `Mdl` approach is that this is just the classic `ToyBoxOfContextArg` where, yeah, sure it 
_looks_ like a nice clean argument, but inside it's just chaos. It's also unchangeable since the crap inside it inevitably
becomes state-dependent and then you're just passing around what are essentially global variables. 

:thinking_face: The `Mdl` seems more like a ViewModel than a classic MVC Model -- it's not necessarily the data that is 
meaningful in the domain, it's the data that are necessary to render a View. So, if that's "proper", then can / how / should
MVVM's support for validation and data-binding be integrated into the architecture?

I don't think these questions are fleshed out in `Tempted`. Tempted to skip to `Crud` 