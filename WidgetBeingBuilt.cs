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
        
        public bool HasEvent<T>() where T: struct, IWidgetEvent => widget.HasEvent<T>();
        public bool HasStatus<T>(out T status) where T: struct, IWidgetEvent => widget.HasEvent(out status);
        
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
    }
}