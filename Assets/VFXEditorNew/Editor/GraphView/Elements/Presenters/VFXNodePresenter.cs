using UIElements.GraphView;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace UnityEditor.VFX.UI
{
    class VFXNodePresenter : VFXLinkablePresenter, IVFXPresenter
    {
        public VFXModel model { get { return m_Node; } }

        [SerializeField]
        private VFXSlotContainerModel<VFXModel, VFXModel> m_Node;

        public override Rect position
        {
            get
            {
                return base.position;
            }

            set
            {
                base.position = value;
                var newPos = base.position.position;
                if (node.position != newPos)
                {
                    Undo.RecordObject(node, "Position");
                    node.position = base.position.position;
                }
            }
        }

        public override bool expanded
        {
            //TODOPAUL : Share this behavior with contextPresenter
            get
            {
                return base.expanded;
            }

            set
            {
                base.expanded = value;
                var newCollapsed = !expanded;
                if (newCollapsed != node.collapsed)
                {
                    Undo.RecordObject(node, "Collapse");
                    node.collapsed = newCollapsed;
                }
            }
        }

        public override IVFXSlotContainer slotContainer { get { return m_Node; } }

        public VFXSlotContainerModel<VFXModel, VFXModel> node { get { return m_Node; } }

        protected virtual NodeAnchorPresenter CreateAnchorPresenter(VFXSlot slot, Direction direction)
        {
            VFXOperatorAnchorPresenter anchor;
            if (direction == Direction.Input)
            {
                anchor = CreateInstance<VFXInputOperatorAnchorPresenter>();
            }
            else
            {
                anchor = CreateInstance<VFXOutputOperatorAnchorPresenter>();
            }
            anchor.Init(model, slot, this);

            return anchor;
        }

        public static bool SequenceEqual<T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second, Func<T1, T2, bool> comparer)
        {
            using (IEnumerator<T1> e1 = first.GetEnumerator())
                using (IEnumerator<T2> e2 = second.GetEnumerator())
                {
                    while (e1.MoveNext())
                    {
                        if (!(e2.MoveNext() && comparer(e1.Current, e2.Current)))
                            return false;
                    }

                    if (e2.MoveNext())
                        return false;
                }

            return true;
        }

        void CreateAllAnchorPresenter(IEnumerable<VFXSlot> slots, Direction direction, List<NodeAnchorPresenter> results)
        {
            foreach (VFXSlot slot in slots)
            {
                NodeAnchorPresenter pres = CreateAnchorPresenter(slot, direction);

                results.Add(pres);

                //if( slot.expanded )
                {
                    CreateAllAnchorPresenter(slot.children, direction, results);
                }
            }
        }

        virtual protected void Reset()
        {
            if (m_Node != null)
            {
                position = new Rect(model.position.x, model.position.y, position.width, position.height);
                expanded = !model.collapsed;

                //TODOPAUL : Avoid this hotfix (case with Append operator : outputSlots change when GetExpression is called from CreateAnchorPresenter)
                foreach (var slot in node.outputSlots.ToArray())
                {
                    slot.GetExpression();
                }

                List<NodeAnchorPresenter> newinputAnchors = new List<NodeAnchorPresenter>();
                CreateAllAnchorPresenter(node.inputSlots, Direction.Input, newinputAnchors);


                List<NodeAnchorPresenter> newoutputAnchors = new List<NodeAnchorPresenter>();
                CreateAllAnchorPresenter(node.outputSlots, Direction.Output, newoutputAnchors);

                Func<NodeAnchorPresenter, NodeAnchorPresenter, bool> fnComparer = delegate(NodeAnchorPresenter x, NodeAnchorPresenter y)
                    {
                        var X = x as VFXOperatorAnchorPresenter;
                        var Y = y as VFXOperatorAnchorPresenter;
                        return X.model == Y.model
                            && X.name == Y.name
                            && X.anchorType == Y.anchorType;
                    };

                if (!SequenceEqual(newinputAnchors, inputAnchors, fnComparer)
                    || !SequenceEqual(newoutputAnchors, outputAnchors, fnComparer))
                {
                    inputAnchors.Clear();
                    outputAnchors.Clear();
                    inputAnchors.AddRange(newinputAnchors);
                    outputAnchors.AddRange(newoutputAnchors);
                    viewPresenter.RecreateNodeEdges();
                }
            }
            else
            {
                inputAnchors.Clear();
                outputAnchors.Clear();
            }
        }

        void OnOperatorInvalidate(VFXModel model, VFXModel.InvalidationCause cause)
        {
            if (cause == VFXModel.InvalidationCause.kUIChanged ||
                cause == VFXModel.InvalidationCause.kExpressionInvalidated)
                return;

            if (model == m_Node)
            {
                Reset();
            }
        }

        public virtual void Init(VFXModel model, VFXViewPresenter viewPresenter)
        {
            base.Init(viewPresenter);

            if (m_Node != model)
            {
                if (m_Node != null)
                {
                    m_Node.onInvalidateDelegate -= OnOperatorInvalidate;
                }

                m_Node = model as VFXSlotContainerModel<VFXModel, VFXModel>;
                m_Node.onInvalidateDelegate += OnOperatorInvalidate;
            }
            Reset();
        }
    }
}
