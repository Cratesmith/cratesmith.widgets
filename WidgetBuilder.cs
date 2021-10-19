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

            m_Children.Begin();
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
            void Init(WidgetBehaviour _prefab, WidgetBehaviour _parentWidget, WidgetBehaviour _ownerWidget, WidgetContext _context);
            bool TryToDespawn();
            void OnDespawn();
            void SortChildren();
            void SetContext(WidgetContext _context);
        }

        public WidgetBeingBuilt<TWidget> Widget<TWidget, TState>(TWidget _prefab, in TState _state)
            where TWidget : WidgetBehaviour<TState> 
            where TState : struct, IWidgetState//, IEquatable<TState>
        {
            var widget = m_Children?.Widget(_prefab, null, default, m_ParentWidget, m_OwnerWidget);
            return new WidgetBeingBuilt<TWidget>(widget, m_OwnerWidget).State(_state);
        }

        public WidgetBeingBuilt<TWidget> Widget<TWidget, TState>(TWidget _prefab, in WidgetContext _context, in TState _state)
            where TWidget : WidgetBehaviour<TState> 
            where TState : struct, IWidgetState//, IEquatable<TState>
        {
            var widget = m_Children?.Widget(_prefab, null, _context, m_ParentWidget, m_OwnerWidget);
            return new WidgetBeingBuilt<TWidget>(widget, m_OwnerWidget).State(_state);
        }
        
        public WidgetBeingBuilt<TWidget> Widget<TWidget>(in WidgetContext _context = default)
            where TWidget : WidgetBehaviour
        {
            var widget = m_Children?.Widget((TWidget)null, null, _context, m_ParentWidget, m_OwnerWidget);
            return new WidgetBeingBuilt<TWidget>(widget, m_OwnerWidget);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public WidgetBeingBuilt<TWidget> Widget<TWidget>(TWidget _prefab, in WidgetContext _context=default) 
            where TWidget:WidgetBehaviour
        {
            var widget = m_Children?.Widget(_prefab, null, _context, m_ParentWidget, m_OwnerWidget);
            return new WidgetBeingBuilt<TWidget>(widget, m_OwnerWidget);
        }
       
        public WidgetBeingBuilt<TWidget> Widget<TWidget>(WidgetPrefab<TWidget> _prefab, in WidgetContext _context=default)  
            where TWidget:WidgetBehaviour
        {
            var widget = m_Children?.Widget(_prefab.widget, _prefab.prefab, _context, m_ParentWidget, m_OwnerWidget);
            return new WidgetBeingBuilt<TWidget>(widget, m_OwnerWidget);
        }
        
        public WidgetBeingBuilt<TWidget> Widget<TWidget, TState>(WidgetPrefab<TWidget> _prefab, in TState _state)
            where TWidget : WidgetBehaviour<TState> 
            where TState : struct, IWidgetState//, IEquatable<TState>
        {
            var widget = m_Children?.Widget(_prefab.widget, _prefab.prefab, default, m_ParentWidget, m_OwnerWidget);
            return new WidgetBeingBuilt<TWidget>(widget, m_OwnerWidget).State(_state);
        }

        public WidgetBeingBuilt<TWidget> Widget<TWidget, TState>(WidgetPrefab<TWidget> _prefab, in WidgetContext _context, in TState _state)
            where TWidget : WidgetBehaviour<TState> 
            where TState : struct, IWidgetState//, IEquatable<TState>
        {
            var widget = m_Children?.Widget(_prefab.widget, _prefab.prefab, _context, m_ParentWidget, m_OwnerWidget);
            return new WidgetBeingBuilt<TWidget>(widget, m_OwnerWidget).State(_state);
        }


        public void EndChildren()
        {
            if (m_Children==null)
                return;
            
            m_Children.End();
            if (m_Children.OrderChanged)
            {
                ((ISecret)m_ParentWidget).SortChildren();
                
                // if this edit is coming as an external 
                if (m_ParentWidget != m_OwnerWidget && Is.Spawned(m_ParentWidget.ParentWidget))
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
