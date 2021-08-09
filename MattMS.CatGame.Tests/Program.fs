open Fuchu
open MattMS.CatGame.Core

[<Tests>]
let catTests =
  testList "Cat" [
    testCase "Cat.Feed" <|
      fun _ ->
        let cat =
          {
            Cat.init with
              IdealMealSize = 0x40<gram>
              Meals = List.empty
          }

        let food = {Kind = PremiumAir; Weight = 0x100<gram>}

        Assert.Equal("Cat has not been fed", true, (List.isEmpty cat.Meals))

        let fedCat, leftover = Cat.Feed food cat

        Assert.Equal(
          "Cat receives food",
          64<gram>,
          (fedCat.Meals |> List.tryHead |> Option.map (fun m -> m.Weight) |> Option.defaultValue 0<gram>)
        )
        Assert.Equal("Leftover food is reduced", 0xc0<gram>, leftover)
  ]

[<Tests>]
let ``player feeds cat tests`` =
  let ``get first cat of player in game`` game = game.Player.Cats.Head

  let ``feed first cat of player in game`` foodKind game =
    Game.PlayerFeedsCat foodKind (``get first cat of player in game`` game) game

  let mealSize = 0x20<gram>
  let startCatWeight = 0x7ff<gram>
  let startFoodHeld = mealSize * 2

  let startCat =
    {
      Cat.init with
        IdealMealSize = mealSize
        Meals = List.empty
        Weight = startCatWeight
        WeightMax = startCatWeight + (mealSize * 8)
        WeightMin = startCatWeight - (mealSize * 8)
    }

  testList $"{nameof Game}.{nameof Game.PlayerFeedsCat}" [
    testCase "Feed all food" <|
      fun _ ->
        let startGame =
          {
            Player = {
              Rancher.init with
                Cats =
                  [
                    startCat
                  ]
                FoodHeld = Map.ofList [BudgetLand, startFoodHeld]
            }
            World = World.init
          }

        let ``assert that meals are eaten`` (mealCount: int) foodKind game =
          Assert.Equal("Player has 1 cat", 1, (game.Player.Cats.Length))
          let cat = ``get first cat of player in game`` game

          Assert.Equal($"Cat has been fed {mealCount} meals", mealCount, (cat.Meals.Length))

          let correctWeight = startCatWeight + (startCat.IdealMealSize * mealCount)
          Assert.Equal($"Cat has correct weight at {mealCount} meals", correctWeight, cat.Weight)

          let foodHeld = game.Player.FoodHeld |> Map.tryFind foodKind
          Assert.Equal($"Meal {mealCount} has taken correct food held", Some (startFoodHeld - (mealCount * mealSize)), foodHeld)

          let meal = cat.Meals.Head
          Assert.Equal($"Meal {mealCount} has correct kind", foodKind, meal.Kind)
          Assert.Equal($"Meal {mealCount} has correct weight", startCat.IdealMealSize, meal.Weight)
          game

        let ``assert that game is unchanged`` (lastGame: Game) game =
          let currentCat = ``get first cat of player in game`` game
          let lastCat = ``get first cat of player in game`` lastGame

          let currentMeal = currentCat.Meals.Head
          let lastMeal = lastCat.Meals.Head
          Assert.Equal("Last meal unchanged", lastMeal, currentMeal)

          let currentMeals = currentCat.Meals
          let lastMeals = lastCat.Meals
          Assert.Equal("All meals unchanged", lastMeals, currentMeals)

          Assert.Equal("Cat unchanged", lastCat, currentCat)

          let currentFoodHeld = game.Player.FoodHeld
          let lastFoodHeld = game.Player.FoodHeld
          Assert.Equal("All food held unchanged", lastFoodHeld, currentFoodHeld)

          Assert.Equal("Player unchanged", lastGame.Player, game.Player)
          game

        let lastGame =
          startGame
          |> ``feed first cat of player in game`` BudgetLand
          |> ``assert that meals are eaten`` 1 BudgetLand
          |> ``feed first cat of player in game`` BudgetLand
          |> ``assert that meals are eaten`` 2 BudgetLand

        lastGame
        |> ``feed first cat of player in game`` BudgetLand
        |> ``assert that game is unchanged`` lastGame
        |> ignore
  ]

[<EntryPoint>]
let main args = defaultMainThisAssembly args
