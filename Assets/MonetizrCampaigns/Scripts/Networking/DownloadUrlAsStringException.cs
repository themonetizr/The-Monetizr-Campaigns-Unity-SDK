﻿using System;

namespace Monetizr.SDK.Networking
{
    internal partial class MonetizrHttpClient
    {
        internal class DownloadUrlAsStringException : Exception
        {
            public DownloadUrlAsStringException()
            {
            }

            public DownloadUrlAsStringException(string message)
                : base(message)
            {
            }

            public DownloadUrlAsStringException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }
    }
}