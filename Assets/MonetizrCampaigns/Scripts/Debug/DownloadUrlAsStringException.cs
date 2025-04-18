﻿using System;

namespace Monetizr.SDK.Debug
{
    public class DownloadUrlAsStringException : Exception
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