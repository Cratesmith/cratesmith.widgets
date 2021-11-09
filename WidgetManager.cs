using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Object = System.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace com.cratesmith.widgets
{
    /// <summary>
    /// Manages rebuilds of widgets at runtime. 
    /// </summary>
    public class WidgetManager : MonoBehaviour
    {
        static int s_CurrentRefreshDepth = -1;

        static List<List<WidgetBehaviour>> s_RefreshByDepth 
            = new List<List<WidgetBehaviour>>();

        static List<List<WidgetBehaviour>> s_NextRefreshByDepth 
            = new List<List<WidgetBehaviour>>();
        
        Dictionary<WidgetBehaviour, Queue<WidgetBehaviour>> m_PrefabPool =
            new Dictionary<WidgetBehaviour, Queue<WidgetBehaviour>>();

        static Dictionary<Type, WidgetBehaviour> s_TypeOnlyPrefabs 
            = new Dictionary<Type, WidgetBehaviour>();

        Transform m_Transform;
        public Transform Transform => m_Transform;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void EditorInitializeOnLoad()
        {
            if (Application.isPlaying) 
                return;

            EditorCleanup();
            Undo.undoRedoPerformed += () =>
            {
                EditorCleanup();
            };

            EditorApplication.hierarchyChanged += EditorRefreshSelection;
            EditorApplication.projectChanged += EditorRefreshSelection;
            EditorApplication.update += () =>
            {
                EditorRefreshSelection();
                RefreshDirty();
            };
        }

        static void EditorRefreshSelection()
        {
            foreach (var go in Selection.gameObjects)
            {
                if (!go.TryGetComponent(out WidgetBehaviour wb)) continue;
                if (wb.SetDirty())
                {
                    //Debug.Log($"Setting {wb.name}({wb.gameObject.scene.name}) dirty by selection edit");
                }
            }
        }

        static void EditorCleanup()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage)
            {
                EditorCleanup(stage.FindComponentsOfType<WidgetBehaviour>());
            }

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                var scene = EditorSceneManager.GetSceneAt(i);
                if(!scene.IsValid() || !scene.isLoaded) continue;
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    EditorCleanup(root.GetComponentsInChildren<WidgetBehaviour>(true));
                }
            }
        }

        static void EditorCleanup(WidgetBehaviour[] widgets)
        {
            foreach (var widget in widgets) 
            {
                if (!Is.NotNull(widget)) continue;
                
                if (widget.gameObject.scene.IsValid() // ensure part of scene or PrefabStage
                    && (widget.hideFlags & HideFlags.DontSave) != 0)
                {  
                    DestroyImmediate(widget.gameObject);
                }
                else
                {
                    ((WidgetBuilder.ISecret)widget).ClearDirty();
                    widget.SetDirty();
                }
            }
        }
#endif

        void Awake()
        {
            if (s_Instance && s_Instance != this)
            {
                Destroy(gameObject); 
                return;
            }
            this.EnsureComponent(ref m_Transform);
        }

        public static WidgetBehaviour.WidgetChild<TWidget> SpawnChild<TWidget>(TWidget _widget, WidgetBehaviour _prefab, WidgetBehaviour _parentWidget, WidgetBehaviour _ownerWidget, WidgetContext _context) where TWidget : WidgetBehaviour
            => Spawn(_widget, _prefab, _parentWidget.RectTransform, _parentWidget, _ownerWidget, _context);


        static Stack<int> s_BackwalkStack = new Stack<int>();
        static WidgetBehaviour.WidgetChild<TWidget> Spawn<TWidget>(TWidget _widget, WidgetBehaviour _prefab, RectTransform _parent, WidgetBehaviour _parentWidget, WidgetBehaviour _ownerWidget, WidgetContext _context) where TWidget : WidgetBehaviour
        {
            WidgetBehaviour prefabInstance = null;
            TWidget widgetInstance = null;
            TryGet(out var manager);
            
            if (!Is.NotNull(_prefab))
            {
                _prefab = _widget;
            }
            
            _prefab = LookupPrefab(typeof(TWidget),_prefab);

            if (manager)
            {
                if (!manager.m_PrefabPool.TryGetValue(_prefab, out var queue))
                {
                    queue = manager.m_PrefabPool[_prefab] = new Queue<WidgetBehaviour>();
                }
                
                if (queue.Count > 0)
                {
                    prefabInstance = queue.Dequeue();
                    Assert.IsTrue(prefabInstance.Despawned);
                }
            }

            lock (s_BackwalkStack)
            {
                s_BackwalkStack.Clear();
                if (Is.NotNull(_widget) && _widget != _prefab)
                {
                    var current = (Transform)_widget.RectTransform;
                    var dest = _prefab.RectTransform;
                    while (current && current != dest)
                    {
                        s_BackwalkStack.Push(current.GetSiblingIndex());
                        current = current.parent;
                        if (!current)
                        {
                            Debug.LogError($"did not find prefab root:{_prefab.name} as a parent of widget prefab:{_widget.name}");
                            s_BackwalkStack.Clear();
                            _prefab = _widget;
                        }
                        else if (current == _ownerWidget.RectTransform)
                        {
                            Debug.LogError($"prefab for widget {_widget.name} (owner {_ownerWidget.name}) appears to be cyclic");
                            s_BackwalkStack.Clear();
                            _prefab = _widget;
                        }
                    }
                }
                
                if (!Is.NotNull(prefabInstance))
                {
                    prefabInstance = InstantiateNew(_prefab, _parent);
                    prefabInstance.gameObject.hideFlags = HideFlags.DontSave;
                }
                else
                {
                    prefabInstance.transform.SetParent(_parent);
                }

                if (Is.NotNull(_widget)
                    && Is.NotNull(_prefab) 
                    && _widget != _prefab)
                {
                    Assert.IsTrue(s_BackwalkStack.Count > 0);

                    // backwalk up the stack to find our desired child
                    Transform current = prefabInstance.RectTransform;
                    while (s_BackwalkStack.Count > 0)
                    {
                        var childId = s_BackwalkStack.Pop();
                        Assert.IsTrue(childId < current.childCount);
                        current = current.GetChild(childId);
                    }
                    widgetInstance = current.GetComponent<TWidget>();
                    Assert.IsNotNull(widgetInstance);
                } 
                else
                {
                    widgetInstance = (TWidget)prefabInstance;
                }
                
                s_BackwalkStack.Clear();
            }

            Assert.IsNotNull(_prefab);
            ((WidgetBuilder.ISecret) prefabInstance).Init(_prefab, _parentWidget, _ownerWidget, _context);
            prefabInstance.gameObject.SetActive(true);

#if UNITY_EDITOR
            if (!s_InstancesInEditor.TryGetValue(_prefab.gameObject, out var list))
            {
                s_InstancesInEditor[_prefab.gameObject] = list = new HashSet<WidgetBehaviour>();
            }
            list.Add(prefabInstance);  
            // Debug.Log($"Spawned instance {instance.name}({instance.gameObject.scene.name})", instance);
            // Debug.Log($"\tfrom prefab from prefab {_prefab.name}({_prefab.gameObject.scene.name})", _prefab);
            
#endif
            return new WidgetBehaviour.WidgetChild<TWidget>()
            {
                widget = widgetInstance,
                prefabInstance = prefabInstance,
            };
        }

        static T InstantiateNew<T>(T _prefab, Transform _parent) where T : WidgetBehaviour
        {
            if (!Is.NotNull(_prefab))
            {
                var go = new GameObject(typeof(T).Name);
                go.transform.SetParent(_parent);
                return go.AddComponent<T>();
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var instance = (T) PrefabUtility.InstantiatePrefab(_prefab, _parent);
                if (Is.NotNull(instance)) return instance;
            }
#endif
            return  Instantiate(_prefab, _parent);
        }


        public static void Despawn(WidgetBehaviour _widget)
        {
            if (!Is.Spawned(_widget))  
                return;
            
            if (Application.isPlaying
                && !((WidgetBuilder.ISecret)_widget).TryToDespawn())
            {
                s_DelayedDespawns.Add(_widget);
                return;
            }
            
            DespawnImmediately(_widget);
        }
        
        public static void DespawnImmediately(WidgetBehaviour _widget)
        {
            if (!Is.Spawned(_widget)) 
                return; 
            
 #if UNITY_EDITOR
            if (Is.NotNull(_widget.Prefab) && s_InstancesInEditor.TryGetValue(_widget.Prefab.gameObject, out var list))
            {
                list.Remove(_widget);
            }

            s_InstancesInEditor.Remove(_widget.gameObject);

            if (!Application.isPlaying)
            {
                DestroyImmediate(_widget.gameObject);
                return;
            }
#endif

            ((WidgetBuilder.ISecret)_widget).OnDespawn();
            
            foreach (var group in ((WidgetBuilder.ISecret)_widget).OwnerChildren.ContextGroups.Values)
            {
                foreach (var child in @group)
                {
                    DespawnImmediately(child.prefabInstance);
                }
            }
            ((WidgetBuilder.ISecret)_widget).OwnerChildren.Clear();

            foreach (var group in ((WidgetBuilder.ISecret)_widget).InternalChildren.ContextGroups.Values)
            {
                foreach (var child in @group)
                {
                    DespawnImmediately(child.prefabInstance);
                }
            }
            ((WidgetBuilder.ISecret)_widget).InternalChildren.Clear();

            if (Is.NotNull(_widget.Prefab) && TryGet(out var manager))
            {
                _widget.gameObject.SetActive(false);
                _widget.transform.SetParent(manager.m_Transform);
                if (!manager.m_PrefabPool.TryGetValue(_widget.Prefab, out var queue))
                {
                    queue = manager.m_PrefabPool[_widget.Prefab] = new Queue<WidgetBehaviour>();
                }
                queue.Enqueue(_widget);
            }
        }

        static WidgetManager                                    s_Instance;
        static Dictionary<GameObject, HashSet<WidgetBehaviour>> s_InstancesInEditor = new Dictionary<GameObject, HashSet<WidgetBehaviour>>();
        static HashSet<WidgetBehaviour>                         s_DelayedDespawns = new HashSet<WidgetBehaviour>();

        // ReSharper disable Unity.PerformanceAnalysis
        static bool TryGet(out WidgetManager _output) 
        {
            if (!s_Instance && Application.isPlaying)
            {
                var go = new GameObject(nameof(WidgetManager));
                DontDestroyOnLoad(go);
                s_Instance = go.AddComponent<WidgetManager>();
            }

            _output = s_Instance;
            return _output;
        }
        public static void MarkForRebuild(WidgetBehaviour _widgetBehaviour, bool _forceImmediate)
        {
            if (Application.isPlaying)
            {
                TryGet(out var manager);
            }

            var refreshList = s_CurrentRefreshDepth < _widgetBehaviour.Depth || _forceImmediate
                ? s_RefreshByDepth
                : s_NextRefreshByDepth;
            
            for (int i = refreshList.Count; i <= _widgetBehaviour.Depth; i++)
            {
                refreshList.Add(new List<WidgetBehaviour>());
            }
            
            refreshList[_widgetBehaviour.Depth].Add(_widgetBehaviour);     
            // Debug.Log($"Marking {_widgetBehaviour.name}({_widgetBehaviour.gameObject.scene.name}) for rebuild (depth {_widgetBehaviour.Depth}) ({(s_InstancesInEditor.TryGetValue(_widgetBehaviour.gameObject, out var instList)?instList.Count:0)} instances)", _widgetBehaviour);
#if UNITY_EDITOR
            if(s_InstancesInEditor.TryGetValue(_widgetBehaviour.gameObject, out var list))
            {
                foreach (var widget in list)
                {
                    if (Is.Spawned(widget))
                    {
                        ((WidgetBuilder.ISecret)widget).ClearHasRefreshed();
                        if (widget.SetDirty())
                        {
                            // Debug.Log($"Set {widget.name}({widget.gameObject.scene.name}) dirty because of prefab change");
                        }
                    }
                }
            }
#endif
        }

        public static bool IsWidget<TWidget>(WidgetBehaviour widget, out TWidget output) 
            where TWidget:WidgetBehaviour
        {
            if (Is.Spawned(widget) && widget is TWidget result && !result.Despawned)
            {
                output = result;
                return true;
            }
            output = null;
            return false;
        }

        void LateUpdate()
        {
            if (Application.isPlaying)
            {
                DoDelayedDespawns();
                RefreshDirty();
            }
        }
        
        static void DoDelayedDespawns()
        {
            s_DelayedDespawns.RemoveWhere(x =>
            {
                if (!Is.NotNull(x))
                    return true;

                if (!((WidgetBuilder.ISecret)x).TryToDespawn())
                    return false;

                // refresh children after removing a child
                if (Is.Spawned(x.OwnerWidget))
                {
                    x.OwnerWidget.SetDirty(); 
                }
                
                DespawnImmediately(x);
                
                return true;
            });
        }

        static void RefreshDirty()
        {
            while(true)
            {
                s_CurrentRefreshDepth = 0;
                while (s_CurrentRefreshDepth < s_RefreshByDepth.Count && s_RefreshByDepth[s_CurrentRefreshDepth].Count == 0)
                    ++s_CurrentRefreshDepth;

                if (s_CurrentRefreshDepth >= s_RefreshByDepth.Count)
                    break;
                
                var refreshList = s_RefreshByDepth[s_CurrentRefreshDepth];
                for (var j = 0; j < refreshList.Count; j++)
                {
                    var widgetBehaviour = refreshList[j];
                    if (!Is.Spawned(widgetBehaviour))
                        continue;

                    if (!widgetBehaviour.gameObject.activeInHierarchy)
                    {
                        ((WidgetBuilder.ISecret)widgetBehaviour).ClearDirty();
                        continue;
                    }

                    try
                    {
                        widgetBehaviour.Refresh();

                        // fixes issue with layout lifecycle in edit mode
                        if (!Application.isPlaying)
                        {
                            LayoutRebuilder.ForceRebuildLayoutImmediate(Is.Spawned(widgetBehaviour.ParentWidget)
                                                                            ? widgetBehaviour.ParentWidget.RectTransform : widgetBehaviour.RectTransform);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
                refreshList.Clear();
            }
            
            s_CurrentRefreshDepth = -1;
            var swap = s_RefreshByDepth;
            s_RefreshByDepth = s_NextRefreshByDepth;
            s_NextRefreshByDepth = swap;
        } 

        public static void EditorRegisterPrefabInstance(WidgetBehaviour _widget, WidgetBehaviour _prefab)
        {
            if (!s_InstancesInEditor.TryGetValue(_prefab.gameObject, out var list))
            {
                list = s_InstancesInEditor[_prefab.gameObject] = new HashSet<WidgetBehaviour>();
            }
            list.Add(_widget);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        public static WidgetBehaviour LookupPrefab(Type _type, WidgetBehaviour _prefab=null) 
        {  
            if (Is.NotNull(_prefab)) 
                return _prefab;
            
            TryGet(out var manager);
            if (!s_TypeOnlyPrefabs.TryGetValue(_type, out var newPrefab) || !Is.NotNull(newPrefab))
            {
                var go = new GameObject(_type.Name);
                go.hideFlags = HideFlags.DontSave;
                go.transform.SetParent(manager ? manager.m_Transform:null);
                s_TypeOnlyPrefabs[_type] = newPrefab = (WidgetBehaviour)go.AddComponent(_type);
                ((WidgetBuilder.ISecret)newPrefab).Init(null, null, null, default);
                newPrefab.gameObject.SetActive(false);
            }
            return newPrefab;
        }
    }

}
