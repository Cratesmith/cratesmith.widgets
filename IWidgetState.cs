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
        Events    =1<<7,

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

    public struct SWidget : IWidgetState, IEquatable<SWidget>
    {
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = rectTransform.GetHashCode();
                hashCode = (hashCode * 397) ^ layoutElement.GetHashCode();
                hashCode = (hashCode * 397) ^ contentSizeFitter.GetHashCode();
                hashCode = (hashCode * 397) ^ debugLogging.GetHashCode();
                return hashCode;
            }
        }
        public WidgetRectTransform rectTransform { get; set; }
        public WidgetLayoutElement layoutElement { get; set; }
        public WidgetContentSizeFitter contentSizeFitter { get; set; }
        public Optional<LogLevel> debugLogging { get; set; }
        public bool Equals(SWidget other)
        {
            return rectTransform.Equals(other.rectTransform) && layoutElement.Equals(other.layoutElement) && contentSizeFitter.Equals(other.contentSizeFitter) && debugLogging.Equals(other.debugLogging);
        }
        public override bool Equals(object obj)
        {
            return obj is SWidget other && Equals(other);
        }
       
        public static bool operator ==(SWidget left, SWidget right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(SWidget left, SWidget right)
        {
            return !left.Equals(right);
        }
    }
}