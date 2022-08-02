using System;

namespace com.cratesmith.widgets
{
    /// <summary>
    /// Used to rebuild widgets 
    /// </summary>
    public ref struct WidgetBuilder
    {
        WidgetBehaviour                 m_OwnerWidget;
        WidgetBehaviour                 m_ParentWidget;
        WidgetBehaviour.ChildCollection m_Children;
        
        public WidgetBuilder(WidgetBehaviour _parent, WidgetBehaviour _owner)
        {
            m_ParentWidget = _parent;
            m_OwnerWidget = _owner;

            if (!Is.NotNull(m_ParentWidget))
            {                
                m_Children = null;
                return;
            }
            
            m_Children = !Is.NotNull(m_OwnerWidget) || m_OwnerWidget == m_ParentWidget
                ? ((ISecret)m_ParentWidget).InternalChildren
                : ((ISecret)m_ParentWidget).OwnerChildren;

            m_Children.Begin(_parent,_owner);
        }

        public interface ISecret
        {
            WidgetBehaviour.ChildCollection OwnerChildren { get; }
            WidgetBehaviour.ChildCollection InternalChildren { get; }
            void ClearHasRefreshed();
            void ClearDirty();
            
            /// <summary>
            /// Setup method called by WidgetBuilder when widget is constructed
            /// </summary>
            void Init(WidgetBehaviour _prefab, WidgetBehaviour _parentWidget, WidgetBehaviour _ownerWidget, WidgetContext _context,  WidgetSorting _sorting);
            bool TryToDespawn();
            void OnDespawn();
            void SetContext(WidgetContext _context);
            
            void SetSorting(WidgetSorting sorting);
            void ClearEventSubscriptions();
            T SubscribeAndGetEvent<T>() where T : struct, IWidgetEvent;
            bool IsSubscribedToEvent<T>() where T : struct, IWidgetEvent;
        }
        public WidgetBehaviour ParentWidget => m_ParentWidget;
        public WidgetBehaviour OwnerWidget => m_OwnerWidget;

        public WidgetBeingBuilt<TWidget> Widget<TWidget>(in WidgetContext _context = default)
            where TWidget : WidgetBehaviour
        {
            var (prefabInstance, widget) = m_Children.Widget((TWidget)null, null, _context, m_ParentWidget, m_OwnerWidget, m_Children.DefaultSortingGroup);
            return new WidgetBeingBuilt<TWidget>(widget, prefabInstance);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public WidgetBeingBuilt<TWidget> Widget<TWidget>(TWidget _prefab, in WidgetContext _context=default) 
            where TWidget:WidgetBehaviour
        {
            var (prefabInstance, widget) = m_Children.Widget(_prefab, null, _context, m_ParentWidget, m_OwnerWidget, m_Children.DefaultSortingGroup);
            return new WidgetBeingBuilt<TWidget>(widget, prefabInstance);
        }
        
        public WidgetBeingBuilt<TWidget> Widget<TWidget>(WidgetPrefab<TWidget> _prefab, in WidgetContext _context=default)  
            where TWidget:WidgetBehaviour
        {
            var (prefabInstance, widget) = m_Children.Widget(_prefab.widget, _prefab.prefab, _context, m_ParentWidget, m_OwnerWidget, m_Children.DefaultSortingGroup);
            return new WidgetBeingBuilt<TWidget>(widget, prefabInstance);
        }

        public void EndChildren()
        {
            if (m_Children==null)
                return;
            
            m_Children.End(m_ParentWidget, m_OwnerWidget);
            if (m_Children.OrderChanged)
            {
                // if this edit is coming from the owner, we need to set our parent dirty to ensure order is preserved
                if (m_ParentWidget.ParentWidget != m_OwnerWidget 
                    && Is.Spawned(m_ParentWidget.ParentWidget) 
                    && !m_ParentWidget.ParentWidget.IsRefreshing)
                {
                    m_ParentWidget.ParentWidget.SetDirty();
                }
            }
        }

        public void Dispose()
        {
            EndChildren();
        }
    }
}
