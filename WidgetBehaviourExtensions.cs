namespace com.cratesmith.widgets
{
    public static class WidgetBehaviourExtensions
    {
        // TODO: Hide _widget.Prefab from regular access in OnRefresh
        public static TWidget GetPrefab<TWidget>(this TWidget _widget, bool _returnSelfIfNull = true) where TWidget:WidgetBehaviour
        {
            if(_widget.Prefab is TWidget result) 
                return result;
            return _returnSelfIfNull ? _widget:null;
        }

        public static void SetEvent<TWidget, TEvent>(this TWidget _widget, TEvent _event)
            where TWidget : WidgetBehaviour, IWidgetHasEvent<TEvent>
            where TEvent : struct, IWidgetEvent
        {
            _widget.EventStorage.Set(_event);
            
            if (Is.NotNull(_widget.OwnerWidget) && ((WidgetBuilder.ISecret)_widget).IsSubscribedToEvent<TEvent>())
            {
                _widget.OwnerWidget.SetDirty(true);
            }
        }
    }
}