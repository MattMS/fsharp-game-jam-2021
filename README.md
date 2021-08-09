# JAM (Just Another Milking) game in F#

This was developed for the [F# Game Jam 2021](https://itch.io/jam/fsharp-jam).

While I was not successful in developing a working game,
I decided to post this work anyway as I think it shows off some of the nice F# tooling that is available.

## Inspiration

While reading a [thread on the theme](https://itch.io/jam/fsharp-jam/topic/1543840/theme-alternate-history),
I was inspired by the final [suggestion from Vorotato](https://itch.io/post/4116094):

> Western cowboys but they ranch cats for their fur.

I was reminded that removing fur is not the same as shearing, so I decided to go with milking instead.

## Features

### Mostly working

- Intriguing introduction story
- Browser code from F# using Fable and Rollup
- Unit tests with Fuchu
- Light and dark themes based on system preference.

### Slightly working

- Feed your cat
- Sleep

### Totally missing

- Any story beyond the introduction
- Milk your cat
- Buy and sell cats/milk/food
- Get more cats

## How to build

In the `MattMS.CatGame.Browser` folder, run `dotnet fable` to build the EcmaScript with Fable.

In the project root, run `npm run build` to compile the Pug and Stylus code, and bundle the EcmaScript for the browser.

## How to test

In `MattMS.CatGame.Tests`, run `dotnet run` to test the code.

There is very limited test coverage, as tests were only added when I was having trouble with the code.

## Why this tech?

I have used most of these libraries/tools before (except Rollup and Sutil), but I do not get to use them in my day job.

It is a small project, without multiple developer preferences, so I can fall back to the standard HTML page approach.

I very much like the graceful degredation approach that websites are meant to follow.
Unfortunately, I did make the choice of requiring CSS, otherwise I would have needed many more pages for each screen of the story.
I hope that wrapping each story "page" in an `article` tag can bring me some forgiveness.

## .NET library/tool choices

### Fable

### Fuchu

### Sutil

## Node tool choices

### Concurrently

Although not strictly necessary, I decided to use Concurrently to simplify my npm scripts.

- [@ npm](https://www.npmjs.com/package/concurrently)

I did have `npm-run-all`, for the sequential execution, but that does not matter now that there are no dependencies from the Pug templates.

```
"build": "run-s build-styles build-scripts build-pages",
```

- [@ GitHub](https://github.com/mysticatea/npm-run-all)
- [@ npm](https://www.npmjs.com/package/npm-run-all)

### Pug

[Pug](https://pugjs.org/) templates are used to generate the HTML.

- [Pug @ GitHub](https://github.com/pugjs/pug)
- [Pug CLI @ GitHub](https://github.com/pugjs/pug-cli)
- [Includes @ Pug docs](https://pugjs.org/language/includes.html)
- [Inheritance @ Pug docs](https://pugjs.org/language/inheritance.html)

### Rollup

### Stylus

[Stylus](https://stylus-lang.com/) is used to generate the CSS.

- [CLI @ Stylus docs](https://stylus-lang.com/docs/executable.html)

## Editor choices

This was mostly written in VS Code, using the Ionide extension.
I made heavy use of the `npm scripts` in the Explorer, and the Terminal for using Fable.

Essentially this involved 2 terminal sessions:

- `dotnet fable watch`
- `npm run watch`
