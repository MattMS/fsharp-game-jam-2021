namespace MattMS.CatGame.Core

[<Measure>] type gram

[<Measure>] type money

type FoodKind = | PremiumAir | PremiumLand | PremiumSea | BudgetAir | BudgetLand | BudgetSea

type ProductKind = | Cheese | IceCream | Soap | SportsDrink

type Quality = | Premium | Budget

type Meal =
  {
    Kind: FoodKind
    // Time: uint
    Weight: int<gram>
  }

  static member name =
    function
    | PremiumAir -> "Eagle"
    | PremiumLand -> "Bear"
    | PremiumSea -> "Turtle"
    | BudgetAir -> "Seagull"
    | BudgetLand -> "Rodent"
    | BudgetSea -> "Jellyfish"
    // Caviar
    // Gecko meat for rock-climbing.

  static member quality =
    function
    | PremiumAir | PremiumLand | PremiumSea -> Premium
    | BudgetAir | BudgetLand | BudgetSea -> Budget

type Cat =
  {
    Alive: bool
    // GUID: System.Guid
    IdealMealSize: int<gram>
    Meals: Meal list
    Milk: uint
    // MilkMax: uint
    Name: string option
    Weight: int<gram>
    WeightMax: int<gram>
    WeightMin: int<gram>
  }

  // Number of meals that are "remembered" by a cat, and used to define the milk.
  static member MealMemory = 4

  static member init =
    {
      Alive = true
      // GUID = System.Guid.NewGuid()
      IdealMealSize = 0x80<gram>
      Meals = []
      Milk = 0u
      Name = None
      Weight = 0x7ff<gram>
      WeightMax = 0x9ff<gram>
      WeightMin = 0x5ff<gram>
    }

  // static member LastMealTime (cat: Cat) =
  //   cat.Meals |> List.tryHead |> Option.map (fun m -> m.Time) |> Option.defaultValue 0u

  static member Feed (mealOffer: Meal) (cat: Cat) =
    if mealOffer.Weight = 0<gram> then
      cat, mealOffer.Weight
    else
      let mealTaken = {mealOffer with Weight = min mealOffer.Weight cat.IdealMealSize}
      let fedCat =
        {
          cat with
            Meals = (mealTaken :: cat.Meals) //|> List.truncate Cat.MealMemory
            Weight = cat.Weight + mealTaken.Weight
        }
      fedCat, (mealOffer.Weight - mealTaken.Weight)

  // Need to ensure we deal with the case of not having any meals.
  static member MilkFlavour (cat: Cat) =
    let kinds: Map<FoodKind, int<gram>> = Map.empty
    let folder state meal =
      Map.change meal.Kind (Option.orElse (Some 0<gram>) >> Option.map ((+) meal.Weight)) state
    List.fold folder kinds cat.Meals
    |> Map.toList
    |> List.sortByDescending snd
    |> List.map fst
    |> List.tryHead

  // This could be changed to find the dominant flavour and control the quality on how many other flavours there are.
  static member MilkQuality (cat: Cat) =
    let meatFilter quality = List.filter (fun m -> m.Kind |> Meal.quality = quality)
    let premium = cat.Meals |> meatFilter Premium |> List.sumBy (fun m -> m.Weight)
    let budget = cat.Meals |> meatFilter Budget |> List.sumBy (fun m -> m.Weight)
    if (float premium) / (float budget) > 1.0 then Premium else Budget

  static member Sleep (cat: Cat) =
    // let safeWeight = (cat.WeightMin <= cat.Weight) && (cat.Weight <= cat.WeightMax)
    let stomachEmpty = List.isEmpty cat.Meals
    let underweight = cat.Weight < cat.WeightMin

    match stomachEmpty, underweight with
    | true, true ->
      {cat with Alive = false}
    | true, false ->
      {cat with Weight = cat.Weight - cat.IdealMealSize}
    | false, _ ->
      {
        cat with
          Meals = (cat.Meals |> List.rev |> List.tail |> List.rev) // Drop last meal
          Milk = cat.Milk + 10u
          Weight = cat.Weight - cat.IdealMealSize
      }

type FoodingSkillKind =
  | LandFoodingSkill
  | SkyFoodingSkill
  | SeaFoodingSkill

// type FoodingSkill() =
//   member _.Experience: uint = 0u

// type LandFoodingSkill() =
//   inherit FoodingSkill()

// type SeaFoodingSkill() =
//   inherit FoodingSkill()

// type SkyFoodingSkill() =
//   inherit FoodingSkill()

type Gang =
  {
    Name: string
  }

type Rancher =
  {
    Cats: Cat list
    FoodBait: Map<FoodKind, uint>
    FoodHeld: Map<FoodKind, int<gram>>
    Gang: Gang option
    // LandFoodingSkill: LandFoodingSkill
    // SeaFoodingSkill: SeaFoodingSkill
    // SkyFoodingSkill: SkyFoodingSkill
    Money: int<money>
  }

  static member init =
    {
      Cats = []
      FoodBait = Map.empty
      FoodHeld = Map.empty
      Gang = None
      // LandFoodingSkill = LandFoodingSkill()
      // SeaFoodingSkill = SeaFoodingSkill()
      // SkyFoodingSkill = SkyFoodingSkill()
      Money = 0<money>
    }

// let GetFoodingSkillNeeded =
//   function
//   | PremiumLand | BudgetLand -> LandFoodingSkill
//   | PremiumAir | BudgetAir -> SkyFoodingSkill
//   | PremiumSea | BudgetSea -> SeaFoodingSkill

type Shop =
  {
    FoodBaitCost: Map<FoodKind, uint>
    FoodCost: Map<FoodKind, uint>
  }

type World =
  {
    Time: uint
    Ranchers: Rancher list
  }

  static member init = {Time = 0u; Ranchers = []}

  // static member ProductCost (kind: ProductKind) (_: World) : Gang option =

  // static member ProductBlocker (kind: ProductKind) (_: World) : Gang option =
  //   match kind with
  //   | Cheese -> None
  //   | IceCream -> None
  //   | Soap -> None
  //   | SportsDrink -> None

type Game =
  {
    Player: Rancher
    World: World
  }

  static member init =
    {
      Player = {
        Rancher.init with
          Cats =
            [
              Cat.init
            ]
          // FoodHeld = Map.add PremiumAir 100<gram> Rancher.init.FoodHeld
          FoodHeld = Map.ofList [BudgetLand, 1024<gram>; BudgetSea, 1024<gram>]
          Money = 128<money>
      }
      World = World.init
    }

  static member PlayerFeedsCat foodKind cat game =
    let player = game.Player

    let meal =
      {
        Kind = foodKind
        Weight = player.FoodHeld |> Map.tryFind foodKind |> Option.defaultValue 0<gram>
      }

    let fedCat, leftover = Cat.Feed meal cat

    {
      game with
        Player = {
          player with
            // Cats = player.Cats |> List.mapi (fun i otherCat -> if i = catIndex then fedCat else otherCat)
            Cats =
              if cat <> fedCat then
                player.Cats |> List.except [cat] |> List.append [fedCat]
              else
                player.Cats
            // Using `Map.add` will replace the value if it already exists.
            FoodHeld = Map.add foodKind leftover player.FoodHeld
        }
    }

  static member Sleep game =
    {
      game with
        Player =
          {
            game.Player with
              Cats = game.Player.Cats |> List.map Cat.Sleep
          }
        World =
          {
            game.World with
              Ranchers = game.World.Ranchers |> List.map (fun rancher ->
                {
                  rancher with
                    Cats = rancher.Cats |> List.map Cat.Sleep
                }
              )
              Time = game.World.Time + 1u
          }
    }
