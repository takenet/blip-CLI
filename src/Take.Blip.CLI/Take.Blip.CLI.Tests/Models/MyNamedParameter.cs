using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Take.Blip.CLI.Tests.Models
{
    public class MyNamedParameter<T> : INamedParameter<T>
    {
        public bool IsSet => Value != null;

        public T Value { set; get; }

        public event Action<INamedParameter<T>> ValueParsed;

        public INamedParameter<T> Alias(string name)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> DefaultValue(T value)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> HelpText(string text)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> Hidden(bool hidden = true)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> ParseUsing(Func<string, T> parser)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> Required(bool isRequired = true)
        {
            throw new NotImplementedException();
        }
    }

    public class MySwitch : ISwitch
    {
        public bool IsSet { get; set; }

        public event Action<ISwitch> ValueParsed;

        public ISwitch Alias(string name)
        {
            throw new NotImplementedException();
        }

        public ISwitch HelpText(string text)
        {
            throw new NotImplementedException();
        }

        public ISwitch Hidden(bool hidden = true)
        {
            throw new NotImplementedException();
        }
    }
}
