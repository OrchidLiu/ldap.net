﻿using System;
using System.Text;
using zivillian.ldap.Asn1;

namespace zivillian.ldap
{
    public class LdapSearchRequest : LdapRequestMessage
    {
        public string BaseObject { get; }

        public SearchScope Scope { get; }

        public DerefAliases DerefAliases { get; }

        public int SizeLimit { get; }

        public TimeSpan TimeLimit { get; }

        public bool TypesOnly { get; }

        public LdapFilter Filter { get; }

        public string[] Attributes { get; }

        internal LdapSearchRequest(Asn1LdapMessage message)
            : base(message)
        {
            var search = message.ProtocolOp.SearchRequest;
            BaseObject = Encoding.UTF8.GetString(search.BaseObject.Span);
            Scope = search.Scope;
            DerefAliases = search.DerefAliases;
            SizeLimit = search.SizeLimit;
            if (SizeLimit == 0)
                SizeLimit = Int32.MaxValue;
            TimeLimit = TimeSpan.FromSeconds(search.TimeLimit);
            if (TimeLimit == TimeSpan.Zero)
                TimeLimit = TimeSpan.MaxValue;
            TypesOnly = search.TypesOnly;
            Filter = LdapFilter.Create(search.Filter);
            Attributes = new string[0];
            if (search.Attributes.Length > 0)
            {
                Attributes = new string[search.Attributes.Length];
                for (int i = 0; i < search.Attributes.Length; i++)
                {
                    Attributes[i] = Encoding.UTF8.GetString(search.Attributes[i].Span);
                }
            }
        }

        internal override void SetProtocolOp(Asn1ProtocolOp op)
        {
            op.SearchRequest = new Asn1SearchRequest
            {
                BaseObject =  Encoding.UTF8.GetBytes(BaseObject),
                Scope = Scope,
                DerefAliases = DerefAliases,
                SizeLimit = SizeLimit,
                TimeLimit = (int) TimeLimit.TotalSeconds,
                TypesOnly = TypesOnly,
                Filter = Filter.GetAsn(),
                Attributes = new ReadOnlyMemory<byte>[0]
            };
            if (SizeLimit == Int32.MaxValue)
                op.SearchRequest.SizeLimit = 0;
            if (TimeLimit == TimeSpan.MaxValue)
                op.SearchRequest.TimeLimit = 0;
            if (Attributes != null && Attributes.Length > 0)
            {
                var attr = op.SearchRequest.Attributes = new ReadOnlyMemory<byte>[Attributes.Length];
                for (int i = 0; i < Attributes.Length; i++)
                {
                    attr[i] = Encoding.UTF8.GetBytes(Attributes[i]);
                }
            }
        }
    }
}