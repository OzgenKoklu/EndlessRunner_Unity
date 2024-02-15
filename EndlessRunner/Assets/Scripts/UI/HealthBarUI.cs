using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarUI : MonoBehaviour
{

    [SerializeField] private Player _player;
    [SerializeField] private Transform _hearthTemplate;
    [SerializeField] private Transform _hearthContainer;

    private void Awake()
    {
        _hearthTemplate.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        Player.Instance.OnHealthChanged += Player_OnHealthChanged;
        UpdateVisual();
    }

    private void Player_OnHealthChanged(object sender, System.EventArgs e)
    {
        UpdateVisual();  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateVisual()
    {
        foreach(Transform child in _hearthContainer)
        {
            if (child == _hearthTemplate) continue;
            Destroy(child.gameObject);
        }

        int playerHealthAmount = Player.Instance.PlayerHealthAmount();
        for (int i = 0; i < playerHealthAmount ; i++)
        {
            Transform hearthTransform = Instantiate(_hearthTemplate, _hearthContainer);
            hearthTransform.gameObject.SetActive(true);
        }

    }
}
