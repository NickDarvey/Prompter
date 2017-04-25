# Prompter
A lil' helper library for Service Fabric actor's reminders.

It adds two things we found ourselves implementing over and over again:
* Safe unregistering of reminders
  
  No more try/catch blocks! Huzzah!

* Single-use reminders
  
  You're meant to unregister reminders, even if you're using the fire-once approach (`dueTime: TimeSpan.FromMilliseconds(-1)`).
  This makes that process automatic with `PromptOnce`.
  This is currently [listed as a functional bug](https://github.com/Azure/service-fabric-issues/issues/178), but this will get you around it.

* Correlation IDs

  We use found it super useful to have an ID we could track across all of our services. This let's you track a GUID across reminders.

## Usage
```c#

internal sealed class OrderActor : PromptableActor, IOrderActor
{
    public async Task PlaceOrder(OrderRequest req)
    {
        // Some internal logic, whatever
        var token = await SetOrderState(req);

        // Register a prompt to come back in seven days
        await PromptOnce(token, TimeSpan.FromDays(7));
    }

    protected override async Task OnPrompt(string name, byte[] context, TimeSpan due, TimeSpan period, Guid cid)
    {
        var token = name;
        var order = await GetOrderState(name);
        await SendEmailReminder(order);
    }
}
```

## Setup
Install it from [NuGet](https://www.nuget.org/packages/Prompter/) (`Install-Package Prompter`) and use one of the two options for integration.

### A. Inheritance
```c#
internal sealed class MyActor : PromptableActor, IMyActor { }
```

### B. Pretending you have traits

Setup a callback:
```c#
private Task OnPrompt(string name, byte[] context, TimeSpan due, TimeSpan period, Guid cid)
{
    // Do stuff
}
```

then add it to your actor's constructor:
```c#
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

and link the reminder callback to Prompter:
```c#
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