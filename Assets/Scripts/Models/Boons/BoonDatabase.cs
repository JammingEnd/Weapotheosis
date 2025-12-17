namespace Models.Boons
{
    public static class BoonDatabase
    {
        private static BoonContainer _container;
        
        public static void Initialize(BoonContainer container)
        {
            _container = container;
            _container.Initialize();
        }
        public static BoonCardSC GetBoonById(int id)
        {
            if (_container == null)
            {
                throw new System.Exception("BoonDatabase not initialized. Call Initialize() with a BoonContainer before accessing boons.");
            }
            return _container.GetBoonById(id);
        }
    }
}