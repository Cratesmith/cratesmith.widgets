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
        public readonly WidgetBehaviour owner;
        public readonly TWidget widget;

        public WidgetBeingBuilt(TWidget _widget, WidgetBehaviour _owner)
        {
            widget = _widget;
            owner = _owner;
        }

        public static implicit operator TWidget(in WidgetBeingBuilt<TWidget> @this) => @this.widget;

        public WidgetBuilder BeginChildren()
        {
            return new WidgetBuilder(widget, owner);
        }
    }
    
    public static class WidgetBeingBuiltExtensions
    {
        public static WidgetBeingBuilt<TWidget> State<TWidget, TState>(this WidgetBeingBuilt<TWidget> _widgetBuild, in TState _state, bool _forceRebuild=false)
            where TWidget:WidgetBehaviour<TState>
            where TState:struct, IWidgetState, IEquatable<TState>
        {
            if (Is.NotNull(_widgetBuild.widget))
                _widgetBuild.widget.SetState(_state, _forceRebuild);
            
            return _widgetBuild;
        }
    }
}