namespace MattMS.CatGame.Browser

open MattMS.CatGame.Core

// Helpers
// =======

type Logger(name: string) =
  let write message =
    Browser.Dom.console.log (sprintf "%s %s" name message)

  member _.Debug = write

  member this.DebugValue name value = sprintf "%s = %A" name value |> this.Debug

// Quick module for making my HTML tags consistent.
module H =
  open Sutil
  open Sutil.DOM
  open Sutil.Attr

  let h1 label = Html.h1 [text label]
  let h2 label = Html.h2 [text label]
  let h3 label = Html.h3 [text label]
  let h4 label = Html.h4 [text label]
  let p label = Html.p [text label]
  let td label = Html.tableCell [text label]
  let th label = Html.tableHeader [text label]

  let button label clickHandler =
    Html.button [
      text label
      onClick clickHandler []
    ]

// Views
// =====

// Feeding
// -------

module FeedingGameView =
  open Sutil
  open Sutil.DOM
  open Thoth.Json

  type TableColumn =
    | CatLastMealCell
    | CatNameCell
    | CatWeightCell
    | FoodKindCell of FoodKind

  let localStorageKey = "MattMS.CatGame.Core.Game"

  let create () =
    let game = Game.init
    let gameStore: IStore<Game> = Store.make game

    // gameStore |> Store.write (fun game -> Browser.Dom.console.log (Encode.toString 2 game))
    gameStore |> Store.write (fun game -> Browser.WebStorage.localStorage.setItem (localStorageKey, Encode.toString 0 game))

    let cats = Store.map (fun game -> game.Player.Cats) gameStore

    let foodKindStore = Store.make [BudgetAir; BudgetLand; BudgetSea]

    let tableColumns = Store.map (List.map FoodKindCell >> List.append [CatNameCell; CatWeightCell; CatLastMealCell]) foodKindStore

    let headingText =
      function
      | CatLastMealCell -> "Last meal kind"
      | CatNameCell -> "Cat name"
      | CatWeightCell -> "Weight"
      | FoodKindCell foodKind -> Meal.name foodKind

    let tableHeadings = Store.map (List.map headingText) tableColumns

    Html.article [
      disposeOnUnmount [gameStore; foodKindStore]

      Html.table [
        Html.thead [
          Html.tr [
            each tableHeadings H.th []
          ]
        ]
        Html.tbody [
          eachi cats (fun (catIndex, cat) ->
            Html.tr [
              each tableColumns (
                fun columnType ->
                  match columnType with
                  | CatLastMealCell ->
                      match cat.Meals with
                      | [] -> text "Nothing"
                      | lastMeal :: _ -> lastMeal.Kind |> Meal.name |> text
                  | CatNameCell -> text (cat.Name |> Option.defaultValue $"Cat {catIndex + 1}")
                  | CatWeightCell -> text $"{cat.Weight}g"
                  | FoodKindCell foodKind ->
                      H.button "Feed" (fun _ -> Game.PlayerFeedsCat foodKind cat |> Store.modify <| gameStore)
                  |> Html.td
              ) []
            ]
          ) []
        ]
      ]
    ]

  let mount id =
    create () |> mountElement id

// Ranch
// -----

module RanchGameView =
  open Sutil
  open Sutil.DOM

  type Msg =
    | FeedCat of int * Cat * FoodKind
    | MilkCat of int * Cat
    | Sleep

  let init _ = Game.init

  let update msg game =
    match msg with
    | FeedCat (catIndex, cat, food) ->
      let log = Logger $"RanchGameView.update.{nameof FeedCat}"
      log.Debug "Started"
      Game.PlayerFeedsCat food cat game
    | MilkCat (catIndex, cat) ->
      let log = Logger $"RanchGameView.update.{nameof MilkCat}"
      log.DebugValue "Cat milk" cat.Milk
      game
    | Sleep ->
      let log = Logger $"RanchGameView.update.{nameof Sleep}"
      log.DebugValue "Finished day" game.World.Time
      Game.Sleep game

  let catDetailField name value =
    H.p $"{name} is {value}"

  let catDetailFieldWithUnit name value measure =
    catDetailField name $"{value} {measure}"

  // let foods = rancherStore |> Store.map (fun r -> r.FoodHeld |> Map.toList |> List.map fst)
  let foodKindStore = Store.make [BudgetLand; BudgetSea]

  let create () =
    let log = Logger "RanchGameView.view"

    let gameStore, dispatch = Store.makeElmishSimple init update ignore ()

    gameStore |> Store.write (fun _ -> log.Debug "`Store.write` called")

    let cats = gameStore |> Store.map (fun game -> game.Player.Cats)
    let foodHeld = gameStore |> Store.map (fun game -> game.Player.FoodHeld)

    Html.article [
      disposeOnUnmount [gameStore; foodKindStore]
      // bindFragment rancherStore <| fun rancher -> Html.div [
      Html.div [
        H.h1 "Ranch"
        bindFragment gameStore <| fun game ->
          Html.p [
            text $"Day {game.World.Time}"
            H.button "Sleep" <| fun _ -> dispatch Sleep
          ]
        eachi cats (fun (i, cat) ->
          let catStore = Store.make cat
          Html.article [
            disposeOnUnmount [catStore]
            bindFragment catStore <| fun cat ->
              Html.section [
                H.h2 (cat.Name |> Option.defaultValue $"Cat {i + 1}")
                catDetailFieldWithUnit "Current weight" cat.Weight "grams"
                H.p $"You must keep this cat between {cat.WeightMin} and {cat.WeightMax} grams."
              ]
            bindFragment catStore <| fun cat ->
              Html.section [
                H.h3 "Feeding"
                match cat.Meals with
                | [] ->
                  H.p "This poor cat has an empty stomach!"
                | lastMeal :: _ ->
                  H.p $"Last meal was {lastMeal.Weight} grams of {lastMeal.Kind |> Meal.name}"
                catDetailFieldWithUnit "Feed size" cat.IdealMealSize "grams"
                each foodKindStore (fun kind ->
                  bindFragment foodHeld <| fun foodHeld ->
                    Html.p [
                      text $"You have {foodHeld |> Map.tryFind kind |> Option.defaultValue 0<gram>} grams of {Meal.name kind}"
                      H.button "Feed" <| fun _ ->
                        FeedCat (i, cat, kind) |> dispatch
                    ]
                ) []
              ]
            bindFragment catStore <| fun cat ->
              Html.section [
                H.h3 "Milking"
                H.p $"This cat contains {cat.Milk}ml of milk."
                H.p $"""The milk flavour is {cat |> Cat.MilkFlavour |> Option.map Meal.name |> Option.defaultValue "nothing yet"}"""
                Html.div [
                  H.button "Milk" <| fun _ ->
                    MilkCat (i, cat) |> dispatch
                ]
              ]
          ]
        ) []
      ]
    ]

  let mount id =
    create () |> mountElement id

// Shop
// ----

module ShopGameView =
  open Sutil
  open Sutil.DOM

  type Msg = | Inc

  let init _ = Game.init

  let update msg state =
    match msg with
    | Inc -> state

  // let foodKindStore = Store.make [BudgetLand; BudgetSea]

  let view () =
    let state, dispatch = Store.makeElmishSimple init update ignore ()

    Html.article [
      disposeOnUnmount [state]

      H.h1 "Shop"
    ]

  let mount id =
    view () |> mountElement id
