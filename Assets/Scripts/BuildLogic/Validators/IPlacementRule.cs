namespace Game.Placement
{
    public interface IPlacementRule
    {
        bool Validate(BuildingInstance building, HexagonMain targetCell);
    }
}
