namespace Helpers
{
    public static class CursorHelper
    {
        public static void LockCursor()
        {
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
        public static void UnlockCursor()
        {
            UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
        }
    }
}