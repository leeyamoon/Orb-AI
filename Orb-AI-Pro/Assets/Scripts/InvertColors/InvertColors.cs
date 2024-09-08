using System.Collections.Generic;
using UnityEngine;



public class InvertColors : MonoBehaviour
{
    [Header("Player GameObject")] [SerializeField]
    private SpriteRenderer player;

    [Header("The Static Material In Game")] [SerializeField]
    private Material staticObjMaterial;
    
    [Header("Background Object")] [SerializeField]
    private SpriteRenderer BackGround;

    [Header("List Of All Trails In Game")] [SerializeField]
    private List<GameObject> trails;

    [Header("Toxic Area GameObject")] 
    [SerializeField] private Material toxic;

    [Header("List Of All Animations In Game")] [SerializeField]
    private List<SpriteRenderer> animations;
    
    [Header("The Player Material In Game")] [SerializeField]
    private Material playerMaterial;
    
    public void Invert()
    {
        PlayerInvert();
        RandomObjectsInvert();
        BackGroundInvert();
        TrailsInvert();
        ToxicAreaInvert();
        AnimationsInvert();
    }

    private void TrailsInvert()
    {
        if (trails[0].activeSelf)
        {
            trails[0].gameObject.SetActive(false);
            trails[1].gameObject.SetActive(true);
        }
        else
        {
            trails[0].gameObject.SetActive(true);
            trails[1].gameObject.SetActive(false);
        }
    }

    private void ToxicAreaInvert()
    {
        if (toxic.color == Color.black) toxic.color = Color.white;
        else if (toxic.color == Color.white) toxic.color = Color.black;

    }
    
    private void RandomObjectsInvert()
    {
        if (staticObjMaterial.color == Color.black) staticObjMaterial.color = Color.white;
        else if (staticObjMaterial.color == Color.white) staticObjMaterial.color = Color.black;
        if (playerMaterial.color == Color.black) playerMaterial.color = Color.white;
        else if (playerMaterial.color == Color.white) playerMaterial.color = Color.black;
    }

    private void BackGroundInvert()
    {
        if (BackGround.color == Color.black) BackGround.color = Color.white;
        else if (BackGround.color == Color.white) BackGround.color = Color.black;
    }

    private void PlayerInvert()
    {
        if (player.color == Color.black) player.color = Color.white;
        else if (player.color == Color.white) player.color = Color.black;
    }

    
    private void AnimationsInvert()
    {
        foreach (var gameObj in animations)
        {
            if (gameObj.color == Color.black) gameObj.color = Color.white;
            else if (gameObj.color == Color.white) gameObj.color = Color.black;
        }
    }

    public bool IsBackgroundWhite()
    {
        return BackGround.color == Color.white;
    }
    
}