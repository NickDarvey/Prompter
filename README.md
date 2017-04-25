# SomethingSomethingConfig
A lil' helper library for Service Fabric actor's reminders.

It adds two things we found ourselves implementing over and over again:
* Safe unregistering of reminders
  
  No more try/catch blocks! Huzzah!

* Single-use reminders
  
  You're meant to unregister reminders, even if you're using the fire-once approach (`dueTime: TimeSpan.FromMilliseconds(-1)`).
  This makes that process automatic with `PromptOnce`.
  This is currently [listed as a functional bug](https://github.com/Azure/service-fabric-issues/issues/178), but this will get you around it.

## Usage


## Setup
Install it from [NuGet](https://www.nuget.org/packages/Prompter/) (`Install-Package Prompter`)
and setup a callback:
```
private Task OnPrompt(string name, byte[] context, TimeSpan due, TimeSpan period, Guid cid)
{
    // Do stuff
}
```

then add it to your actor's constructor:
```
private readonly Prompt _prompt;

public ActorFixture(ActorService svc, ActorId id)
: base(svc, id)
{
    _prompt = new Prompt(
        OnPrompt,
        GetReminder,
        RegisterReminderAsync,
        UnregisterReminderAsync,
        this);
}
```

link the reminder callback to Prompter:
```
public Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period) =>
    _prompt.ReceivePrompt(reminderName, state, dueTime, period);
```

It's awful, isn't it?
I couldn't figure out a nicer way layer it on but I'm open to suggestions.
(We have a bunch of these 'traits' we wanted to use and didn't like the idea of bundling them all into one base class.)



## Logging
You should be logging everything like crazy, so I chucked in logs here too.
That's why you (should) pass in the current actor when constructing `Prompt`.
(I don't do anything else with it, I swear!)

The provider is `Prompter-Prompt` and it logs registers, unregisters and receives.