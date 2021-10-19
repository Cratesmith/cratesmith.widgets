namespace com.cratesmith.widgets
{
	public class WHorizontalBasicLayout 
		: WBasicLayout
	{
		protected override void Awake()
		{
			if (!this.EnsureComponent(ref m_BasicLayoutGroup))
			{
				m_BasicLayoutGroup.isVertical = false;
				m_BasicLayoutGroup.childForceExpandWidth = false;
				m_BasicLayoutGroup.childForceExpandHeight = false;
				m_BasicLayoutGroup.childControlWidth = true;
				m_BasicLayoutGroup.childControlHeight = true;
			}
			base.Awake();
		}
	}
}
