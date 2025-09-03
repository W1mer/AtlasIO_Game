namespace Game.Placement
{

    public interface IPlacementValidator
    {
        bool CanPlace(BuildingInstance building, HexagonMain targetCell);
    }
}
