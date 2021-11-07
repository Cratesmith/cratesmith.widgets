using System;
using UnityEngine;

namespace com.cratesmith.widgets
{
	public struct WidgetEventStorage<T> where T:struct,IWidgetEvent
	{
		bool hasEvent;
		T    eventData;

		public void Set(in T _event)
		{
			hasEvent = true;
			eventData = _event;
		}
		public void Clear()
		{
			hasEvent = false;
			eventData = default;
		}
		public bool Get(out T _event)
		{
			if (hasEvent && eventData.CheckValid())
			{
				_event = eventData;
				return true;
			}
			_event = default;
			return false;
		}
	}
}
