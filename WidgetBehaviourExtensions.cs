namespace com.cratesmith.widgets
{
    public static class WidgetBehaviourExtensions
    {
        public static TWidget GetPrefab<TWidget>(this TWidget _widget, bool _returnSelfIfNull = true) where TWidget:WidgetBehaviour
        {
            if(_widget.Prefab is TWidget result) 
                return result;
            return _returnSelfIfNull ? _widget:null;
        }
    }
}