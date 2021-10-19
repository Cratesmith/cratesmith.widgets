using UnityEngine;
using UnityEngine.UI;

namespace com.cratesmith.widgets
{
    public class BasicLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        [SerializeField] bool m_AutoMarkForRebuild = true;
        public bool autoMarkForRebuild 
        {
            get => m_AutoMarkForRebuild;
            set
            {
                if (m_AutoMarkForRebuild == value) return;
                m_AutoMarkForRebuild = value;
                if(Application.isPlaying && m_AutoMarkForRebuild) 
                    LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }
        
        [SerializeField] bool m_IsVertical;
        public bool isVertical
        {
            get => m_IsVertical;
            set
            {
                if (m_IsVertical == value) return;
                m_IsVertical = value;
                if(Application.isPlaying) 
                    LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if(m_AutoMarkForRebuild) base.OnRectTransformDimensionsChange();
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, m_IsVertical);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, m_IsVertical);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, m_IsVertical);
        }

        /// <summary>
        /// Called by the layout system. Also see ILayoutElement
        /// </summary>
        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, m_IsVertical);
        }
    }
}