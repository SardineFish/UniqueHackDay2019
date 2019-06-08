using UnityEngine;
using System.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

public class GrassItem : MapItem
{
    public GameObject Fire;
    public Sprite BurntGrass;
    public Sprite NormalGrass;
    public float BurnTime = 2;
    public float GrowTime = 2;
    bool burnt = false;
    public override void OnPlayerTouch()
    {
        StartCoroutine(GrassProcess((pos) => Debug.Log(pos)));
    }
    public void Burn()
    {
        if (burnt)
            return;
        burnt = true;
        StartCoroutine(UnnamedFunc());
    }
    IEnumerator UnnamedFunc()
    {
        var fx = Instantiate(Fire, transform.position, Quaternion.identity);
        GetComponentInChildren<SpriteRenderer>().sprite = null;
        foreach (var t in Utility.Timer(1))
        {
            yield return null;
        }
        Destroy(fx);
        foreach (var t in Utility.Timer(GrowTime))
        {
            yield return null;
        }
        //GetComponentInChildren<SpriteRenderer>().sprite = NormalGrass;
        burnt = false;
    }

    TypedTile BFS(List<TypedTile> searched)
    {
        for (var i = 0; i < searched.Count; i++)
        {
            var tile = searched[i];
            var pos = TileMap.Instance.WorldToGrid(tile.transform.position);
            var next = TileMap.Instance.GetTile(pos + Vector2Int.up);
            if (next != null && !searched.Contains(next))
                searched.Add(next);

            next = TileMap.Instance.GetTile(pos + Vector2Int.left);
            if (next != null && !searched.Contains(next))
                searched.Add(next);

            next = TileMap.Instance.GetTile(pos + Vector2Int.down);
            if (next != null && !searched.Contains(next))
                searched.Add(next);

            next = TileMap.Instance.GetTile(pos + Vector2Int.right);
            if (next != null && !searched.Contains(next))
                searched.Add(next);
        }
        return searched[searched.Count - 1];
    }

    IEnumerable<TypedTile> BFSEnumerable(List<TypedTile> searched)
    {
        for (var i = 0; i < searched.Count; i++)
        {
            var tile = searched[i];
            var pos = TileMap.Instance.WorldToGrid(tile.transform.position);
            var next = TileMap.Instance.GetTile(pos + Vector2Int.up);
            if (next != null && !searched.Contains(next))
            {
                searched.Add(next);
                yield return next;
            }

            next = TileMap.Instance.GetTile(pos + Vector2Int.left);
            if (next != null && !searched.Contains(next))
            {
                searched.Add(next);
                yield return next;
            }

            next = TileMap.Instance.GetTile(pos + Vector2Int.down);
            if (next != null && !searched.Contains(next))
            {
                searched.Add(next);
                yield return next;
            }

            next = TileMap.Instance.GetTile(pos + Vector2Int.right);
            if (next != null && !searched.Contains(next))
            {
                searched.Add(next);
                yield return next;
            }
        }
    }

    public IEnumerator GrassProcess(Action<Vector2> setPosition)
    {
        GameObject.Find("Player").SetActive(false);
        var searched = new List<TypedTile>();
        searched.Add(GetComponent<TypedTile>());
        var tile = BFS(searched);
        setPosition?.Invoke(tile.transform.position);
        Burn();
        searched = new List<TypedTile>();
        searched.Add(GetComponent<TypedTile>());
        var i = 0;
        foreach(var _tile in BFSEnumerable(searched))
        {
            _tile?.GetComponent<GrassItem>()?.Burn();
            if (i % 2 == 1)
                yield return new WaitForSeconds(.01f);
            i++;
        }
    }

}
