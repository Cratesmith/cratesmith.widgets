using System.Collections;
using System.Collections.Generic;

namespace com.cratesmith.widgets
{
	static class CollectionPool<TCollection,T> where TCollection : ICollection<T>, new()
	{
		static Queue<TCollection> s_Queue = new Queue<TCollection>();
        
		public static TCollection Get()
		{
			return s_Queue.Count != 0 ? s_Queue.Dequeue()
				: new TCollection();
		}
        
		public static void Release(TCollection _collection)
		{
			_collection.Clear();
			s_Queue.Enqueue(_collection);    
		}
	}
}
