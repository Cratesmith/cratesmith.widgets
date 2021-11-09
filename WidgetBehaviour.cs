//#define WIDGETS_GC_TEST

#if !WIDGETS_GC_TEST
#define WIDGETS_LOG_ERROR
#define WIDGETS_LOG_WARNING
#define WIDGETS_LOG_INFO


#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define WIDGETS_LOG_DEBUGGING
#define WIDGETS_LOG_INTERNAL
#endif
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace com.cratesmith.widgets
{
    /// <summary>
    /// Base class for widget behaviours
    /// 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public abstract class WidgetBehaviour<TState> 
        : WidgetBehaviour
        where TState:struct, IWidgetState//, IEquatable<TState>
    {
        /// <summary>
        /// Current state of the widget.
        /// This contains all the data to build/update the widget in OnRefresh()  
        /// </summary>
        public TState State { get; private set; }
        
        /// <summary>
        /// Modify the state of this widget
        /// If the state changes the widget will call SetDirty() 
        /// </summary>
        public void SetState(in TState _state, bool _forceDirty=false)
        {
            if (!_forceDirty 
                && typeof(IEquatable<TState>).IsAssignableFrom(typeof(TState)) 
                && EqualityComparer<TState>.Default.Equals(_state,State) && !_forceDirty) return;
            
            State = _state;
            LogInternal($"Widget {name} state changed to [{State}]");
            SetDirty();
        }

        protected override void OnRefresh(ref WidgetBuilder builder)
        { 
            var prefab = this.GetPrefab();
            State.layoutElement.Apply(LayoutElement, prefab.LayoutElement, UsesTypePrefab);
            State.rectTransform.Apply(RectTransform, Application.isPlaying && HasRefreshed ? RectTransform:prefab.RectTransform, UsesTypePrefab);
            State.contentSizeFitter.Apply(ContentSizeFitter, prefab.ContentSizeFitter, UsesTypePrefab);
            DebugLogging = GetValue(State.debugLogging, prefab.DebugLogging);
            var _contentSizeFitterEnabled = ContentSizeFitter.horizontalFit != ContentSizeFitter.FitMode.Unconstrained
                                        || ContentSizeFitter.verticalFit != ContentSizeFitter.FitMode.Unconstrained;
            if (_contentSizeFitterEnabled != ContentSizeFitter.enabled)
                ContentSizeFitter.enabled=_contentSizeFitterEnabled;
        }

        public override void ResetState()
        {
            SetState(default);
        }
    }

    /// <summary>
    /// Base class for all widgets
    /// Should not inherited from directly, use WidgetBehaviour<T> instead
    /// </summary>
    [ExecuteAlways]
    public abstract class WidgetBehaviour
        : MonoBehaviour
        , WidgetBuilder.ISecret
        , IWidgetHasEvent<EHovered>, IPointerEnterHandler, IPointerExitHandler   
    {
        [SerializeField, ReadOnlyField] protected RectTransform m_RectTransform;
        public RectTransform RectTransform => m_RectTransform;
        
        [SerializeField, ReadOnlyField] private List<WidgetBehaviour> m_StaticChildren = new List<WidgetBehaviour>();

        [SerializeField, ReadOnlyField] protected LayoutElement m_LayoutElement;
        public LayoutElement LayoutElement => m_LayoutElement;

        [SerializeField, ReadOnlyField] protected ContentSizeFitter m_ContentSizeFitter;
        public ContentSizeFitter ContentSizeFitter => m_ContentSizeFitter;

        [SerializeField] LogLevel m_DebugLogging = LogLevel.DefaultPreset;
        public LogLevel DebugLogging {
            get => m_DebugLogging;
            protected set => m_DebugLogging = value;
        }

        struct WidgetSiblingIndexComparision : IComparer<WidgetBehaviour>
        {
            public int Compare(WidgetBehaviour x, WidgetBehaviour y)
            {
                if (ReferenceEquals(x, y))
                    return 0;
                if (ReferenceEquals(null, y))
                    return 1;
                if (ReferenceEquals(null, x))
                    return -1;
                return x.m_RectTransform.GetSiblingIndex().CompareTo(y.m_RectTransform.GetSiblingIndex());
            }
        }
        
        struct WidgetIndexComparision : IComparer<WidgetChild>
        {
            public int Compare(WidgetChild x, WidgetChild y)
            {
                if (ReferenceEquals(x.prefabInstance, y.prefabInstance))
                    return 0;
                if (ReferenceEquals(null, y.prefabInstance))
                    return 1;
                if (ReferenceEquals(null, x.prefabInstance))
                    return -1;
                return x.prefabInstance.m_WidgetIndex.CompareTo(y.prefabInstance.m_WidgetIndex);
            }
        }

        public struct WidgetChild<TWidget> 
            where TWidget:WidgetBehaviour
        {
            public TWidget widget;
            public WidgetBehaviour prefabInstance;
            
            public static implicit operator WidgetChild(WidgetChild<TWidget> @this)
            {
                return new WidgetChild()
                {
                    widget = @this.widget,
                    prefabInstance = @this.prefabInstance
                };
            }
            
            public static implicit operator WidgetChild<TWidget>(WidgetChild @this)
            {
                return new WidgetChild<TWidget>()
                {
                    widget = @this.widget as TWidget,
                    prefabInstance = @this.prefabInstance
                };
            }
        }
        
        public struct WidgetChild
        {
            public WidgetBehaviour widget;
            public WidgetBehaviour prefabInstance;
        }
        
        public class ChildCollection : IEnumerable<WidgetChild>
        {
            public class ContextGroup : List<WidgetChild>, IDisposable
            {
                public int                   numTouched;

                public static ContextGroup Create()
                {
                    var newInstance =CollectionPool<ContextGroup, WidgetChild>.Get();
                    newInstance.numTouched = 0;
                    return newInstance;
                }
                
                public void Dispose()
                {
                    numTouched = 0;
                    CollectionPool<ContextGroup, WidgetChild>.Release(this);
                }
            }

            public struct Group
            {
                public int                touched;
                public List<ContextGroup> widgets;
            }

            public Dictionary<WidgetContext, ContextGroup>  ContextGroups {get; private set;} = new Dictionary<WidgetContext, ContextGroup>();
            public List<WidgetChild> WidgetsInOrder = new List<WidgetChild>();
            public int EditingIndex { get; private set; } = -1;
            public bool OrderChanged { get; private set; } = false;
            public int Count => WidgetsInOrder.Count;

            static HashSet<WidgetContext> s_RemoveContexts = new HashSet<WidgetContext>();

            public void Begin(WidgetBehaviour _parent, WidgetBehaviour _owner)
             {
                _parent.LogInternal($"Begin children widget:{_parent.name} owner:{_owner.name}");
                if (EditingIndex >= 0)
                {
                    _parent.LogWarning($"BeginChildren called twice for widget:{_parent.name}. You probably forgot to call EndChildren()/dispose its WidgetBuilder (which will be in this callstack)");
                    End(_parent, _owner);
                }

                foreach (var group in ContextGroups.Values)
                {
                    group.numTouched = 0;
                }
                
                StepEditingIndex(); // we start at -1 so this will set it to 0
                OrderChanged = false;
            }
            
            public TWidget Widget<TWidget>(TWidget _prefab, WidgetBehaviour _rootPrefab, in WidgetContext _context, WidgetBehaviour m_ParentWidget, WidgetBehaviour m_OwnerWidget)
                where TWidget : WidgetBehaviour
            {
                if (!ContextGroups.TryGetValue(_context, out var group))
                {
                    ContextGroups[_context] = group = ContextGroup.Create();
                }

                var existingChild = group.Count > group.numTouched 
                    ? group[group.numTouched]
                    : default;
                
                //bool isNew = false;
                WidgetChild<TWidget> result = default;
                if (WidgetManager.IsWidget(existingChild.widget, out TWidget widget) 
                    && !existingChild.prefabInstance.IsDespawning
                    && existingChild.prefabInstance.Prefab==WidgetManager.LookupPrefab(existingChild.prefabInstance.GetType(), existingChild.prefabInstance.Prefab))
                {
                    result = existingChild;
                }
                else if (_context.isUnique && group.Count > 0)
                {
                    return null;
                }
                else
                {
                    //isNew = true;
                    var spawn = WidgetManager.SpawnChild(_prefab,
                                                      _rootPrefab,
                                                      m_ParentWidget,
                                                      m_OwnerWidget, _context);
                    result = spawn;

                    if (group.numTouched == group.Count)
                        group.Add(spawn);
                    else
                        group.Insert(group.numTouched, spawn);
                    
                    OrderChanged = true;
                }

                var prevIndex = result.prefabInstance.m_WidgetIndex;
                result.prefabInstance.m_WidgetIndex = EditingIndex;
                ++group.numTouched;
                StepEditingIndex();
                /*
                if (m_ParentWidget.DebugLogging.HasFlag(LogLevel.Internal))
                {
                    var sb = new StringBuilder();
                    sb.Append($"{m_ParentWidget.name}");
                    sb.Append($":{(m_OwnerWidget == m_ParentWidget ? "Int" : "Own")}");
                    sb.Append(":Widget");
                    if (isNew)
                    {
                        sb.Append($":Added[{result.prefabInstance.m_WidgetIndex}]");
                    }
                    else if (prevIndex != result.prefabInstance.m_WidgetIndex)
                    {
                        sb.Append($":Reordered[{prevIndex}->{result.prefabInstance.m_WidgetIndex}]");
                    } 
                    else
                    {
                        sb.Append($":Kept[{result.prefabInstance.m_WidgetIndex}]");
                    }
                    sb.Append($"\n{result}");
                    m_ParentWidget.LogInternal( sb.ToString());
                    Debug.Log(sb);
                }*/

                return result.widget;
            }

            public void End(WidgetBehaviour _parent, WidgetBehaviour _owner)
            {
                if (EditingIndex == -1)
                {
                    Debug.LogWarning("End called without Begin!");
                    return;
                }
                
                lock (s_RemoveContexts)
                {
                    foreach (var pair in ContextGroups)
                    {
                        var context = pair.Key;  
                        var group = pair.Value; 
                        for (int i = group.numTouched; i < group.Count; i++)
                        {
                            var widget = group[i].prefabInstance;
                            if (Is.Spawned(widget))
                            {
                                WidgetManager.Despawn(widget);
                                if (widget.IsDespawning)
                                {
                                    // keep this widget by incrementing numTouched
                                    // and placing it at numTouched-1
                                    var swap = group[i];
                                    group[i] = group[group.numTouched];
                                    group[group.numTouched] = swap;
                                    ++group.numTouched;
                                    continue;
                                }
                            }
                            // this widget got despawned, or was externally deleted. 
                            OrderChanged = true;
                        }
                        
                        // clear all removed items from the group (all widgets after numTouched)
                        if(group.numTouched<group.Count)
                            group.RemoveRange(group.numTouched, group.Count-group.numTouched);
                        
                        // destroy the group if it's empty (done below)
                        if (group.Count == 0)
                        {
                            group.Dispose();
                            s_RemoveContexts.Add(context);
                        }
                    }
                    
                    // destroy empty groups contd.
                    foreach (var context in s_RemoveContexts)
                    {
                        ContextGroups.Remove(context);
                    }
                    s_RemoveContexts.Clear();
                }

                // write the widgetsInOrder list
                // At this point we are garunteed to have unique widgetIndex values
                // ...but there may be gaps. (Fixed below)
                WidgetsInOrder.Clear();
                foreach (var group in ContextGroups.Values)
                {
                    foreach (var widget in group)
                    {
                        if(WidgetsInOrder.Count==0 
                           || WidgetsInOrder[WidgetsInOrder.Count-1].prefabInstance.WidgetIndex <= widget.prefabInstance.WidgetIndex)
                        {
                            WidgetsInOrder.Add(widget);
                        } else
                        {
                            var index = WidgetsInOrder.BinarySearch(widget, default(WidgetIndexComparision));
                            if (index < 0) index = ~index;
                            WidgetsInOrder.Insert(index, widget);
                        }
                    }
                }
                
                // Fix any gaps in widgetIndex values
                // These can occur if a despawning widget gets despwawned by this method.
                for (int i = 0; i < WidgetsInOrder.Count; i++)
                {
                    WidgetsInOrder[i].prefabInstance.m_WidgetIndex = i;
                }

                // mark us as done
                EditingIndex = -1;
                
                _parent.LogInternal($"End children widget:{_parent.name} owner:{_owner.name}");
            }
            
            void StepEditingIndex()
            {
                while (true)
                {
                    ++EditingIndex;
                    if (EditingIndex >= WidgetsInOrder.Count)
                        break;
                    
                    var current = WidgetsInOrder[EditingIndex].prefabInstance;
                    if (Is.Spawned(current) && !current.IsDespawning)
                        break;
                }
            }

            public void Clear()
            {
                foreach (var group in ContextGroups.Values)
                {
                    group.Dispose();
                }
                ContextGroups.Clear();
                EditingIndex = -1;
            }

            public List<WidgetChild>.Enumerator GetEnumerator() => WidgetsInOrder.GetEnumerator();
            IEnumerator<WidgetChild> IEnumerable<WidgetChild>.GetEnumerator() => WidgetsInOrder.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this).GetEnumerator();
        }
        
        ChildCollection m_InternalChildren = new ChildCollection();

        ChildCollection m_OwnerChildren = new ChildCollection();
        int             m_WidgetIndex;
        public int WidgetIndex => m_WidgetIndex; 
        
        List<WidgetBehaviour> m_Children = new List<WidgetBehaviour>();
        public List<WidgetBehaviour> Children => m_Children;

        ref WidgetEventStorage<EHovered> IWidgetHasEvent<EHovered>.EventStorage => ref m_Hovered;
        WidgetEventStorage<EHovered> m_Hovered;
        
        /// <summary>
        /// The parent of this Widget.
        /// If this is null the widget is most likely a child of a WidgetRoot  
        /// </summary>
        public WidgetBehaviour ParentWidget { get; private set; }
        
        /// <summary>
        /// The owner of this Widget, (which must be one of it's parents).
        /// If owned by another widget, that widget can manage children of this widget and  will be set dirty when this one is. 
        /// </summary>
        public WidgetBehaviour OwnerWidget { get; private set; }

        /// <summary>
        /// The depth of this widget in the widget hierarchy.
        /// At runtime dirty widgets get refreshed in LateUpdate in order of depth  
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// Is this widget awaiting a refresh?
        /// </summary>
        public bool IsDirty { get; private set; }

        
        /// <summary>
        /// Has this widget been refreshed since it was created?
        /// </summary>
        public bool HasRefreshed { get; private set; }

        /// <summary>
        /// Is this widget a type-only prefab, or a widget created from one?
        /// </summary>
        public bool UsesTypePrefab { get; private set; }
        
        /// <summary>
        /// The canvas this widget is parented to
        /// </summary>
        public Canvas Canvas { get; private set; }
        
        /// <summary>
        /// Refresh this widget based on it's new state
        /// As widgets are usually pooled this function should ensure that
        /// no previous state remains after this method is called.
        /// </summary>
        public void Refresh()
        {
            var builder = new WidgetBuilder(this, this);
            IsDirty = false;
            IsRefreshing = true;
            OnRefresh(ref builder);
            builder.EndChildren();
            IsRefreshing = false;
            HasRefreshed = true;
            LogInternal($"Widget {name} refreshed");
        }

        /// <summary>
        /// Refresh this widget based on it's new state
        /// As widgets are usually pooled this function should ensure that
        /// no previous state remains after this method is called. 
        /// </summary>
        /// <param name="widgetBuilder"></param>
        protected abstract void OnRefresh(ref WidgetBuilder builder);

        /// <summary>
        /// Mark this widget as dirty.
        /// This will schedule a Refresh of the Widget if not already dirty.
        /// </summary>
        public bool SetDirty(bool forceImmediate=false)
        {
            if (IsDirty) 
                return false;
            
            lock (s_SetDirtyStack)
            {
                s_SetDirtyStack.Clear();
                s_SetDirtyStack.Push(this);
                while (s_SetDirtyStack.Count > 0)
                {
                    var current = s_SetDirtyStack.Pop();
                    if (!Is.Spawned(current) || current.IsDirty) continue;
                    current.IsDirty = true;
                    WidgetManager.MarkForRebuild(current, forceImmediate);
                } 
            }
            LogInternal($"Widget {name} set dirty");
            return true;
        }
        
        public static Stack<WidgetBehaviour> s_SetDirtyStack = new Stack<WidgetBehaviour>();

        [Obsolete("Casting WidgetBehaviour to bool is not advised: Use Is.Spawned() (active widgets only)  or Is.Null()/Is.NotNull()")]
        public static implicit operator bool(WidgetBehaviour self)
        {
            return (bool)((MonoBehaviour)self); 
        } 
        
        /// <summary>
        /// Children list access for WidgetBuilder
        /// </summary>
        ChildCollection WidgetBuilder.ISecret.OwnerChildren => m_OwnerChildren;
        ChildCollection WidgetBuilder.ISecret.InternalChildren => m_InternalChildren;
        
        public WidgetBehaviour Prefab { get; private set; }

        /// <summary>
        /// Does this widget exist? Or is it a preab/pooled instance?
        /// </summary>
        public bool Despawned { get; private set; }

        /// <summary>
        /// Setup method called by WidgetBuilder when widget is constructed
        /// </summary>
        void WidgetBuilder.ISecret.Init(WidgetBehaviour _prefab,
                                        WidgetBehaviour _parentWidget,
                                        WidgetBehaviour _ownerWidget,
                                        WidgetContext _context)
        {
            if (!Is.NotNull(_prefab))
            {
                _prefab = this;
                UsesTypePrefab = true;
            } else
            {
                UsesTypePrefab = _prefab.UsesTypePrefab;
            }
            Prefab = _prefab;   
            ParentWidget = _parentWidget;
            Depth = Is.Spawned(_parentWidget) ? _parentWidget.Depth+1:0;
            m_WidgetIndex = 0;
            OwnerWidget = _ownerWidget;
            Context = _context;
            HasRefreshed = false;
            IsDespawning = false;
            Despawned = false;
            Canvas = Is.Spawned(_parentWidget) ? _parentWidget.Canvas:GetComponentInParent<Canvas>();

            for (var i = 0; i < m_StaticChildren.Count; i++)
            {
                var prefab = Is.NotNull(Prefab) && Prefab.m_StaticChildren.Count > i
                    ? Prefab.m_StaticChildren[i]
                    : null;
                ((WidgetBuilder.ISecret)m_StaticChildren[i]).Init(prefab,this, _ownerWidget, _context);
            }
        }

        /// <summary>
        /// Called on widgets when we're trying to despawn them
        /// This call is repeated every frame to check if the widget is ready to despawn 
        /// </summary>
        /// <returns>If this widget is ready to be despawned</returns>
        protected virtual bool TryToDespawn() => true;

        public bool IsDespawning { get; private set; }

        public WidgetContext Context { get; private set; }
        public bool IsRefreshing { get; private set; }

        bool WidgetBuilder.ISecret.TryToDespawn()
        {
            IsDespawning = true;
            return TryToDespawn();
        }

        /// <summary>
        /// OnValidate is used to rebuild widgets in editor if modified
        /// </summary>
        protected virtual void OnValidate()
        {
#if UNITY_EDITOR
            this.EnsureComponent(ref m_RectTransform); // ensure we have a RectTransform at all times in editor.
            BuildStaticChildrenList();

            EditorApplication.delayCall += () =>
            {
                if (Is.NotNull(this))
                    SetDirty();
            };
#endif
        }

        protected virtual void OnEnable()
        {
            SetDirty(true);  
        }

        protected virtual void OnDisable()
        {
            m_Hovered.Clear();
        }

        protected virtual void Awake()
        {
            this.EnsureComponent(ref m_RectTransform);

            BuildStaticChildrenList();

            this.EnsureComponent(ref m_LayoutElement);
            if (!this.EnsureComponent(ref m_ContentSizeFitter))
            {
                m_ContentSizeFitter.enabled = false;
            }

#if UNITY_EDITOR
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(this);
            if (Is.NotNull(prefab))
            {
                WidgetManager.EditorRegisterPrefabInstance(this, prefab);
            }
#endif
        }

        protected virtual void Start()
        {
            // this handles the case non-spawned root widgets that wouldn't have Init called otherwise
            if (!HasRefreshed && !Is.NotNull(OwnerWidget))
            {
                var prefab = WidgetManager.LookupPrefab(this.GetType());
                ((WidgetBuilder.ISecret)this).Init(prefab,null, this, this.AsContextReference());
            }
        }
        
        void BuildStaticChildrenList()
        {
            m_StaticChildren.Clear();
            for (int i = 0; i < m_RectTransform.childCount; i++)
            {
                var child = m_RectTransform.GetChild(i);
                if (!child.gameObject.activeSelf || !child.TryGetComponent(out WidgetBehaviour childWidget) || (child.hideFlags & HideFlags.DontSave) != 0)
                    continue;

                m_StaticChildren.Add(childWidget);
            }

            for (var i = 0; i < m_StaticChildren.Count; i++)
            {
                if (!Is.NotNull(m_StaticChildren[i])) continue;
                m_StaticChildren[i].m_WidgetIndex = i;
            }
        }

        void WidgetBuilder.ISecret.ClearHasRefreshed() { HasRefreshed = false; }
        void WidgetBuilder.ISecret.ClearDirty()
        {
            IsDirty = false;
        }

        void WidgetBuilder.ISecret.OnDespawn()
        {
            ResetState();
            IsDespawning = false;
            Despawned = true;
            ParentWidget = null;
            OwnerWidget = null;
            Depth = 0;
            HasRefreshed = false;
            IsDirty = false;
            Canvas = null;
            foreach (var child in m_StaticChildren)
            {
                if (!Is.NotNull(child)) continue;
                ((WidgetBuilder.ISecret)child).OnDespawn();
            }
        }

        void WidgetBuilder.ISecret.SortChildren()
        {
            if (!m_InternalChildren.OrderChanged && !m_OwnerChildren.OrderChanged)
                return;
            
            m_Children.Clear();
            foreach (var child in m_StaticChildren)
            {
                if(m_Children.Count==0 
                   || m_Children[m_Children.Count-1].RectTransform.GetSiblingIndex() <= child.RectTransform.GetSiblingIndex())
                {
                    m_Children.Add(child);
                } else
                {
                    var insertAt = m_Children.BinarySearch(child, default(WidgetSiblingIndexComparision));
                    if (insertAt < 0) insertAt = ~insertAt;
                    m_Children.Insert(insertAt, child);
                }
            }

            var index = RectTransform.childCount - m_InternalChildren.Count - m_OwnerChildren.Count;
            foreach (var child in m_InternalChildren)
            {
                var childTransform = child.prefabInstance.RectTransform;
                if(childTransform.GetSiblingIndex()!=index)
                    childTransform.SetSiblingIndex(index);
                
                m_Children.Add(child.prefabInstance);
                ++index;
            }

            foreach (var child in m_OwnerChildren)
            {
                var childTransform = child.prefabInstance.RectTransform;
                if(childTransform.GetSiblingIndex()!=index)
                    childTransform.SetSiblingIndex(index);
                
                m_Children.Add(child.prefabInstance);
                ++index;
            }

            if (m_DebugLogging.HasFlag(LogLevel.Internal))
            {
                var sb = new StringBuilder();
                sb.AppendLine($"{name} Sorted children");
                for (int i = 0; i < m_Children.Count; i++)
                {
                    sb.AppendLine($"\t[{i}] {m_Children[i]}");
                }
                LogInternal(sb.ToString());
            }
        }
        void WidgetBuilder.ISecret.SetContext(WidgetContext _context) 
            => Context = _context;

        public virtual void ResetState() {}

        public T GetValue<T>(in Optional<T> _optional, in T _valueIfUnused)
        {
            return _optional.GetValue(_valueIfUnused, UsesTypePrefab);
        }
        public override string ToString()
        {
            return $"{name}:{WidgetIndex}:{(Despawned?"y":(IsDespawning?"n*":"n"))}";
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            ClearEvent<EHovered>();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            this.SetEvent(default(EHovered));
        }

        [Conditional("WIDGETS_LOG_ERROR")]
        public void LogError(string message) => Log(LogLevel.Error, message);
        
        [Conditional("WIDGETS_LOG_WARNING")]
        public void LogWarning(string message) => Log(LogLevel.Warning, message);
        
        [Conditional("WIDGETS_LOG_INFO")]
        public void LogInfo(string message) => Log(LogLevel.Info, message);
        
        [Conditional("WIDGETS_LOG_DEBUGGING")]
        public void LogDebugging(string message) => Log(LogLevel.Debugging, message);
        
        [Conditional("WIDGETS_LOG_INTERNAL")]
        public void LogInternal(string message) => Log(LogLevel.Internal, message);
        
        public void Log(LogLevel level, string message)
        {
            if (!DebugLogging.HasFlag(level)) 
                return;
            
            switch (level)
            {
                case LogLevel.Error:   
                    Debug.LogError(message,this);
                    return;
                
                case LogLevel.Warning: 
                    Debug.LogWarning(message,this);
                    return;
                
                default:
                    Debug.Log(message,this);
                    return;
            }
        }

        public bool HasEvent<T>() where T : struct, IWidgetEvent
        {
            return (this is IWidgetHasEvent<T> secret && secret.EventStorage.Get(out var _));
        }
        
        public bool HasEvent<T>(out T status) where T : struct, IWidgetEvent
        {
            if (this is IWidgetHasEvent<T> secret 
                && secret.EventStorage.Get(out status))
            {
                return true;
            }
            status = default;
            return false;
        }

        public void ClearEvent<T>() where T : struct, IWidgetEvent
        {
            if (this is IWidgetHasEvent<T> secret)
            {
                secret.EventStorage.Clear();
                if (Is.NotNull(OwnerWidget))
                {
                    OwnerWidget.SetDirty();
                }
            }
        }
    }
}
