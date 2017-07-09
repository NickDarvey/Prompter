using Microsoft.ServiceFabric.Actors.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prompter
{
    [EventSource(Name = "Prompter-Prompt")]
    internal sealed class PrompterEventSource : EventSource
    {
        public static readonly PrompterEventSource Log = new PrompterEventSource();
        private PrompterEventSource() : base() { }
        static PrompterEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        [NonEvent]
        public void PromptReceived(Actor actor, string promptName)
        {
            if (this.IsEnabled())
            {
                PromptReceived(
                    actor?.GetType().ToString(),
                    actor?.Id.ToString(),
                    actor?.ActorService?.Context?.CodePackageActivationContext?.ApplicationTypeName,
                    actor?.ActorService?.Context?.CodePackageActivationContext?.ApplicationName,
                    actor?.ActorService?.Context?.ServiceTypeName,
                    actor?.ActorService?.Context?.ServiceName?.ToString(),
                    actor?.ActorService?.Context?.PartitionId,
                    actor?.ActorService?.Context?.ReplicaId,
                    actor?.ActorService?.Context?.NodeContext?.NodeName,
                    promptName);
            }
        }


        private const int PromptReceivedEventId = 1;
        [Event(PromptReceivedEventId, Level = EventLevel.Informational, Message = "Prompt received: {9}")]
        private void PromptReceived(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            string partitionId,
            long? replicaOrInstanceId,
            string nodeName,
            string promptName)
        {
            WriteEvent(
                    PromptReceivedEventId,
                    actorType,
                    actorId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    nodeName,
                    promptName);
        }

        [NonEvent]
        public void PromptRegistered(Actor actor, string promptName, TimeSpan promptDue, TimeSpan promptPeriod)
        {
            if (this.IsEnabled())
            {
                PromptRegistered(
                    actor?.GetType().ToString(),
                    actor?.Id.ToString(),
                    actor?.ActorService?.Context?.CodePackageActivationContext?.ApplicationTypeName,
                    actor?.ActorService?.Context?.CodePackageActivationContext?.ApplicationName,
                    actor?.ActorService?.Context?.ServiceTypeName,
                    actor?.ActorService?.Context?.ServiceName.ToString(),
                    actor?.ActorService?.Context?.PartitionId,
                    actor?.ActorService?.Context?.ReplicaId,
                    actor?.ActorService?.Context?.NodeContext?.NodeName,
                    promptName,
                    promptDue.ToString(),
                    promptPeriod.ToString());
            }
        }


        private const int PromptRegisteredEventId = 2;
        [Event(PromptRegisteredEventId, Level = EventLevel.Informational, Message = "Prompt registered: {9}")]
        private void PromptRegistered(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            string partitionId,
            long? replicaOrInstanceId,
            string nodeName,
            string promptName,
            string promptDue,
            string promptPeriod)
        {
            WriteEvent(
                    PromptRegisteredEventId,
                    actorType,
                    actorId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    nodeName,
                    promptName,
                    promptDue,
                    promptPeriod);
        }

        [NonEvent]
        public void PromptUnregistered(Actor actor, string promptName, string promptUnregisteredResult)
        {
            if (this.IsEnabled())
            {
                PromptUnregistered(
                    actor?.GetType()?.ToString(),
                    actor?.Id?.ToString(),
                    actor?.ActorService?.Context?.CodePackageActivationContext?.ApplicationTypeName,
                    actor?.ActorService?.Context?.CodePackageActivationContext?.ApplicationName,
                    actor?.ActorService?.Context?.ServiceTypeName,
                    actor?.ActorService?.Context?.ServiceName?.ToString(),
                    actor?.ActorService?.Context?.PartitionId,
                    actor?.ActorService?.Context?.ReplicaId,
                    actor?.ActorService?.Context?.NodeContext?.NodeName,
                    promptName,
                    promptUnregisteredResult);
            }
        }


        private const int PromptUnregisteredEventId = 3;
        [Event(PromptUnregisteredEventId, Level = EventLevel.Informational, Message = "Prompt unregistered: {9}. Outcome: {10}.")]
        private void PromptUnregistered(
            string actorType,
            string actorId,
            string applicationTypeName,
            string applicationName,
            string serviceTypeName,
            string serviceName,
            string partitionId,
            long? replicaOrInstanceId,
            string nodeName,
            string promptName,
            string promptUnregisteredResult)
        {
            WriteEvent(
                    PromptUnregisteredEventId,
                    actorType,
                    actorId,
                    applicationTypeName,
                    applicationName,
                    serviceTypeName,
                    serviceName,
                    partitionId,
                    replicaOrInstanceId,
                    nodeName,
                    promptName,
                    promptUnregisteredResult);
        }
    }
}
