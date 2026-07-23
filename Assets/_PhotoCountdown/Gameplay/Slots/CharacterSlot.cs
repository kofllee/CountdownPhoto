using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSlot : MonoBehaviour
{
    [SerializeField] private List<CharacterSlot> _neighbors = new();
    
    public IReadOnlyList<CharacterSlot> Neighbors => _neighbors;

    public bool IsNeighbor(CharacterSlot characterSlot)
    {
        return _neighbors.Contains(characterSlot);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {

        foreach (CharacterSlot neighbor in _neighbors.ToList())
        {
            if (neighbor)
                neighbor.AddNeighborFromEditor(this);
            
            if(neighbor == this)
                _neighbors.Remove(neighbor);
        }
    }

    private void AddNeighborFromEditor(CharacterSlot slot)
    {
        if (!slot || slot == this || _neighbors.Contains(slot))
            return;

        _neighbors.Add(slot);
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
    
    private void OnDrawGizmosSelected()
    {
        foreach (CharacterSlot neighbor in _neighbors)
        {
            if (!neighbor)
                continue;

            Gizmos.DrawLine(transform.position, neighbor.transform.position);
        }
    }
}
