using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RUI
{
    [Obsolete("Rather than render a triangle, we imagine one :)")]
    internal class RUIContextualMenuSafeZone : VisualElement
    {
        private Triangle m_SafeZone;
        private Rect m_SafeZoneBounds;

        internal RUIContextualMenuSafeZone()
        {
            style.position = Position.Absolute;
            style.backgroundColor = Color.black;
            generateVisualContent = OnGenerateVisualContent;
        }

        internal void SetSafeZone(Triangle safeZone)
        {
            // Set Bounding Box
            m_SafeZoneBounds = safeZone.BoundingBox();

            // Convert to Local Triangle within Bounding Box
            m_SafeZone.p0 = safeZone.p0 - m_SafeZoneBounds.position;
            m_SafeZone.p1 = safeZone.p1 - m_SafeZoneBounds.position;
            m_SafeZone.p2 = safeZone.p2 - m_SafeZoneBounds.position;

            // Set Style
            style.width = new Length(m_SafeZoneBounds.width, LengthUnit.Pixel);
            style.height = new Length(m_SafeZoneBounds.height, LengthUnit.Pixel);
            style.left = new Length(m_SafeZoneBounds.x, LengthUnit.Pixel);
            style.top = new Length(m_SafeZoneBounds.y, LengthUnit.Pixel);
            MarkDirtyRepaint();
        }

        public override bool ContainsPoint(Vector2 localPoint)
            => m_SafeZone.ContainsPoint(localPoint);

        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            Debug.Log($"Bounds = {m_SafeZoneBounds}");
            Debug.Log($"Triangle = {m_SafeZone.p0} - {m_SafeZone.p1} - {m_SafeZone.p2}");
            MeshWriteData meshWriteData = ctx.Allocate(3, 3);
            meshWriteData.SetAllVertices(new Vertex[]{
                new(){
                   position = m_SafeZone.p0,
                   tint = Color.white,
                },
                new(){
                   position = m_SafeZone.p1,
                   tint = Color.white,
                },
                new(){
                   position = m_SafeZone.p2,
                   tint = Color.white,
                },
            });
            meshWriteData.SetAllIndices(new ushort[] { 0, 1, 2 });
        }
    }
}
