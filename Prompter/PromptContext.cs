using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Prompter
{
    [DataContract]
    internal sealed class PromptContext
    {
        [DataMember(Name = "CorrelationId", Order = 0)]
        public string Cid { get; private set; }

        [DataMember(Name = "Kind", Order = 1)]
        public PromptKind Kind { get; private set; }

        [DataMember(Name = "Data", Order = 2)]
        public byte[] Data { get; private set; }

        public PromptContext(string cid, PromptKind kind, byte[] data)
        {
            Cid = cid;
            Kind = kind;
            Data = data;
        }
    }

    [DataContract]
    public enum PromptKind
    {
        [EnumMember]
        Once,

        [EnumMember]
        Many
    }
}
