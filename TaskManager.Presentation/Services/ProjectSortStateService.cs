using TaskManager.Presentation.Enums;


namespace TaskManager.Presentation.Services
{
    public class ProjectSortStateService
    {
        private SortOption _sortSettingCache = SortOption.DateDesc;

        public SortOption? GetSortingMethod()
        {
            return _sortSettingCache;
        }

        public void SetSortingMethod(SortOption option)
        {
            _sortSettingCache = option;
        }






    }
}
