using UnityEngine;
using UnityEngine.UIElements;

namespace RUI
{
    internal static class Helpers
    {
        #region Dropdown Menu Extensions
        public static bool IsHidden(this DropdownMenuAction action) =>
            (action.status & DropdownMenuAction.Status.Hidden) == DropdownMenuAction.Status.Hidden
                || action.status == DropdownMenuAction.Status.None;
        public static bool IsDisabled(this DropdownMenuAction action) =>
            (action.status & DropdownMenuAction.Status.Disabled)
                == DropdownMenuAction.Status.Disabled;
        public static bool IsChecked(this DropdownMenuAction action) =>
            (action.status & DropdownMenuAction.Status.Checked)
                == DropdownMenuAction.Status.Checked;
        public static bool IsNormal(this DropdownMenuAction action) =>
            (action.status & DropdownMenuAction.Status.Normal)
                == DropdownMenuAction.Status.Normal;
        #endregion
    }

    #region Safe Zone Triange 
    internal struct Triangle
    {
        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;

        public Rect BoundingBox()
        {
            float sx0 = p0.x;
            float sx1 = p1.x;
            float sx2 = p2.x;
            float sy0 = p0.y;
            float sy1 = p1.y;
            float sy2 = p2.y;
            float xMax = sx0 > sx1 ? (sx0 > sx2 ? sx0 : sx2) : (sx1 > sx2 ? sx1 : sx2);
            float yMax = sy0 > sy1 ? (sy0 > sy2 ? sy0 : sy2) : (sy1 > sy2 ? sy1 : sy2);
            float xMin = sx0 < sx1 ? (sx0 < sx2 ? sx0 : sx2) : (sx1 < sx2 ? sx1 : sx2);
            float yMin = sy0 < sy1 ? (sy0 < sy2 ? sy0 : sy2) : (sy1 < sy2 ? sy1 : sy2);
            return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
        }

        public bool ContainsPoint(Vector2 p)
        {
            float dX = p.x - p2.x;
            float dY = p.y - p2.y;
            float dX21 = p2.x - p1.x;
            float dY12 = p1.y - p2.y;
            float D = dY12 * (p0.x - p2.x) + dX21 * (p0.y - p2.y);
            float s = dY12 * dX + dX21 * dY;
            float t = (p2.y - p0.y) * dX + (p0.x - p2.x) * dY;
            if (D < 0) return s <= 0 && t <= 0 && s + t >= D;
            return s >= 0 && t >= 0 && s + t <= D;
        }

        public override string ToString() => $"{p0} - {p1} - {p2}";
    }
    #endregion
}
