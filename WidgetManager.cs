using System;
using System.Collections.Generic;
using System.Linq;
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
		    var burnAndRebuild = new List<WidgetBehaviour>();
		
            foreach (var go in Selection.gameObjects)
            {
	            if (!go)
	            {
		            continue;
	            }
                if (go.TryGetComponent(out WidgetBehaviour wb))
					wb.SetDirty();
                else if(s_InstancesInEditor.TryGetValue(go, out var list))
                {
	                foreach (var widget in list)
	                {
		                if (!Is.Spawned(widget) || (widget.gameObject.hideFlags & HideFlags.DontSave)!=0)
			                continue;
		                
		                burnAndRebuild.Add(widget);
	                }
                }

                if (burnAndRebuild.Count > 0)
                {
	                foreach(var widget in burnAndRebuild)
	                {
		                widget.OwnerWidget.SetDirty(true);
		                WidgetManager.Despawn(widget);
	                }
					RefreshDirty();	                
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

			if (TryGet(out var manager))
			{
				foreach (var value in manager.m_PrefabPool.Values)
				{
					foreach (var widgetBehaviour in value)
					{
						if (Is.NotNull(widgetBehaviour))
						{
							DestroyImmediate(widgetBehaviour.gameObject);
						}
					}
				}
				manager.m_PrefabPool.Clear();
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

		public static WidgetBehaviour.WidgetChild<TWidget> SpawnChild<TWidget>(TWidget _widget, WidgetBehaviour _prefab, WidgetBehaviour _parentWidget, WidgetBehaviour _ownerWidget, WidgetContext _context, WidgetSorting _sorting) where TWidget : WidgetBehaviour
            => Spawn(_widget, _prefab, _parentWidget, _ownerWidget, _context, _sorting);


		static Stack<int> s_BackwalkStack = new Stack<int>();
		static WidgetBehaviour.WidgetChild<TWidget> Spawn<TWidget>(TWidget _widget, WidgetBehaviour _prefab, WidgetBehaviour _parentWidget, WidgetBehaviour _ownerWidget, WidgetContext _context, WidgetSorting _sorting) where TWidget : WidgetBehaviour
		{
			WidgetBehaviour prefabInstance = null;
			TWidget widgetInstance = null;
            TryGet(out var manager);


			Assert.IsTrue(Is.Spawned(_parentWidget));
			Assert.IsTrue(Is.Spawned(_ownerWidget));
			Assert.IsTrue(_parentWidget.transform.IsChildOf(_ownerWidget.transform));
			
			var parentTransform = _parentWidget.RectTransform;

			if (Is.Null(_prefab))
			{
				_prefab = _widget;
			}

			_prefab = LookupPrefab(typeof(TWidget), _prefab);
			
			if (Is.NotNull(_widget) && _parentWidget.transform.IsChildOf(_widget.transform))
			{
				Debug.LogError($"did not find prefab root:{_widget.name} as a parent of widget prefab:{_widget.name}");
				_widget = null;
				_prefab = null;
			}

			if (Is.NotNull(_widget) && !_widget.transform.IsChildOf(_prefab.transform))
			{
				Debug.LogError($"did not find prefab root:{_prefab.name} as a parent of widget prefab:{_widget.name}");
				_prefab = _widget;
			}

			if (Is.NotNull(_parentWidget) && parentTransform.IsChildOf(_prefab.transform))
			{
				Debug.LogError($"prefab for widget {_widget.name} (parent {_parentWidget.name}) appears to be cyclic");
				_prefab = _widget;
			}
			
			if (Is.NotNull(_ownerWidget) && _ownerWidget.transform.IsChildOf(_prefab.transform))
			{
				Debug.LogError($"prefab for widget {_widget.name} (owner {_ownerWidget.name}) appears to be cyclic");
				_prefab = _widget;
			}

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
					}
				}

				if (Is.Null(prefabInstance))
				{
					prefabInstance = InstantiateNew(_prefab, parentTransform);
                }
                else
				{
					prefabInstance.transform.SetParent(parentTransform);
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
			((WidgetBuilder.ISecret)prefabInstance).Init(_prefab, _parentWidget, _ownerWidget, _context, _sorting); prefabInstance.gameObject.SetActive(true);

			Assert.IsTrue(Is.Spawned(prefabInstance));
			Assert.IsTrue(Is.Spawned(widgetInstance));
			Assert.IsTrue(prefabInstance.RectTransform.IsChildOf(parentTransform));
			if(s_Instance)
				Assert.IsFalse(parentTransform == s_Instance.transform);

#if UNITY_EDITOR
			EditorRegisterPrefabInstance(prefabInstance, _prefab);
#endif

			if ((prefabInstance.DebugLogging & LogLevel.Internal) != 0)
			{
				prefabInstance.LogInternal($"Spawned instance {prefabInstance} from prefab {_prefab}");
			}
			
			prefabInstance.gameObject.hideFlags = HideFlags.DontSave;
			widgetInstance.gameObject.hideFlags = HideFlags.DontSave;
			
			return new WidgetBehaviour.WidgetChild<TWidget>
			{
				widget = widgetInstance,
				prefabInstance = prefabInstance
			};
		}

		static T InstantiateNew<T>(T _prefab, Transform _parent) where T : WidgetBehaviour
		{
			if (Is.Null(_prefab))
			{
                var go = new GameObject(typeof(T).Name);
				go.transform.SetParent(_parent);
				return go.AddComponent<T>();
			}
//             
// #if UNITY_EDITOR
//             if (!Application.isPlaying)
//             {
//                 var instance = (T) PrefabUtility.InstantiatePrefab(_prefab, _parent);
//                 if (Is.NotNull(instance)) return instance;
//             }
// #endif
			return Instantiate(_prefab, _parent);
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

			if (!Application.isPlaying)
			{
				DestroyImmediate(_widget.gameObject);
				return;
			}
#endif
			((WidgetBuilder.ISecret)_widget).OnDespawn();

			if (!Is.NotNull(_widget.Prefab) || !TryGet(out WidgetManager manager))
			{
				DestroyImmediate(_widget.gameObject);
				return;
			}

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

			_widget.gameObject.SetActive(false);
			_widget.transform.SetParent(manager.m_Transform);

			if (!manager.m_PrefabPool.TryGetValue(_widget.Prefab, out Queue<WidgetBehaviour> queue))
			{
				queue = manager.m_PrefabPool[_widget.Prefab] = new Queue<WidgetBehaviour>();
			}

			Assert.IsTrue(((WidgetBuilder.ISecret)_widget).InternalChildren.Count ==0, $"ERROR: {_widget} has internal children when returned to pool");
			Assert.IsTrue(((WidgetBuilder.ISecret)_widget).OwnerChildren.Count==0, $"ERROR: {_widget} has owner children when returned to pool");
			queue.Enqueue(_widget);
		}

		static WidgetManager                                    s_Instance;
		static Dictionary<GameObject, HashSet<WidgetBehaviour>> s_InstancesInEditor = new Dictionary<GameObject, HashSet<WidgetBehaviour>>();
		static HashSet<WidgetBehaviour>                         s_DelayedDespawns   = new HashSet<WidgetBehaviour>();

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
		
		public static void MarkForRebuild(WidgetBehaviour _widgetBehaviour, bool _mustRefreshThisFrame) 
		{
			if (Application.isPlaying)
			{
				TryGet(out var manager); // This just ensures the manager is created here if not already
			}

            var refreshList = s_CurrentRefreshDepth < _widgetBehaviour.Depth || _mustRefreshThisFrame
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

						if (widget != widget.OwnerWidget)
						{
							((WidgetBuilder.ISecret)widget.OwnerWidget).ClearHasRefreshed();
							if (widget.OwnerWidget.SetDirty())
							{
								// Debug.Log($"Set {widget.name}({widget.gameObject.scene.name}) dirty because of prefab change");
							}
						}
					}
				}
			}
#endif
		}

		public static bool IsWidget<TWidget>(WidgetBehaviour widget, out TWidget output)
			where TWidget : WidgetBehaviour
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
				if (Is.Null(x))
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
			const int MAX_PASSES = 3;
			for (int pass = 0; pass < MAX_PASSES; pass++)
			{

				s_CurrentRefreshDepth = 0;
				while (s_CurrentRefreshDepth < s_RefreshByDepth.Count)
				{
					if (s_RefreshByDepth[s_CurrentRefreshDepth].Count > 0)
						RefreshDirtyListAndClear(s_RefreshByDepth[s_CurrentRefreshDepth]);

					++s_CurrentRefreshDepth;
				}

				for (int i = s_CurrentRefreshDepth - 1; i >= 0; i--)
				{
					if (s_RefreshByDepth[i].Count > 0)
						RefreshDirtyListAndClear(s_RefreshByDepth[i]);
				}

				var anyStillDirty = false;
				foreach (var list in s_RefreshByDepth)
				{
					if (list.Count == 0)
						continue;

					anyStillDirty = true;
					break;
				}

				if (anyStillDirty)
					break;
			}

			for (int i = 0; i < s_RefreshByDepth.Count; i++)
			{
				if(s_RefreshByDepth[i].Count == 0)
					continue;

				if(s_NextRefreshByDepth.Count < i)
					s_NextRefreshByDepth.Add(new List<WidgetBehaviour>());

				foreach (var widget in s_RefreshByDepth[i])
				{
					if(!Is.Spawned(widget) || !widget.IsDirty) 
						continue;
					
					widget.LogWarning($"{widget} still dirty at end of refresh (usually due to forced refresh from from an event on it's owner). Deferring update till next frame.");
					s_NextRefreshByDepth[i].Add(widget);
				}
				s_RefreshByDepth[i].Clear();
			}
			
			s_CurrentRefreshDepth = -1;
            var swap = s_RefreshByDepth;
			s_RefreshByDepth = s_NextRefreshByDepth;
			s_NextRefreshByDepth = swap;
		}
		static void RefreshDirtyListAndClear(List<WidgetBehaviour> refreshList)
		{
			for (var j = 0; j < refreshList.Count; j++)
			{
				var widgetBehaviour = refreshList[j];
				if (!Is.Spawned(widgetBehaviour) || !widgetBehaviour.IsDirty)
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

		public static void EditorRegisterPrefabInstance(WidgetBehaviour _widget, WidgetBehaviour _prefab, bool _clear = true)
		{
            foreach (var transform in _prefab.GetComponentsInChildren<Transform>())
			{
                if (_clear || !s_InstancesInEditor.TryGetValue(transform.gameObject, out var list))
				{
					list = s_InstancesInEditor[transform.gameObject] = new HashSet<WidgetBehaviour>();
				}

				list.Add(_widget);
			}

            foreach (var staticChild in _widget.StaticChildren)
			{
				EditorRegisterPrefabInstance(staticChild, _prefab, false);
			}
		}

		// ReSharper disable Unity.PerformanceAnalysis
		public static WidgetBehaviour LookupPrefab(Type _type, WidgetBehaviour _prefab = null)
		{
			if (Is.NotNull(_prefab))
				return _prefab;

            TryGet(out var manager);
            if (!s_TypeOnlyPrefabs.TryGetValue(_type, out var newPrefab) || !Is.NotNull(newPrefab))
			{
                var go = new GameObject(_type.Name);
				go.hideFlags = HideFlags.DontSave;
				go.transform.SetParent(manager ? manager.m_Transform : null);
				s_TypeOnlyPrefabs[_type] = newPrefab = (WidgetBehaviour)go.AddComponent(_type);
				((WidgetBuilder.ISecret)newPrefab).Init(null, null, null, default, default);
				newPrefab.gameObject.SetActive(false);
			}
			return newPrefab;
		}
	}
}
