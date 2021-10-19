using System;

namespace com.cratesmith.widgets
{
    [Flags]
    public enum LogLevel
    {
        Error     =1<<1,
        Warning   =1<<2,
        Info      =1<<3,
        Details   =1<<4,
        Debugging =1<<5,
        Internal  =1<<6,

        DefaultPreset   =Error|Warning|Info,
    }

    public interface IWidgetState
    {
        WidgetRectTransform rectTransform { get; }
        WidgetLayoutElement layoutElement { get; }
        WidgetContentSizeFitter contentSizeFitter { get; }
        public Optional<LogLevel> debugLogging { get; }
    }

    public interface IWidgetStateLayoutGroup<TLayoutGroup> : IWidgetState where TLayoutGroup:struct
    {
        TLayoutGroup layoutGroup { get; }
        public Optional<bool> autoDisableLayoutGroup { get; }
    }
}