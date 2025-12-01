using UnityEngine;

public class CellObject : MonoBehaviour
{

    protected Vector2Int m_Cell;

    public virtual void Init(Vector2Int cell)
    {
        m_Cell = cell;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public virtual void PlayerEntered()
    {
      
    }

    public virtual bool PlayerWantsToEnter()
    {
        return true;
    }
}
