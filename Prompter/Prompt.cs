using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static Prompter.PrompterEventSource;
using static Prompter.Serialization;

namespace Prompter
{
    public delegate IActorReminder GetReminder(string reminderName);
    public delegate Task<IActorReminder> RegisterReminder(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period);
    public delegate Task UnregisterReminder(IActorReminder reminder);
    public delegate Task OnPrompt(string name, byte[] context, TimeSpan due, TimeSpan period, Guid? cid);

    public sealed class Prompt
    {
        private const char PROMPT_DELIMITER = '|';
        private const string PROMPT_ONCE_PREFIX = "prompt:once";
        private const string PROMPT_MANY_PREFIX = "prompt:many";

        private readonly DataContractSerializer _serializer = new DataContractSerializer(typeof(PromptContext));
        private readonly OnPrompt _prompt;
        private readonly GetReminder _get;
        private readonly RegisterReminder _register;
        private readonly UnregisterReminder _unregister;
        private readonly Actor _actor;

        /// <summary>
        /// Create a prompt instance for a specific actor
        /// </summary>
        /// <param name="prompt">A callback for when the actor is being prompted.</param>
        /// <param name="get">The actor's GetReminderAsync method</param>
        /// <param name="register">The actor's RegisterReminderAsync</param>
        /// <param name="unregister">The actor's UnregisterReminderAsync</param>
        /// <param name="actor">Optionally (recommended), the actor instance for detailed logging.</param>
        public Prompt(
            OnPrompt prompt,
            GetReminder get,
            RegisterReminder register,
            UnregisterReminder unregister,
            Actor actor = null)
        {
            _prompt = prompt ?? throw new ArgumentNullException("An OnPrompt callback must be supplied"); ;
            _get = get ?? throw new ArgumentNullException("The actor's GetReminderAsync method must be supplied");
            _register = register ?? throw new ArgumentNullException("The actor's RegisterReminderAsync method must be supplied");
            _unregister = unregister ?? throw new ArgumentNullException("The actor's UnregisterReminderAsync method must be supplied");
            _actor = actor;
        }

        /// <summary>
        /// Registers a prompt with the actor.
        /// This prompt will fire once and automatically clean up it's state after firing.
        /// </summary>
        /// <param name="due">
        /// A System.TimeSpan representing the amount of time to delay before firing the
        /// reminder. Specify negative one (-1) milliseconds to prevent reminder from firing.
        /// Specify zero (0) to fire the reminder immediately.
        /// </param>
        /// <param name="cid">
        /// Optionally, a correlation ID.
        /// </param>
        /// <returns>
        /// A reference to the reminder.
        /// Store this if you want to manually unregister a prompt.
        /// </returns>
        public Task<IActorReminder> PromptOnce(string name, TimeSpan due, Guid? cid = null, byte[] data = null)
        {
            var context = _serializer.Serialize(new PromptContext(cid, PromptKind.Once, data));
            return _register(
                reminderName: name,
                state: context,
                dueTime: due,
                period: TimeSpan.FromMilliseconds(-1))
            .ContinueWith(t => { Log.PromptRegistered(_actor, t.Result.Name, t.Result.DueTime, t.Result.Period); return t.Result; });
        }

        /// <summary>
        /// Registers a prompt with the actor.
        /// </summary>
        /// <param name="due">
        /// A System.TimeSpan representing the amount of time to delay before firing the
        /// reminder. Specify negative one (-1) milliseconds to prevent reminder from firing.
        /// Specify zero (0) to fire the reminder immediately.
        /// </param>
        /// <param name="period">
        /// The time interval between firing of reminders. Specify negative one (-1) milliseconds
        /// to disable periodic firing.
        /// </param>
        /// <param name="cid">
        /// Optionally, a correlation ID.
        /// </param>
        /// <returns>
        /// A reference to the reminder.
        /// Store this if you want to manually unregister a prompt.
        /// </returns>
        public Task<IActorReminder> PromptMany(string name, TimeSpan due, TimeSpan period, Guid? cid = null, byte[] data = null)
        {
            var context = _serializer.Serialize(new PromptContext(cid, PromptKind.Many, data));
            return _register(
                reminderName: name,
                state: context,
                dueTime: due,
                period: period)
            .ContinueWith(t => { Log.PromptRegistered(_actor, t.Result.Name, t.Result.DueTime, t.Result.Period); return t.Result; });
        }

        public async Task<bool> ForgetPrompt(string name)
        {
            try
            {
                var r = _get(name);
                await _unregister(r);
                Log.PromptUnregistered(_actor, name, "found and unregistered");
                return true;
            }
            catch (ReminderNotFoundException)
            {
                Log.PromptUnregistered(_actor, name, "not found");
                return false;
            }
        }

        public async Task ReceivePrompt(string name, byte[] data, TimeSpan dueTime, TimeSpan period)
        {
            Log.PromptReceived(_actor, name);

            try
            {
                var context = _serializer.Deserialize<PromptContext>(data);
                if (context.Kind == PromptKind.Once) await ForgetPrompt(name);
                await _prompt(name, context.Data, dueTime, period, context.Cid);
            }
            catch(SerializationException)
            {
                await _prompt(name, data, dueTime, period, null);
            }
        }
    }
}
