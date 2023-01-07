using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class NextToFightScript : MonoBehaviour
{
    [SerializeField] private GameObject _listContent;
    [SerializeField] private GameObject _iconPrefab;
    private LinkedList<CharacterDisplayScript> _displayOrderData;
    private IconBehavior[] _icons;
    private int _curFighterIndex;

    private void Awake()
    {
        _displayOrderData = new LinkedList<CharacterDisplayScript>();
    }

    public void SetFightOrder(List<CharacterDisplayScript> order)
    {
        if (_icons != null)
        {
            DestroyOld();
        }
        print("fightOrder");
        _curFighterIndex = 0;
        _displayOrderData.Clear();
        _icons = new IconBehavior[order.Count];
        for(int i=0;i<order.Count;i++)
        {
            _displayOrderData.AddLast(order[i]);
            _icons[i] = Instantiate(_iconPrefab, _listContent.transform).GetComponent<IconBehavior>();
            _icons[i].UpdateSprite(order[i].data.characterIcon);
        }
    }

    private void DestroyOld()
    {
        for(int i=0;i<_icons.Length;i++)
        {
            Destroy(_icons[i].gameObject); //TODO: DOSEN'T WORK!
        }
    }

    public void DisplayNextFighter()
    {
        CharacterDisplayScript playedChar = _displayOrderData.First.Value;
        Destroy(_icons[_curFighterIndex]);
        _icons[_curFighterIndex] = Instantiate(_iconPrefab, _listContent.transform).GetComponent<IconBehavior>();
        _icons[_curFighterIndex].UpdateSprite(playedChar.data.characterIcon);
        _curFighterIndex++;
        _displayOrderData.AddLast(playedChar);
        _displayOrderData.RemoveFirst();
    }
}
