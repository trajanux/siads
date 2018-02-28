using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Security.Cryptography;

namespace CheckTag
{
    class Tag
    {
        public enum TagState { Valid, Invalid, Error, Message }

        public string Epc { get; set; }
        public string Id { get; set; }
        public UInt32 Check { get; set; }       // Valor esperado
        public UInt32 Memory { get; set; }      // Valor lido
        public bool RdErr { get; set; }
        public TagState Status { get; set; }

        public Tag()
        {
            Status = TagState.Message;
        }

        public Tag(string s)
        {
            ParseString(s);
        }

        public TagState State()
        {
            if (RdErr) return TagState.Error;
            if (Check == Memory) return TagState.Valid;
            return TagState.Invalid;
        }

        void ParseString(string s)
        {
            if ( s.Contains("RDERR") )
            {
                Epc = "Read Error";
                Status = TagState.Error;
                RdErr = true;
                return;
            }
            if ( s.Contains("NOTAG") )
            {
                Epc = "No tag";
                Status = TagState.Error;
                RdErr = true;
                return;
            }
            RdErr = false;

            String[] fields = s.ToUpper().Split(' ');
            Epc = fields[0].Substring(1);
            Id = fields[1].Substring(1);
            Memory = Convert.ToUInt32(fields[2].Substring(1).Trim(),16);

            string aux = string.Concat(Epc, Id);

            HashAlgorithm sha = new SHA1CryptoServiceProvider();
            byte[] result = sha.ComputeHash(Encoding.UTF8.GetBytes(aux));
            Check = ((uint)result[0]<<24) | ((uint)result[1]<<16) | ((uint)result[2]<<8) | ((uint)result[3]);

            if (Check == Memory)
            {
                Status = TagState.Valid;
            } else
            {
                Status = TagState.Invalid;
            }
        }
    }
}