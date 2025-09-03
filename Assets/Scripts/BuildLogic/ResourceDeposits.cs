using UnityEngine;

public class ResourceDeposits : MonoBehaviour
{
    public int Id; // Присваивается в инспекторе или из кода
    public bool IsDiscovered { get; private set; } = false;

    private void Start()
    {
        ResourceDepositManager.Instance.RegisterDeposit(this);

        if (IsDiscovered == true)
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }

        if ((Id == 0 || Id == 1) && IsDiscovered == false)
            SetDiscovered(true);
    }

    public void Discover()
    {
        if (!IsDiscovered)
        {
            IsDiscovered = true;
            gameObject.SetActive(true);
            
            ResourceDepositManager.Instance.SaveDeposits(); // Сохраняем централизованно
        }
    }

    public void SetDiscovered(bool state)
    {
        IsDiscovered = state;
        gameObject.SetActive(state);
    }
}
