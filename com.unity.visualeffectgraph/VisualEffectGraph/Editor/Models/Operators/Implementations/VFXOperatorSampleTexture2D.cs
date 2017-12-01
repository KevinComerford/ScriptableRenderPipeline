using System;
using UnityEngine;

namespace UnityEditor.VFX
{
    [VFXInfo(category = "Utility")]
    class VFXOperatorSampleTexture2D : VFXOperator
    {
        override public string name { get { return "Sample Texture2D"; } }

        public class InputProperties
        {
            [Tooltip("The texture to sample from.")]
            public Texture2D texture = Texture2D.whiteTexture;
            [Tooltip("The texture coordinate used for the sampling.")]
            public Vector2 uv = Vector2.zero;
            [Min(0), Tooltip("The mip level to sample from.")]
            public float mipLevel = 0.0f;
        }

        override protected VFXExpression[] BuildExpression(VFXExpression[] inputExpression)
        {
            if (inputExpression.Length != 3)
            {
                return new VFXExpression[] {};
            }

            return new[] { new VFXExpressionSampleTexture2D(inputExpression[0], inputExpression[1], inputExpression[2]) };
        }
    }
}
