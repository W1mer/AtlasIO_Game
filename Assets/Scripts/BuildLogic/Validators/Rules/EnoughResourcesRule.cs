namespace Game.Placement.Rules
{
    /// <summary>
    /// Проверяет, хватает ли ресурсов для постройки.
    /// </summary>
    public class EnoughResourcesRule : IPlacementRule
    {
        public bool Validate(BuildingInstance building, HexagonMain targetCell)
        {
            return ResourceManager.Instance.HasEnoughResources(building.baseData.upgradeLevels[building.currentLevel].cost);
        }
    }
}
