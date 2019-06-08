using UnityEngine;
using System.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

public class GrassItem : MonoBehaviour
{
    public const float BurnTime = 1;
    public const float GrowTime = 10;
    public const float SpreadInterval = 0.1f;

    public GameObject Fire;
    public Sprite BurntGrass;
    public Sprite NormalGrass;
    public bool burnt = false;
    public void Burn(float startTime)
    {
        if (burnt)
            return;
        burnt = true;
        StartCoroutine(BurnProcess(startTime));
    }
    IEnumerator BurnProcess(float startTime)
    {
        var fx = Instantiate(Fire, transform.position, Quaternion.identity);
        GetComponentInChildren<SpriteRenderer>().sprite = null;
        foreach (var t in Utility.Timer(BurnTime))
        {
            yield return null;
        }
        Destroy(fx);
        while(Time.time < startTime + GrowTime)
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
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
                searched.Add(next);

            next = TileMap.Instance.GetTile(pos + Vector2Int.left);
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
                searched.Add(next);

            next = TileMap.Instance.GetTile(pos + Vector2Int.down);
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
                searched.Add(next);

            next = TileMap.Instance.GetTile(pos + Vector2Int.right);
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
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
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
            {
                searched.Add(next);
                yield return next;
            }

            next = TileMap.Instance.GetTile(pos + Vector2Int.left);
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
            {
                searched.Add(next);
                yield return next;
            }

            next = TileMap.Instance.GetTile(pos + Vector2Int.down);
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
            {
                searched.Add(next);
                yield return next;
            }

            next = TileMap.Instance.GetTile(pos + Vector2Int.right);
            if (next != null && !searched.Contains(next) && next.GetComponent<GrassItem>())
            {
                searched.Add(next);
                yield return next;
            }
        }
    }

    public IEnumerator GrassProcess(Action<Vector2> setPosition)
    {
        var startTime = Time.time;
        var searched = new List<TypedTile>();
        searched.Add(GetComponent<TypedTile>());
        var tile = BFS(searched);
        setPosition?.Invoke(tile.transform.position);
        Burn(startTime);
        searched = new List<TypedTile>();
        searched.Add(GetComponent<TypedTile>());
        var i = 0;
        foreach(var _tile in BFSEnumerable(searched))
        {
            _tile?.GetComponent<GrassItem>()?.Burn(startTime);
            if (i % 2 == 1)
                yield return new WaitForSeconds(SpreadInterval);
            i++;
        }
    }

}
