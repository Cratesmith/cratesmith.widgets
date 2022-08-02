using System;
using TMPro;

namespace com.cratesmith.widgets
{
    /// <summary>
    /// A wrapper returned by WidgetBuilder instead of returning the widgets themselves
    /// to help prevent calling methods other than State() during the build
    /// </summary>
    public ref struct WidgetBeingBuilt<TWidget> 
        where TWidget:WidgetBehaviour
    {
        public readonly WidgetBehaviour prefabInstance;
        public readonly TWidget         widget;

        public WidgetBeingBuilt(TWidget _widget, WidgetBehaviour _prefabInstance)
        {
            widget = _widget;
            prefabInstance = _prefabInstance;
        }

        public static implicit operator TWidget(in WidgetBeingBuilt<TWidget> @this) => @this.widget;

        public WidgetBuilder BeginChildren()
        {
            var owner = Is.NotNull(widget) ? widget.OwnerWidget:null;
            return new WidgetBuilder(widget, owner);
        }
        
        public WidgetBeingBuilt<TWidget> Sorting(int group)
        {
            ((WidgetBuilder.ISecret)widget).SetSorting(new WidgetSorting(group, widget.Sorting.index));
            return this;
        }
        
        public WidgetBeingBuilt<TWidget> Sorting(int group, int index)
        {
            ((WidgetBuilder.ISecret)widget).SetSorting(new WidgetSorting(group, index));
            return this;
        }
        
        public WidgetBeingBuilt<TWidget> Event<TEvent>(out bool hasEvent)
            where TEvent : struct, IWidgetEvent
        {
            if (widget is IWidgetHasEvent<TEvent>)
            {
                var evt = ((WidgetBuilder.ISecret)widget).SubscribeAndGetEvent<TEvent>();
                hasEvent = evt.CheckValid();
            } else
            {
                widget.LogWarning($"{widget} doesn't support event type {typeof(TEvent).Name}");
                hasEvent = false;
            }
            return this;
        }
        
        public WidgetBeingBuilt<TWidget> Event<TEvent>()
            where TEvent : struct, IWidgetEvent
        {
            if (widget is IWidgetHasEvent<TEvent>)
            {
                 ((WidgetBuilder.ISecret)widget).SubscribeAndGetEvent<TEvent>();
            } else
            {
                widget.LogWarning($"{widget} doesn't support event type {typeof(TEvent).Name}");
            }
            return this;
        }
    }

    public static class WidgetBeingBuiltExtensions
    {
        public static WidgetBeingBuilt<TWidget> State<TWidget, TState>(in this WidgetBeingBuilt<TWidget> _widgetBuild, in TState _state, bool _forceRebuild=false)
            where TWidget:WidgetBehaviour<TState>
            where TState:struct, IWidgetState//, IEquatable<TState>
        {
            if (Is.NotNull(_widgetBuild.widget))
                _widgetBuild.widget.SetState(_state, _forceRebuild);
            
            return _widgetBuild;
        }
        
        public static WidgetBeingBuilt<TWidget> Event<TWidget,TEvent>(in this WidgetBeingBuilt<TWidget> _widgetBuild, out TEvent eventData)
            where TWidget:WidgetBehaviour, IWidgetHasEvent<TEvent>
            where TEvent : struct, IWidgetEvent
        {
            eventData = Is.Spawned(_widgetBuild.widget) 
                ? ((WidgetBuilder.ISecret)_widgetBuild.widget).SubscribeAndGetEvent<TEvent>()
                : default;
        
            return _widgetBuild;
        }

    }
}