namespace CleanToContinue.Flow
{
    public static class SceneFlow
    {
        public static void Load(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                SceneTransitionController.Load(sceneName);
            }
        }
    }
}
