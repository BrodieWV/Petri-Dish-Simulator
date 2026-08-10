using UnityEngine;

namespace PetriDish.Presentation.UI
{
    public sealed class AdaptiveDishCardLayoutGroup : UnityEngine.UI.HorizontalOrVerticalLayoutGroup
    {
        [SerializeField] private bool vertical = true;

        public bool IsVertical
        {
            get => vertical;
            set
            {
                if (vertical == value) return;
                vertical = value;
                SetDirty();
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, vertical);
        }

        public override void CalculateLayoutInputVertical() => CalcAlongAxis(1, vertical);
        public override void SetLayoutHorizontal() => SetChildrenAlongAxis(0, vertical);
        public override void SetLayoutVertical() => SetChildrenAlongAxis(1, vertical);
    }
}
