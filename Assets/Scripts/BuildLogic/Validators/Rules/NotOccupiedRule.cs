namespace Game.Placement.Rules
{
    /// <summary>
    /// Проверяет, что клетка свободна.
    /// </summary>
    public class NotOccupiedRule : IPlacementRule
    {
        public bool Validate(BuildingInstance building, HexagonMain targetCell)
        {
            return !targetCell.isOccupied;
        }
    }
}
