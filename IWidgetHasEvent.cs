namespace com.cratesmith.widgets
{
	public interface IWidgetHasEvent<T> where T:struct,IWidgetEvent
	{
		ref WidgetEventStorage<T> EventStorage { get; }
	}
}
