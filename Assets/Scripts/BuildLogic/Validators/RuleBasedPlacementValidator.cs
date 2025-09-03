using System.Collections.Generic;

namespace Game.Placement
{
    /// <summary>
    /// Валидатор, который использует набор правил (IPlacementRule).
    /// </summary>
    public class RuleBasedPlacementValidator : IPlacementValidator
    {
        private readonly List<IPlacementRule> _rules;

        public RuleBasedPlacementValidator(IEnumerable<IPlacementRule> rules)
        {
            _rules = new List<IPlacementRule>(rules);
        }

        public bool CanPlace(BuildingInstance building, HexagonMain targetCell)
        {
            foreach (var rule in _rules)
            {
                if (!rule.Validate(building, targetCell))
                    return false;
            }
            return true;
        }
    }
}
