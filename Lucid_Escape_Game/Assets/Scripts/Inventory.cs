using System;
using System.Collections.Generic;
using UnityEngine;

/// Un obiect din inventar. Date simple, fara ScriptableObject,
/// ca sa nu fie nevoie sa creezi asseturi separate pentru fiecare item.
[Serializable]
public class Item
{
    public string id = "item";           // folosit de puzzle-uri: "cheie_camera6"
    public string displayName = "Obiect";
    [TextArea(2, 5)] public string description;
    public Sprite icon;

    public Item() { }

    public Item(string id, string displayName, string description = "", Sprite icon = null)
    {
        this.id = id;
        this.displayName = displayName;
        this.description = description;
        this.icon = icon;
    }
}

/// Inventarul jucatorului. Se pune pe acelasi obiect ca PlayerController.
public class Inventory : MonoBehaviour
{
    [Tooltip("Cate obiecte incap. 0 = nelimitat.")]
    public int capacity = 12;

    [SerializeField] List<Item> items = new List<Item>();

    /// Se declanseaza la orice adaugare sau scoatere (pentru UI).
    public event Action Changed;

    public IReadOnlyList<Item> Items => items;
    public int Count => items.Count;
    public bool IsFull => capacity > 0 && items.Count >= capacity;

    public bool Add(Item item)
    {
        if (item == null || IsFull) return false;
        items.Add(item);
        Changed?.Invoke();
        return true;
    }

    public bool Remove(string id)
    {
        int i = items.FindIndex(x => x.id == id);
        if (i < 0) return false;
        items.RemoveAt(i);
        Changed?.Invoke();
        return true;
    }

    /// Pentru puzzle-uri: "are jucatorul cheia?"
    public bool Has(string id) => items.Exists(x => x.id == id);

    public void Clear()
    {
        items.Clear();
        Changed?.Invoke();
    }
}
