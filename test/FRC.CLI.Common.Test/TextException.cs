using System;

namespace FRC.CLI.Common.Test
{
    public class TestException : Exception
    {
        public string Msg { get; }
        public TestException(string msg) : base(msg)
        {
            Msg = msg;
        }
    }
}