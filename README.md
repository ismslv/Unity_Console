# Unity_Console

Console system for Unity projects, v0.24

Installation:

1. Drop the Console prefab on the scene.
2. Add canvas, if not already, optionally connect it to prefab in inspector
3. That's it! Basic commands, such as *quit*, *reload_scene*, *screenshot* are already available.
4. By default, **tab to switch**

Console saves the state of cursor (hidden, locked) and returns it back when you hide it.

To add a new command:

```csharp
Console.Register(new Command() {
    name = "set_season",
    argumentsText = "season name",
    arguments = 1,
    toHideAfter = true,
    action = (s) => {
        switch (s[1]) {
          case "winter":
            //do winter
            break;
          case "summer":
            //do summer
            break;
          default:
            //do rain and wind
        }
      }
  });
```

Besides Commands, there are also Actions. For example, to block in-game input when console is shown, use the following, it will run when console is shown and hidden, respectively.

```csharp
Console.RegisterAction(Console.ActionType.OnShow, ()=> {
    //block input
});
Console.RegisterAction(Console.ActionType.OnHide, ()=> {
    //unblock input
});
```

Additional beta feature is Watchers. It can be used to keep atttention for some useful data, for example, to count something during the testing. Default watchers are Time and Screen size. There is the default command *save_watchers*, which will save all watchers to the .txt file. The usage is like the following:

```csharp
RegisterWatcher(new Watcher() {
    pattern = "Screen size: {0}",
    isHidden = true,
    updater = () => {
        return Screen.width + "x" + Screen.height;
    }
});
```
